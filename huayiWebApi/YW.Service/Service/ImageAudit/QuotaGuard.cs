using System;
using System.Threading.Tasks;

namespace YW.Service.ImageAudit
{
    /// <summary>
    /// 圖片篩查「防自動扣費」守衛。
    ///
    /// 職責：
    /// 1. 總開關 ImageAudit:Enabled —— false 時所有雲 API 一律不調用（0 成本，全走人工審核）。
    /// 2. 月度免費額度鎖 —— 每種雲 API 有獨立配額，用盡後自動降級為人工審核。
    /// 3. 額度用完通知 —— 當某配額本月首次用盡，自動發郵件通知管理員 studioofjoyhk@gmail.com。
    /// 4. 雲 API 報錯也降級 —— 調用失敗當「待人工審核」，不阻塞用戶上傳、不重試燒錢。
    ///
    /// 用法（在原有上傳/發佈邏輯前呼叫）：
    ///   var guard = new QuotaGuard();
    ///   if (guard.ShouldCallCloud(QuotaGuard.QuotaImageTag))
    ///   {
    ///       // 調用騰訊雲圖像標籤...（只調一次）
    ///   }
    ///   else
    ///   {
    ///       // 降級：標記圖片為「待人工審核」，不再調雲 API
    ///   }
    /// </summary>
    public class QuotaGuard
    {
        // 三種配額名稱（對應騰訊雲三項服務）
        public const string QuotaPorn = "ImagePorn";   // 圖片內容安全（鉴黄）
        public const string QuotaImageTag = "ImageTag"; // 圖像標籤（含植物判斷）
        public const string QuotaOcr = "Ocr";           // OCR（執照關鍵字）

        private static string AdminEmail =>
            YW.Common.ConfigHelper.GetSectionValue("EmailSetting:AdminTo") ?? "studioofjoyhk@gmail.com";

        /// <summary>
        /// 總開關：是否啟用雲 API 圖片篩查。false = 全部降級人工審核。
        /// </summary>
        public static bool Enabled
        {
            get
            {
                var v = YW.Common.ConfigHelper.GetSectionValue("ImageAudit:Enabled");
                return !"false".Equals(v, StringComparison.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// 取得某配額的月度上限（從 appsettings 讀，缺省給保守默認值）
        /// </summary>
        private static long GetLimit(string quotaName)
        {
            var v = YW.Common.ConfigHelper.GetSectionValue($"ImageAudit:Quota:{quotaName}");
            return long.TryParse(v, out var n) && n > 0 ? n : DefaultLimit(quotaName);
        }

        private static long DefaultLimit(string quotaName) => quotaName switch
        {
            QuotaPorn => 5000,      // 鉴黄：騰訊雲試用包通常數千張（需控制台領取，非每月續）
            QuotaImageTag => 1000,  // 圖像標籤：每月 1000 次免費（用戶提供情報）
            QuotaOcr => 1000,       // OCR：保守默認
            _ => 1000
        };

        /// <summary>
        /// 是否應該調用雲 API（含配額消耗）。
        /// 返回 true：額度內，可調用（請只調用一次）。
        /// 返回 false：總開關關閉 / 額度用盡 / Redis 異常 —— 請降級人工審核。
        /// 當額度「首次用盡」會觸發管理員郵件通知（每個月只通知一次）。
        /// </summary>
        public bool ShouldCallCloud(string quotaName)
        {
            if (!Enabled)
                return false;

            bool allowed = YW.Common.RedisQuotaHelper.TryConsume(quotaName, GetLimit(quotaName));

            if (!allowed)
            {
                // 額度用盡：檢查本月是否已通知過，未通知則發郵件
                NotifyAdminIfExhausted(quotaName);
            }
            return allowed;
        }

        /// <summary>
        /// 雲 API 調用失敗時的降級處理標記（語義化方法，方便業務代碼閱讀）
        /// </summary>
        public static bool ShouldFallbackToManual => true;

        /// <summary>
        /// 額度首次用盡時，發郵件通知管理員。用 Redis 標記「本月已通知」，避免重複轟炸。
        /// </summary>
        private static void NotifyAdminIfExhausted(string quotaName)
        {
            try
            {
                var notifiedKey = $"quota:notified:{quotaName}:{DateTime.Now:yyyy-MM}";
                // 用現有 Redis 簡易讀寫判斷是否已通知過
                var already = YW.Common.RedisCacheHelper.GetStringValue(notifiedKey);
                if (!string.IsNullOrEmpty(already))
                    return;

                YW.Common.RedisCacheHelper.SetStringValue(notifiedKey, "1", 24 * 32); // 約一個月過期

                var used = YW.Common.RedisQuotaHelper.GetUsed(quotaName);
                var limit = GetLimit(quotaName);
                var quotaLabel = quotaName switch
                {
                    QuotaPorn => "圖片內容安全（鉴黄）",
                    QuotaImageTag => "圖像標籤（含植物識別）",
                    QuotaOcr => "OCR（營業執照識別）",
                    _ => quotaName
                };

                var body = $@"
                    <h3>【華藝】騰訊雲免費額度已用盡通知</h3>
                    <p>管理員你好，</p>
                    <p>以下雲 API 的<b>本月免費調用額度已用完</b>，系統已自動降級為「人工審核」模式，<b>不會再產生扣費</b>：</p>
                    <ul>
                        <li>服務：{quotaLabel}</li>
                        <li>配額名稱：{quotaName}</li>
                        <li>本月已用：{used} 次</li>
                        <li>月度上限：{limit} 次</li>
                        <li>發生時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</li>
                    </ul>
                    <p>如需恢復自動篩查，請：</p>
                    <ol>
                        <li>前往騰訊雲購買資源包或充值；</li>
                        <li>在 appsettings.json 調高 <code>ImageAudit:Quota:{quotaName}</code> 上限；</li>
                        <li>或保持人工審核（0 成本），待下月額度重置。</li>
                    </ol>
                    <p style='color:#888;'>此郵件由系統自動發出，無需回覆。</p>
                ";
                _ = YW.Common.EmailClient.SendAsync(AdminEmail, $"【華藝】免費額度用盡：{quotaLabel}", body);
            }
            catch
            {
                // 通知失敗不影響主流程
            }
        }
    }
}
