using StackExchange.Redis;
using System;

namespace YW.Common
{
    /// <summary>
    /// 基於 Redis 的「月度免費額度配額鎖」。
    /// 解決現有 RedisCacheHelper 沒有原子自增（Increment）的問題，
    /// 用底層 ConnectionMultiplexer 實現並發安全的計數器。
    ///
    /// 設計目標：防止騰訊雲等按量付費 API 超出免費額後「自動扣費」。
    /// 當月調用次數達到上限，系統自動「降級」為人工審核，不再調用雲 API。
    /// </summary>
    public class RedisQuotaHelper
    {
        private static readonly object _lock = new object();
        private static ConnectionMultiplexer _muxer = null;

        /// <summary>
        /// 懶加載單例連接（復用 appsettings 中既有的 Redis 連接串）
        /// </summary>
        private static ConnectionMultiplexer Muxer
        {
            get
            {
                if (_muxer == null || !_muxer.IsConnected)
                {
                    lock (_lock)
                    {
                        if (_muxer == null || !_muxer.IsConnected)
                        {
                            var conn = ConfigHelper.GetSectionValue("RedisConnectionStrings:Connection");
                            _muxer = ConnectionMultiplexer.Connect(conn ?? "127.0.0.1:6379,abortConnect=false");
                        }
                    }
                }
                return _muxer;
            }
        }

        /// <summary>
        /// 當前月份 key 前綴，例如 "quota:ImageTag:2026-08"
        /// </summary>
        private static string MonthKey(string quotaName)
        {
            return $"quota:{quotaName}:{DateTime.Now:yyyy-MM}";
        }

        /// <summary>
        /// 嘗試消耗一次額度。
        /// 返回 true 表示「額度內，允許調用雲 API」；
        /// 返回 false 表示「額度已用盡，應降級為人工審核，禁止調用」。
        /// 此方法具備原子性（Lua 腳本），高併發下不會超額。
        /// </summary>
        /// <param name="quotaName">配額名（如 ImageTag / ImagePorn / Ocr）</param>
        /// <param name="monthlyLimit">每月免費額度上限</param>
        public static bool TryConsume(string quotaName, long monthlyLimit)
        {
            try
            {
                var db = Muxer.GetDatabase();
                var key = MonthKey(quotaName);

                // 月內剩餘秒數，用於讓計數器在月底自動過期（不手動清理）
                var endOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
                    DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month), 23, 59, 59);
                var ttlSeconds = (long)(endOfMonth - DateTime.Now).TotalSeconds;

                // Lua：原子地 自增並判斷是否超過上限
                var script = @"
                    local current = redis.call('INCR', KEYS[1])
                    if current = 1 then
                        redis.call('EXPIRE', KEYS[1], ARGV[2])
                    end
                    if current > tonumber(ARGV[1]) then
                        return 0
                    end
                    return 1
                ";
                var result = (int)db.ScriptEvaluate(script, new RedisKey[] { key },
                    new RedisValue[] { monthlyLimit, ttlSeconds });
                return result == 1;
            }
            catch (Exception ex)
            {
                // Redis 不可用時，出於安全默認「禁止調用雲 API」（fail-closed），轉人工審核
                Console.WriteLine($"[RedisQuotaHelper] 配額檢查異常，已降級: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 讀取當前月份已用量
        /// </summary>
        public static long GetUsed(string quotaName)
        {
            try
            {
                var db = Muxer.GetDatabase();
                var val = db.StringGet(MonthKey(quotaName));
                return val.HasValue ? (long)val : 0;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 該配額本月是否已用盡
        /// </summary>
        public static bool IsExhausted(string quotaName, long monthlyLimit)
        {
            return GetUsed(quotaName) >= monthlyLimit;
        }
    }
}
