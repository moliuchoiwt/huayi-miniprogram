using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.ApiControllers
{
    [Authorize(Roles = "api")]
    public class UploadController : BaseController
    {
        public UploadController(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }
        [HttpGet, HttpPost]
        public async Task<ResultModel> APiUploadFile()
        {
            var result = new ResultModel();
            var files = Request?.Form?.Files;
            if (files == null || files.Count == 0)
            {
                result.msg = "请选择上传文件";
                return result;
            }

            // 上傳場景：avatar=帳戶相片 license=商戶執照 goods=商品圖 comment=評價圖（前端傳入）
            var auditType = Request?.Form?["auditType"]?.ToString() ?? "";

            var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".gif", ".jpg", ".jpeg", ".png", ".bmp",
                ".avi", ".rm", ".rmvb", ".flv", ".mpg",
                ".mov", ".mkv", ".mp4"
            };

            var year = DateTime.Now.ToString("yyyy");
            var day = DateTime.Now.ToString("MMdd");

            var file = files[0];

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || !allowed.Contains(ext))
            {
                result.msg = "不是允许上传的文件";
                return result;
            }

            var isVideo = file.ContentType?.IndexOf("video", StringComparison.OrdinalIgnoreCase) >= 0;
            var subFolder = isVideo ? "Video" : string.Empty;
            var relativeDir = Path.Combine("Upload", subFolder, year, day).TrimEnd(Path.DirectorySeparatorChar);
            var dirFull = Path.Combine(Directory.GetCurrentDirectory(), relativeDir);

            try
            {
                Directory.CreateDirectory(dirFull);
            }
            catch
            {
                result.msg = "创建文件目录失败";
                return result;
            }

            var fileId = DateTime.Now.ToString("yyyyMMddHHmmssffff") + "_" + Guid.NewGuid().ToString("n").Substring(0, 6);
            var fileName = fileId + ext;
            var fullPath = Path.Combine(dirFull, fileName);

            try
            {
                using (var fs = System.IO.File.Create(fullPath))
                {
                    await file.CopyToAsync(fs);
                    await fs.FlushAsync();
                }
            }
            catch
            {
                result.msg = "文件保存失败";
                return result;
            }

            var fileUrl = "/" + relativeDir.Replace("\\", "/").TrimEnd('/') + "/" + fileName;

            // ===== 圖片篩查（需求2）：上傳後自動校驗 =====
            if (!isVideo && !string.IsNullOrEmpty(auditType))
            {
                try
                {
                    var audit = new YW.Service.ImageAudit.ImageAuditService();
                    YW.Service.ImageAudit.AuditResult ar = null;
                    string scene = "";
                    if (auditType == "avatar") { ar = await audit.CheckPorn(fileUrl); scene = "帳戶相片(不含色情暴力)"; }
                    else if (auditType == "license") { ar = await audit.CheckIsBusinessLicense(fileUrl); scene = "商戶登記執照"; }
                    else if (auditType == "goods") { ar = await audit.CheckContainsPlant(fileUrl); scene = "商品圖片(含植物)"; }
                    else if (auditType == "comment") { ar = await audit.CheckContainsPlant(fileUrl); scene = "評價圖片(含植物)"; }

                    if (ar != null)
                    {
                        if (!ar.Passed && !ar.NeedManualReview)
                        {
                            // 篩查不通過：返回失敗 + 紅字提示，前端不上傳成功
                            result.msg = ar.RejectMessage; // "*请上载正确照片。"
                            result.code = (int)ResultEnum.fail;
                            // 通知管理員
                            _ = YW.Service.ImageAudit.ImageAuditService.NotifyAdminAsync(scene, fileUrl, ar.RejectMessage);
                            // 刪除不合規圖片
                            try { if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath); } catch { }
                            return result;
                        }
                        if (ar.NeedManualReview)
                        {
                            // 降級：額度用盡或雲API失敗 → 轉人工審核，正常返回（不阻塞）
                            // 標記圖片需人工審核（用 session/後續提交時處理）
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 篩查異常不影響上傳主流程
                    Console.WriteLine($"[Upload] 圖片篩查異常: {ex.Message}");
                }
            }

            result.data = fileUrl;
            result.msg = "success";
            result.code = (int)ResultEnum.success;
            return result;
        }
    }
}
