using System.Threading.Tasks;

namespace YW.Service.ImageAudit
{
    /// <summary>
    /// 圖片篩查業務包裝：封裝「配額守衛 + 騰訊雲調用 + 失敗降級」。
    /// 這是一個示範/模板，展示如何把 QuotaGuard 插入原有上傳流程。
    /// 實際接騰訊雲時，把 TODO 處替換成真實 API 調用即可（項目無騰訊雲 SDK，需另外引入）。
    /// </summary>
    public class ImageAuditService
    {
        private readonly QuotaGuard _guard = new QuotaGuard();

        /// <summary>
        /// 篩查圖片是否「含植物」（商品圖、評價圖要求）
        /// </summary>
        /// <returns>true=確認含植物；false=不含 / 無法確認（降級人工審核）</returns>
        public async Task<AuditResult> CheckContainsPlant(string imageUrl)
        {
            // 1. 先過配額鎖
            if (!_guard.ShouldCallCloud(QuotaGuard.QuotaImageTag))
            {
                // 開關關閉或額度用盡 → 降級人工審核，不調雲 API、不扣費
                return AuditResult.FallbackToManual("圖像標籤額度不足，轉人工審核");
            }

            // 2. 只調用一次雲 API
            try
            {
                bool containsPlant = await CallTencentImageTagAsync(imageUrl);
                return containsPlant
                    ? AuditResult.Pass()
                    : AuditResult.Fail("*请上载正确照片。"); // 紅字提示文案
            }
            catch
            {
                // 3. 雲 API 報錯 → 降級人工審核，不重試、不燒錢
                return AuditResult.FallbackToManual("雲 API 調用失敗，轉人工審核");
            }
        }

        /// <summary>
        /// 篩查圖片是否「為中國/香港營業執照」
        /// </summary>
        public async Task<AuditResult> CheckIsBusinessLicense(string imageUrl)
        {
            if (!_guard.ShouldCallCloud(QuotaGuard.QuotaOcr))
                return AuditResult.FallbackToManual("OCR 額度不足，轉人工審核");

            try
            {
                bool isLicense = await CallTencentOcrAsync(imageUrl);
                return isLicense
                    ? AuditResult.Pass()
                    : AuditResult.Fail("*请上载正确照片。");
            }
            catch
            {
                return AuditResult.FallbackToManual("雲 API 調用失敗，轉人工審核");
            }
        }

        /// <summary>
        /// 篩查圖片是否含色情/暴力/罪惡
        /// </summary>
        public async Task<AuditResult> CheckPorn(string imageUrl)
        {
            if (!_guard.ShouldCallCloud(QuotaGuard.QuotaPorn))
                return AuditResult.FallbackToManual("鉴黄額度不足，轉人工審核");

            try
            {
                bool isSafe = await CallTencentPornDetectAsync(imageUrl);
                return isSafe
                    ? AuditResult.Pass()
                    : AuditResult.Fail("*请上载正确照片。");
            }
            catch
            {
                return AuditResult.FallbackToManual("雲 API 調用失敗，轉人工審核");
            }
        }

        // ===== 以下為 TODO：接騰訊雲 SDK 的真實調用（項目目前無 SDK，需 NuGet 引入） =====
        private Task<bool> CallTencentImageTagAsync(string imageUrl)
            => throw new System.NotImplementedException("接騰訊雲圖像標籤 SDK（每月 1000 次免費）");

        private Task<bool> CallTencentOcrAsync(string imageUrl)
            => throw new System.NotImplementedException("接騰訊雲 OCR SDK，識別「營業執照/統一社會信用代碼」關鍵字");

        private Task<bool> CallTencentPornDetectAsync(string imageUrl)
            => throw new System.NotImplementedException("接騰訊雲圖片內容安全 SDK（鉴黄）");
    }

    /// <summary>
    /// 篩查結果
    /// </summary>
    public class AuditResult
    {
        /// <summary>通過</summary>
        public bool Passed { get; private set; }
        /// <summary>不通過，給用戶的紅字提示</summary>
        public string RejectMessage { get; private set; }
        /// <summary>降級為人工審核（未調雲 API / 調用失敗）</summary>
        public bool NeedManualReview { get; private set; }
        /// <summary>降級原因（日誌用）</summary>
        public string FallbackReason { get; private set; }

        public static AuditResult Pass() => new AuditResult { Passed = true };
        public static AuditResult Fail(string msg) => new AuditResult { Passed = false, RejectMessage = msg };
        public static AuditResult FallbackToManual(string reason) => new AuditResult { NeedManualReview = true, FallbackReason = reason };
    }
}
