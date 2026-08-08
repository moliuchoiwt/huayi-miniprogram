using Microsoft.AspNetCore.Mvc;
using Senparc.CO2NET.AspNet.HttpUtility;
using System.Threading.Tasks;
using YW.Service.WeChat;

namespace YW.WebApi.ApiControllers
{
    /// <summary>
    /// 资讯
    /// </summary>
    public class OtherController : BaseController
    {

        /// <summary>
        /// 配置消息
        /// </summary>
        [HttpPost]
        public ResultModel GetConfig(QueryModel view)
        {
            var res = new ResultModel();
            var type = 0;
            if (view.queryType.HasValue) type = view.queryType.Value;

            switch (type)
            {
                case 1:  //隐私条例
                    res.data = getContent(PubConstant.Config.PrivacyInfo);
                    break;
                case 2: //接单须知
                    res.data = getContent(PubConstant.Config.richText1);
                    break;
                case 4: //提现说明
                    res.data = getContent(PubConstant.Config.UserWithdrawIntro);
                    break;
                default:
                    var MpWxOpenCheck = PubConstant.Config.MpWxOpenCheck;
                    if (!string.IsNullOrWhiteSpace(MpWxOpenVersion) && PubConstant.Config.MpWxOpenVersion != MpWxOpenVersion)
                    {
                        MpWxOpenCheck = false;
                    }
                    //平台
                    res.data = new
                    {
                        PubConstant.Config.SiteName,
                        PubConstant.Config.DomianStaticName,
                        PubConstant.Config.CustomerMobile,
                        SiteLogo = GetFileUrl(PubConstant.Config.SiteLogo),//网站logo
                        videoUrl = GetFileUrl(PubConstant.Config.videoUrl),
                        PubConstant.Config.orderMinPrice,
                        PubConstant.Config.textContents,
                        MpWxOpenCheck
                    };
                    break;
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            return res;
        }

        #region 微信消息接口对接;/WxRequest

        /// <summary>
        /// 接口认证
        /// </summary>
        /// <param name="echostr"></param>
        /// <param name="signature"></param>
        /// <param name="timestamp"></param>
        /// <param name="nonce"></param>
        /// <returns></returns>
        [HttpGet]
        public string WxRequest(string echostr, string signature, string timestamp, string nonce)
        {
            var responseDoc = MpClient.Check(signature, timestamp, nonce, echostr);
            //LogHelper.Info("微信公众号建立连接消息:" + responseDoc);
            return responseDoc;
        }
        /// <summary>
        /// 接收客服消息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> WxRequest()
        {
            //LogHelper.Info("接收客服消息" + JsonConvert.SerializeObject(Request.QueryString.Value));
            var s = await Request.GetRequestMemoryStreamAsync();
            var signature = Request.Query["signature"].ToString();
            var timestamp = Request.Query["timestamp"].ToString();
            var nonce = Request.Query["nonce"].ToString();
            var msgSignature = Request.Query["msg_signature"].ToString();
            var responseDoc = await MpClient.WxRequest(s, signature, timestamp, nonce, msgSignature);
            return responseDoc;
        }
        #endregion


        //[HttpPost]
        //public ResultModel testApi()
        //{
        //    var res = new ResultModel { code = 200, msg = "" };
        //    _userCouponService.TimedTaskFun();
        //    return res;
        //}

    }
}
