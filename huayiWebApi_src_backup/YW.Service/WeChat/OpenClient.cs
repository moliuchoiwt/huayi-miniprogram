using Senparc.Weixin.Entities;
using Senparc.Weixin.Entities.TemplateMessage;
using Senparc.Weixin.WxOpen.AdvancedAPIs;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Sec;
using Senparc.Weixin.WxOpen.AdvancedAPIs.Sns;
using Senparc.Weixin.WxOpen.AdvancedAPIs.WxApp;
using Senparc.Weixin.WxOpen.AdvancedAPIs.WxApp.Business.JsonResult;
using Senparc.Weixin.WxOpen.Helpers;
using System.IO;

namespace YW.Service.WeChat
{
    /// <summary>
    /// 小程序帮助类
    /// </summary>
    public class OpenClient
    {
        #region [变量]

        /// <summary>
        /// 小程序AppId
        /// </summary>
        private static string AppId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenSetting.WxOpenAppId;
        /// <summary>
        ///小程序Secret
        /// </summary>
        private static string Secret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenSetting.WxOpenAppSecret;

        #endregion

        #region 通过code获取openid，session_key

        /// <summary>
        /// 通过code获取openid，session_key
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static async Task<JsCode2JsonResult> JsCode2Json(string code)
        {
            var result = await SnsApi.JsCode2JsonAsync(AppId, Secret, code);
            return result;
        }
        #endregion

        #region 通过code获取手机号

        /// <summary>
        /// 通过code 获取手机号
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static async Task<GetUserPhoneNumberJsonResult> GetUserPhoneNumberAsync(string code)
        {
            var result = await BusinessApi.GetUserPhoneNumberAsync(AppId, code);
            return result;
        }

        #endregion

        #region session_key 合法性校验

        /// <summary>
        /// session_key 合法性校验
        /// https://mp.weixin.qq.com/debug/wxagame/dev/tutorial/http-signature.html
        /// </summary>
        /// <param name="openId">用户唯一标识符</param>
        /// <param name="sessionKey">用户登录态签名</param>
        /// <param name="buffer">托管数据，类型为字符串，长度不超过1000字节（官方文档没有提供说明，可留空）</param>
        /// <returns></returns>
        public static async Task<string> CheckSession(string openId, string sessionKey, string buffer = null)
        {
            var result = await WxAppApi.CheckSessionAsync(AppId, openId, sessionKey, buffer);
            return result.errmsg;
        }
        #endregion

        #region 解密UserInfo消息（通过SessionId获取）

        /// <summary>
        /// 解密UserInfo消息（通过SessionId获取）
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="encryptedData"></param>
        /// <param name="iv"></param>
        /// <exception cref="WxOpenException">当SessionId或SessionKey无效时抛出异常</exception>
        /// <returns></returns>
        public static object DecodeUserInfoBySessionId(string sessionId, string encryptedData, string iv)
        {
            var result = EncryptHelper.DecodeUserInfoBySessionId(sessionId, encryptedData, iv);
            return result;
        }


        #endregion

        #region 违法违规内容检查


        /// <summary>
        /// 检查一段文本是否含有违法违规内容
        /// </summary>
        /// <param name="content">内容</param>
        /// <param name="scene">场景枚举值（1 资料；2 评论；3 论坛；4 社交日志）</param>
        public static async Task<bool> MsgSecCheck(string content, int scene, string openId)
        {

            var isok = false;
            var result = await WxAppApi.MsgSecCheckAsync(AppId, content, 2, scene, openId);
            //LogHelper.Info("检查一段文本是否含有违法违规内容:" + JsonConvert.SerializeObject(result));
            if (result.ErrorCodeValue == 0) isok = true;
            return isok;
        }

        /// <summary>
        /// 检查图片是否含有违法违规内容
        /// </summary>
        /// <param name="filepath">图片路径</param>
        public static async Task<bool> ImgSecCheck(string filepath)
        {

            var isok = false;
            var result = await WxAppApi.ImgSecCheckAsync(AppId, filepath);
            if (result.ErrorCodeValue == 0) isok = true;
            return isok;
        }
        #endregion




        #region 小程序二维码


        /// <summary>
        /// 获取小程序页面的二维码
        /// </summary>
        /// <param name="stream">储存小程序二维码的流</param>
        /// <param name="path">不能为空，最大长度 128 字节（如：pages/index?query=1。注：pages/index 需要在 app.json 的 pages 中定义）</param>
        public static async Task<WxJsonResult> GetWxQrCode(Stream stream, string path, int widtd = 430)
        {

            var result = await WxAppApi.CreateWxQrCodeAsync(AppId, stream, path, widtd);
            return result;

        }
        #endregion

        #region 小程序码

