using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    [Route("sysapi/[controller]/[action]")]
    [Authorize(Roles = "sys")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v2")]
    public class BaseController : ControllerBase
    {
        protected IClaimsAccessor _claimsAccessor;

        protected readonly IAdminLogService adminLogService;
        protected readonly ISysUserService adminService;
        protected readonly int adminId;
        protected readonly SysUser admin;

        public BaseController()
        {
            if (PubConstant.Accessor != null && PubConstant.Accessor.HttpContext != null)
            {
                string bearer = PubConstant.Accessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(bearer) && bearer.Contains("Bearer") && bearer.Contains("."))
                {
                    _claimsAccessor = new ClaimsAccessor(PubConstant.Accessor);
                    adminService = new SysUserService();

                    adminId = (int)_claimsAccessor.UserId;
                    admin = adminService.GetById(adminId);
                }
            }

        }

        #region 图片处理


        /// <summary>
        /// 获取图片路径
        /// </summary>
        /// <param name="imgPath"></param>
        /// <returns></returns>
        protected string GetUrl(string imgPath, int widthPic = 0, int heightPic = 0)
        {

            return WebFileHelper.GetUrl(imgPath, widthPic, heightPic);
        }

        /// <summary>
        /// 获取图片路径
        /// </summary>
        /// <param name="imgPath"></param>
        /// <returns></returns>
        protected List<string> GetListUrl(string imgPath, int widthPic = 0, int heightPic = 0)
        {
            return WebFileHelper.GetListUrl(imgPath, widthPic, heightPic);
        }

        /// <summary>
        /// 获取图片路径
        /// </summary>
        /// <param name="imgList"></param>
        /// <returns></returns>
        protected List<string> GetListUrl(List<string> imgList)
        {
            return WebFileHelper.GetListUrl(imgList);
        }
        #endregion
    }
}
