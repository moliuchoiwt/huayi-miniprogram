// System, System.Collections.Generic, System.IO, System.Net.Http,
// System.Threading.Tasks, Newtonsoft.Json, YW.Common 已由 GlobalUsings.cs 覆蓋
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace YW.Service.ImageAudit
{
    /// <summary>
    /// 圖片篩查業務服務：封裝「配額守衛 + 騰訊雲調用 + 失敗降級 + 通知管理員」。
    /// 三項雲服務：
    ///   1. 圖片內容安全 IMS（鉴黄/暴力/罪惡）—— TencentCloud.CMS
    ///   2. 圖像標籤 Tiia（含植物判斷）      —— TencentCloud.Tiia
    ///   3. OCR（營業執照關鍵字識別）        —— TencentCloud.Ocr
    /// 調用失敗 / 配額用盡 → 自動降級「人工審核」，不扣費、不阻塞上傳。
    /// 篩查不通過 → 返回紅字提示文案「*请上载正确照片。」，並通知管理員。
    /// </summary>
    public class ImageAuditService
    {
        private readonly QuotaGuard _guard = new QuotaGuard();
        private static readonly HttpClient _http = new HttpClient();

        private static string SecretId => YW.Common.ConfigHelper.GetSectionValue("TencentCloud:SecretId");
        private static string SecretKey => YW.Common.ConfigHelper.GetSectionValue("TencentCloud:SecretKey");
        private static string AdminEmail =>
            YW.Common.ConfigHelper.GetSectionValue("EmailSetting:AdminTo") ?? "studioofjoyhk@gmail.com";

        #region 對外方法

        /// <summary>
        /// 篩查圖片是否「含植物」（商品圖、評價圖要求）
        /// </summary>
        public async Task<AuditResult> CheckContainsPlant(string imageUrl)
        {
            if (!_guard.ShouldCallCloud(QuotaGuard.QuotaImageTag))
                return AuditResult.FallbackToManual("圖像標籤額度不足，轉人工審核");
            try
            {
                bool contains = await TiiaDetectPlantAsync(imageUrl);
                return contains ? AuditResult.Pass() : AuditResult.Fail("*请上载正确照片。");
            }
            catch (Exception ex)
            {
                return AuditResult.FallbackToManual("雲 API 調用失敗：" + ex.Message);
            }
        }

        /// <summary>
        /// 篩查圖片是否「為中國/香港商業登記/營業執照」
        /// </summary>
        public async Task<AuditResult> CheckIsBusinessLicense(string imageUrl)
        {
            if (!_guard.ShouldCallCloud(QuotaGuard.QuotaOcr))
                return AuditResult.FallbackToManual("OCR 額度不足，轉人工審核");
            try
            {
                bool isLicense = await OcrDetectLicenseAsync(imageUrl);
                return isLicense ? AuditResult.Pass() : AuditResult.Fail("*请上载正确照片。");
            }
            catch (Exception ex)
            {
                return AuditResult.FallbackToManual("雲 API 調用失敗：" + ex.Message);
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
                bool safe = await ImsDetectSafeAsync(imageUrl);
                return safe ? AuditResult.Pass() : AuditResult.Fail("*请上载正确照片。");
            }
            catch (Exception ex)
            {
                return AuditResult.FallbackToManual("雲 API 調用失敗：" + ex.Message);
            }
        }

        /// <summary>
        /// 統一通知管理員（圖片篩查不通過時）
        /// </summary>
        public static async Task NotifyAdminAsync(string scene, string imageUrl, string reason)
        {
            try
            {
                var body = $@"
                    <h3>【華藝】圖片篩查不通過通知</h3>
                    <p>管理員你好，</p>
                    <p>系統在用戶上傳圖片時發現以下情況，請人工覆核：</p>
                    <ul>
                        <li>場景：{scene}</li>
                        <li>原因：{reason}</li>
                        <li>圖片：{(string.IsNullOrEmpty(imageUrl) ? "(無)" : imageUrl)}</li>
                        <li>時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</li>
                    </ul>
                    <p>如圖片確實不合規，請在後台處理；如係誤判，可忽略。</p>
                ";
                await YW.Common.EmailClient.SendAsync(AdminEmail, $"【華藝】圖片篩查不通過：{scene}", body);
            }
            catch { }
        }

        #endregion

        #region 騰訊雲真實調用（TC3-HMAC 簽名，免 SDK 依賴）

        private async Task<bool> ImsDetectSafeAsync(string imageUrl)
        {
            // 圖片內容安全：調用後若無色情/暴力標籤則安全
            var resp = await TencentRequest("cms.tencentcloudapi.com", "ImageModeration", new JObject
            {
                ["FileUrl"] = ToAbsoluteUrl(imageUrl)
            });
            // 騰訊雲 IMS 返回 Label 為 Normal 表示正常
            var label = resp?["Response"]?["Label"]?.ToString();
            return "Normal".Equals(label, StringComparison.OrdinalIgnoreCase)
                || "Pass".Equals(label, StringComparison.OrdinalIgnoreCase)
                || string.IsNullOrEmpty(label);
        }

        private async Task<bool> TiiaDetectPlantAsync(string imageUrl)
        {
            var resp = await TencentRequest("tiia.tencentcloudapi.com", "DetectLabel", new JObject
            {
                ["ImageUrl"] = ToAbsoluteUrl(imageUrl)
            });
            var labels = resp?["Response"]?["Labels"] as JArray;
            if (labels == null) return false;
            foreach (var l in labels)
            {
                var name = (l["Name"]?.ToString() ?? "").ToLower();
                // 含植物相關標籤
                if (name.Contains("植物") || name.Contains("花") || name.Contains("plant")
                    || name.Contains("flower") || name.Contains("草") || name.Contains("tree")
                    || name.Contains("葉") || name.Contains("果"))
                    return true;
            }
            return false;
        }

        private async Task<bool> OcrDetectLicenseAsync(string imageUrl)
        {
            var resp = await TencentRequest("ocr.tencentcloudapi.com", "BizLicenseOCR", new JObject
            {
                ["ImageUrl"] = ToAbsoluteUrl(imageUrl)
            });
            // 營業執照 OCR 成功且有「統一社會信用代碼」或「登記機關」等字段
            var r = resp?["Response"];
            if (r == null) return false;
            var txt = r.ToString();
            return txt.Contains("統一社會信用代碼") || txt.Contains("營業執照")
                || txt.Contains("註冊號碼") || txt.Contains("商業登記") // 香港商業登記
                || txt.Contains("登記機關") || txt.Contains("法定代表人");
        }

        /// <summary>
        /// 騰訊雲 TC3-HMAC-SHA256 簽名調用（通用）
        /// </summary>
        private async Task<JObject> TencentRequest(string host, string action, JObject param)
        {
            var service = host.Split('.')[0];
            var version = service == "cms" ? "2020-12-29" : (service == "tiia" ? "2019-05-29" : "2018-11-19");
            var payload = param.ToString(Formatting.None);
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
            var timestamp = ToUnix(DateTime.UtcNow);

            // 1. 拼接規範請求
            var hashedPayload = Sha256Hex(payload);
            var canonicalReq = $"POST\n/\n\ncontent-type:application/json; charset=utf-8\nhost:{host}\n\ncontent-type;host\n{hashedPayload}";

            // 2. 拼接待簽字符串
            var credScope = $"{date}/{service}/tc3_request";
            var stringToSign = $"TC3-HMAC-SHA256\n{timestamp}\n{credScope}\n{Sha256Hex(canonicalReq)}";

            // 3. 計算簽名
            var secretDate = HmacSha256(Encoding.UTF8.GetBytes("TC3" + SecretKey), date);
            var secretService = HmacSha256(secretDate, service);
            var secretSigning = HmacSha256(secretService, "tc3_request");
            var signature = HmacSha256Hex(secretSigning, stringToSign);

            // 4. 組裝 Authorization
            var authorization = $"TC3-HMAC-SHA256 Credential={SecretId}/{credScope}, SignedHeaders=content-type;host, Signature={signature}";

            var req = new HttpRequestMessage(HttpMethod.Post, $"https://{host}/");
            req.Headers.Add("Authorization", authorization);
            req.Headers.Add("Host", host);
            req.Headers.Add("X-TC-Action", action);
            req.Headers.Add("X-TC-Version", version);
            req.Headers.Add("X-TC-Timestamp", timestamp.ToString());
            req.Content = new StringContent(payload, Encoding.UTF8, "application/json");

            var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();
            return JObject.Parse(body);
        }

        private static string ToAbsoluteUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return url;
            if (url.StartsWith("http")) return url;
            var domain = YW.Common.ConfigHelper.GetSectionValue("SiteSetting:Domain") ?? "";
            return domain.TrimEnd('/') + (url.StartsWith("/") ? url : "/" + url);
        }

        private static long ToUnix(DateTime dt) =>
            (long)(dt - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        private static string Sha256Hex(string s)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        private static byte[] HmacSha256(byte[] key, string data) =>
            new HMACSHA256(key).ComputeHash(Encoding.UTF8.GetBytes(data));

        private static string HmacSha256Hex(byte[] key, string data)
        {
            var bytes = HmacSha256(key, data);
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        #endregion
    }

    /// <summary>
    /// 篩查結果
    /// </summary>
    public class AuditResult
    {
        public bool Passed { get; private set; }
        public string RejectMessage { get; private set; }
        public bool NeedManualReview { get; private set; }
        public string FallbackReason { get; private set; }

        public static AuditResult Pass() => new AuditResult { Passed = true };
        public static AuditResult Fail(string msg) => new AuditResult { Passed = false, RejectMessage = msg };
        public static AuditResult FallbackToManual(string reason) => new AuditResult { NeedManualReview = true, FallbackReason = reason };
    }
}