        /// <summary>
        /// 获取小程序页面的小程序码
        /// </summary>
        /// <param name="accessTokenOrAppId">AccessToken或AppId（推荐使用AppId，需要先注册）</param>
        /// <param name="filePath">储存图片的物理路径</param>
        /// <param name="path">不能为空，最大长度 128 字节（如：pages/index?query=1。注：pages/index 需要在 app.json 的 pages 中定义）</param>
        /// <param name="width">二维码的宽度</param>
        /// <returns></returns>
        public static async Task<WxJsonResult> GetWxaCode(string filePath, string path, int width = 430)
        {
            using (var ms = new MemoryStream())
            {
                var result = await WxAppApi.GetWxaCodeAsync(AppId, ms, path, width);
                ms.Seek(0, SeekOrigin.Begin);
                //储存图片
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                using (var fs = new FileStream(filePath, FileMode.CreateNew))
                {
                    ms.CopyTo(fs);
                    fs.Flush();
                }
                return result;
            }
        }



        /// <summary>
        /// 获取小程序页面的小程序码
        /// </summary>
        /// <param name="stream">储存小程序码的流</param>
        /// <param name="path">不能为空，最大长度 128 字节（如：pages/index?query=1。注：pages/index 需要在 app.json 的 pages 中定义）</param>
        public static async Task<WxJsonResult> GetWxCode(Stream stream, string path, int widtd = 430)
        {

            var result = await WxAppApi.GetWxaCodeAsync(AppId, stream, path, widtd);
            return result;

        }


        /// <summary>
        /// 获取小程序页面的小程序码[适用于需要的码数量极多，或仅临时使用的业务场景]
        /// </summary>
        /// <param name="stream">储存小程序码的流</param>
        /// <param name="path">不能为空，最大长度 128 字节（如：pages/index?query=1。注：pages/index 需要在 app.json 的 pages 中定义）</param>
        public static async Task<MemoryStream> GetWxaCodeUnlimit(string scene, string path, bool check_path = true, string env_version = "release", int width = 430)
        {
            var ms = new MemoryStream();
            var result = await WxAppApi.GetWxaCodeUnlimitAsync(AppId, ms, scene, path, check_path, env_version, width);
            if (result.ErrorCodeValue.Equals(0)) return ms;
            return null;
        }
        #endregion

        #region 消息

        /// <summary>
        /// 小程序发送客服消息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="msg"></param>
        public static async Task<string> CustomSendTxt(string openId, string msg)
        {
            var result = await CustomApi.SendTextAsync(AppId, openId, msg);
            return result.errcode.ToString();
        }


        /// <summary>
        /// 发送订阅消息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="templateId">模板ID</param>
        /// <param name="data"> 模板消息数据，JSON 格式形如 { "key1": { "value": any }, "key2": { "value": any } }</param>
        /// <param name="page"></param>
        /// <param name="miniprogramState"></param>
        public static async Task<WxJsonResult> SendMessage(string openId, string templateId, TemplateMessageData data, string page = null, string miniprogramState = null)
        {
            return await MessageApi.SendSubscribeAsync(AppId, openId, templateId, data, page, miniprogramState);
        }
        #endregion

        #region 物流发货管理

        /// <summary>
        /// 发货信息录入接口
        /// </summary>
        /// <param name="logistics_type">物流模式，发货方式枚举值：1、实体物流配送采用快递公司进行实体物流配送形式 2、同城配送 3、虚拟商品，虚拟商品，例如话费充值，点卡等，无实体配送形式 4、用户自提</param>
        /// <param name="out_trade_no">商户系统内部订单号，只能是数字、大小写字母`_-*`且在同一个商户号下唯一</param>
        /// <param name="GoodsInfo">商品信息，例如：微信红包抱枕*1个，限120个字以内</param>
        /// <param name="openId">用户标识，用户在小程序appid下的唯一标识。 下单前需获取到用户的Openid</param>
        /// <returns></returns>
        public static WxJsonResult OrderUploadShippingInfo(int logistics_type, string out_trade_no, ShippingListModel GoodsInfo, string openId)
        {
            var wxQuery = new UploadShippingInfoModel();
            wxQuery.order_key = new OrderKeyModel()
            {
                order_number_type = 1,
                mchid = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_MchId,
                out_trade_no = out_trade_no
            };
            wxQuery.logistics_type = logistics_type;
            wxQuery.delivery_mode = 1;
            wxQuery.shipping_list = new List<ShippingListModel> { GoodsInfo };
            wxQuery.upload_time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss+08:00");
            wxQuery.payer = new PayerModel
            {
                openid = openId
            };
            return Order.UploadShippingInfo(AppId, wxQuery);
        }

        #endregion
    }
}
