using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.ApiControllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1")]
    public class BaseController : ControllerBase
    {

        protected IClaimsAccessor _claimsAccessor;
        protected readonly IUserInfoService userService;
        protected readonly UserInfo user;
        protected string MpWxOpenVersion = "";
        public BaseController()
        {
            if (PubConstant.Accessor != null && PubConstant.Accessor.HttpContext != null)
            {
                string bearer = PubConstant.Accessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(bearer) && bearer.Contains("Bearer") && bearer.Contains("."))
                {
                    _claimsAccessor = new ClaimsAccessor(PubConstant.Accessor);
                    userService = new UserInfoService(_claimsAccessor);

                    user = userService.GetById((int)_claimsAccessor.UserId);
                }
                MpWxOpenVersion = PubConstant.Accessor.HttpContext.Request.Headers["version"].FirstOrDefault();
            }

        }

        #region 图片处理


        /// <summary>
        /// 获取图片路径
        /// </summary>
        /// <param name="imgPath"></param>
        /// <returns></returns>
        protected string GetFileUrl(string imgPath, int widthPic = 0, int heightPic = 0)
        {
            return WebFileHelper.GetUrl(imgPath, widthPic, heightPic);
        }

        /// <summary>
        /// 获取图片路径
        /// </summary>
        /// <param name="imgPath"></param>
        /// <returns></returns>
        protected List<string> GetFileListUrl(string imgPath, int widthPic = 0, int heightPic = 0)
        {
            return WebFileHelper.GetListUrl(imgPath, widthPic, heightPic);
        }

        /// <summary>
        /// 获取图片路径
        /// </summary>
        /// <param name="imgPath"></param>
        /// <returns></returns>
        protected List<string> GetFileListUrl(List<string> imgList)
        {
            return WebFileHelper.GetListUrl(imgList);
        }
        #endregion

        #region 内容详情中图片处理
        /// <summary>
        /// 内容详情中图片处理
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        protected string getContent(string content)
        {

            return WebFileHelper.getContent(content);
        }
        #endregion
    }
}
