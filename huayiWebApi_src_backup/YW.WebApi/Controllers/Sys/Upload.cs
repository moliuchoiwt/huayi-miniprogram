using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{

    public class UploadController : BaseController
    {
        public UploadController(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }
        [HttpGet, HttpPost]
        public async Task<ResultModel> WebUploadFile()
        {
            var result = new ResultModel();

            var files = Request?.Form?.Files;
            if (files == null || files.Count == 0)
            {
                result.msg = "请选择上传文件";
                return result;
            }

            // 允许的扩展名集合，使用 HashSet 提升查找性能
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

            // 相对目录和物理目录
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

            // 使用时间戳 + 随机字符串减少冲突，并保留扩展名
            var fileId = DateTime.Now.ToString("yyyyMMddHHmmssffff") + "_" + Guid.NewGuid().ToString("n").Substring(0, 6);
            var fileName = fileId + ext;
            var fullPath = Path.Combine(dirFull, fileName);

            // 异步写入文件
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

        [HttpPost]
        public ResultModel WebDelFile([FromForm] string fileNo, [FromForm] string url)
        {
            var res = new ResultModel();
            if (string.IsNullOrWhiteSpace(fileNo))
            {
                res.msg = "文件编号不能为空";
                return res;
            }
            if (string.IsNullOrWhiteSpace(url))
            {
                res.msg = "文件地址不能为空";
                return res;
            }
            var pathUrl = CommonHelper.GetMapPath(url);
            if (DirFile.IsExistFile(pathUrl)) DirFile.DeleteFile(pathUrl);
            var isok = true;
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = isok ? "OK" : "删除失败";
            return res;
        }

    }
}
