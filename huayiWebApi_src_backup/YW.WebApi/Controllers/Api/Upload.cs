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

            result.data = fileUrl;
            result.msg = "success";
            result.code = (int)ResultEnum.success;
            return result;
        }
    }
}
