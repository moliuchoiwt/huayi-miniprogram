using Senparc.Weixin.Entities;
using Senparc.Weixin.Exceptions;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.AdvancedAPIs.Media;
using Senparc.Weixin.MP.AdvancedAPIs.OAuth;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using Senparc.Weixin.MP.AdvancedAPIs.User;
using Senparc.Weixin.MP.CommonAPIs;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Menu;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.Helpers;
using System.IO;
using System.Threading;

namespace YW.Service.WeChat
{
    /// <summary>
    /// 微信公众号操作
    /// </summary>
    public class MpClient
    {
        public MpClient()
        {

        }

        /*应该可以定义一个异常基类，在全局Application_Error处理*/
        private static void WxWriteLogError(Exception ex, string msg = "")
        {
            LogHelper.Error("【微信】" + ex.TargetSite.Name + "【异常信息Message】：" + ex.Message + "【自定义信息】：" + msg);
        }


        #region [内部变量]
        /// <summary>
        /// 公众号AppId
        /// </summary>
        private static string AppId = Senparc.Weixin.Config.SenparcWeixinSetting.MpSetting.WeixinAppId;
        /// <summary>
        /// 公众号Secret
        /// </summary>
        private static string Secret = Senparc.Weixin.Config.SenparcWeixinSetting.MpSetting.WeixinAppSecret;
        /// <summary>
        /// 公众号token
        /// </summary>
        private static string token = Senparc.Weixin.Config.SenparcWeixinSetting.MpSetting.Token;
        #endregion

        #region [微信公众号建立连接，被动回复]
        /// <summary>
        /// 建立链接
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="timestamp"></param>
        /// <param name="nonce"></param>
        /// <param name="echostr"></param>
        /// <returns></returns>
        public static string Check(string signature, string timestamp, string nonce, string echostr)
        {
            return CheckSignature.Check(signature, timestamp, nonce, token) ? echostr : "验证未通过";
        }

        /// <summary>
        /// 接收微信post消息
        /// </summary>
        /// <param name="inputStream"></param>
        /// <param name="maxRecordCount"></param>
        /// <returns></returns>
        public static async Task<string> WxRequest(Stream inputStream,
            string signature, string timestamp, string nonce, string echostr,
            int maxRecordCount = 10)
        {
            //验证字符串...
            if (!CheckSignature.Check(signature, timestamp, nonce, token))
            {
                return "签名错误";
            }
            PostModel postModel = new PostModel();
            postModel.Token = token;
            postModel.EncodingAESKey = Senparc.Weixin.Config.SenparcWeixinSetting.EncodingAESKey;
            postModel.AppId = AppId;

            var messageHandler = new MpCustomMessageHandler(inputStream, postModel, maxRecordCount);
            try
            {
                var ct = new CancellationToken();
                await messageHandler.ExecuteAsync(ct);
            }
            catch (Exception ex)
            {
                WxWriteLogError(ex);
            }
            return messageHandler.ResponseDocument.ToString();
        }
        #endregion

        #region[微信公众号授权登录相关]
        /// <summary>
        /// 获取授权登录地址
        /// </summary>
        /// <param name="redirectUrl">回调地址</param>
        /// <param name="state">携带参数</param>
        /// <returns></returns>
        public static string GetAuthorizeUrl(string redirectUrl, string state = "")
        {
            try
            {
                return OAuthApi.GetAuthorizeUrl(AppId, redirectUrl, state, OAuthScope.snsapi_userinfo);
            }
            catch (Exception ex)
            {
                WxWriteLogError(ex);
            }
            return null;
        }

        /// <summary>
        /// 根据code获取AccessToken
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static async Task<string> GetAccessToken(string code)
        {
            try
            {
                OAuthAccessTokenResult result = await OAuthApi.GetAccessTokenAsync(AppId, Secret, code);
                return Newtonsoft.Json.JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                WxWriteLogError(ex);
            }
            return string.Empty;
        }
        /// <summary>
        /// 根据OpenId拉取用户信息
        /// </summary>
        /// <param name="OpenId"></param>
        /// <returns></returns>
        public static async Task<UserInfoJson> GetUserInfo(string OpenId)
        {
            try
            {
                var UserInfo = await UserApi.InfoAsync(AppId, OpenId);
                return UserInfo;
            }
            catch (Exception ex)
            {
                WxWriteLogError(ex);
            }
            return null;

        }


        /// <summary>
        /// 根据AccessToken和openid拉取用户信息
        /// </summary>
        /// <returns></returns>
        public static async Task<OAuthUserInfo> GetUserInfo(string accessToken, string openId)
        {
            try
            {
                OAuthUserInfo userInfo = await OAuthApi.GetUserInfoAsync(accessToken, openId);
                return userInfo;
            }
            catch (ErrorJsonResultException ex)
            {
                WxWriteLogError(ex);
                return null;
            }
        }
        #endregion

        #region [获取jssdk参数]

        /// <summary>
        /// 获取jssdk参数
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <returns></returns>
        public static async Task<string> GetJsSdk(string requestUrl)
        {
            string code = "";
            var jssdkUiPackage = await GetJSSDK(requestUrl);
            if (jssdkUiPackage != null)
            {
                code = "{ appId: '" + jssdkUiPackage.AppId
                   + "',timestamp: '" + jssdkUiPackage.Timestamp
                   + "',nonceStr: '" + jssdkUiPackage.NonceStr
                   + "',signature: '" + jssdkUiPackage.Signature
                   + "'}";
            }
            return code;
        }
        /// <summary>
        ///  获取jssdk参数
        /// </summary>
        public static async Task<JsSdkUiPackage> GetJSSDK(string requestUrl)
        {
            try
            {
                var jssdkUiPackage = await JSSDKHelper.GetJsSdkUiPackageAsync(AppId, Secret, requestUrl);
                return jssdkUiPackage;
            }
            catch (Exception ex)
            {
                WxWriteLogError(ex, "异常获取JSSDK参数");
            }
            return null;
        }
        #endregion

        #region [发送消息]

        /// <summary>
        /// 发送客服消息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="msg"></param>
        public static async Task<string> CustomSendText(string openId, string msg)
        {
            try
            {
                var result = await CustomApi.SendTextAsync(AppId, openId, msg);
                return result.errcode.ToString();
            }
            catch (Exception ex)
            {
                WxWriteLogError(ex, "异步：异常发送客服消息文字");
            }
            return "";
        }
        /// <summary>
        /// 发送图文消息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="articles">图文</param>
        public static async Task<string> CustomSendNews(string openId, List<Senparc.NeuChar.Entities.Article> articles)
        {
            try
            {
                var result = await CustomApi.SendNewsAsync(AppId, openId, articles);
                return result.ToString();
            }
            catch (Exception ex)
            {
                WxWriteLogError(ex);
            }
            return string.Empty;
        }
        /// <summary>
        /// 发送图文消息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="mediaId"></param>
        public static async Task<string> CustomSendMpNews(string openId, string mediaId)
        {
            try
            {
                var result = await CustomApi.SendMpNewsAsync(AppId, openId, mediaId);
                return result.ToString();
            }
            catch (Exception ex)
            {
                WxWriteLogError(ex);
            }
            return string.Empty;
        }

        /// <summary>
        /// 发送图片消息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="mediaId">微信模板ID</param>
        public static async Task<string> CustomSendImage(string openId, string mediaId)
        {
            try
            {
                var result = await CustomApi.SendImageAsync(AppId, openId, mediaId);
                return result.ToString();
            }
            catch (Exception ex)
            {
                WxWriteLogError(ex);
            }
            return string.Empty;
        }

        /// <summary>
        /// 发送语音消息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="mediaId"></param>
        public static async Task<string> CustomSendVoice(string openId, string mediaId)
        {
            try
            {
                var result = await CustomApi.SendVoiceAsync(AppId, openId, mediaId);
                return result.ToString();
            }
            catch (Exception ex)
            {
                WxWriteLogError(ex);
            }
            return string.Empty;
        }

        /// <summary>
        /// 发送视频消息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="mediaId"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        public static async Task<string> CustomSendVideo(string openId, string mediaId, string title, string description)
        {
            try
            {
                var result = await CustomApi.SendVideoAsync(AppId, openId, mediaId, title, description);
                return result.ToString();
            }
            catch (Exception ex)
            {
                WxWriteLogError(ex);
            }
            return string.Empty;
        }
        /// <summary>
        /// 发送音乐消息
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="musicUrl">音乐链接</param>
        /// <param name="hqMusicUrl">高品质音乐链接，wifi环境优先使用该链接播放音乐</param>
        /// <param name="thumbMediaId">视频缩略图的媒体ID</param>
        public static async Task<string> CustomSendMusic(string openId, string title, string description, string musicUrl, string hqMusicUrl, string thumbMediaId)
        {
            try
            {
                var result = await CustomApi.SendMusicAsync(AppId, openId, title, description, musicUrl, hqMusicUrl, thumbMediaId);
                return result.ToString();
            }
            catch (Exception ex)
            {
                WxWriteLogError(ex);
            }
            return string.Empty;
        }

        /// <summary>
        /// 群发接口[文本消息]
        /// </summary>
        public static async void SendGroupMessage(string msg, params string[] openIds)
        {
            var result = await GroupMessageApi.SendGroupMessageByOpenIdAsync(AppId, GroupMessageType.text, msg, null, 10000, openIds);
        }

        /// <summary>
        /// 发送模板消息
        /// 注意： data使用匿名类型，参数要和templateId参数一致
        /// var data = new{ first = new TemplateDataItem("【测试标题】"), keyword1 = new TemplateDataItem("keyword1"),keyword2 = new TemplateDataItem("keyword2"), remark = new TemplateDataItem("remark")}
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="templateId"></param>
        /// <param name="url"></param>
        /// <param name="data"></param>
        public static async Task<string> TemplaiteSend(string openId, string templateId, string url, object data, TemplateModel_MiniProgram miniProgram = null)
        {
            string json = "";
            if (miniProgram == null)
            {
                miniProgram = new TemplateModel_MiniProgram { appid = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId, pagepath = "pages/index/index" };
            }
            var res = await TemplateApi.SendTemplateMessageAsync(AppId, openId, templateId, url, data, miniProgram);
            json = res.ToString();
            return json;

        }

        #endregion

        #region 素材管理
        /// <summary>
        /// 获取图文素材列表
        /// <param name="offset">从全部素材的该偏移位置开始返回，0表示从第一个素材 返回</param>
        /// <param name="count">返回素材的数量，取值在1到20之间</param>
        /// </summary>
        public static async Task<MediaList_NewsResult> GetNewsList(int offset = 0, int count = 20)
        {
            return await MediaApi.GetNewsMediaListAsync(AppId, offset, count);

        }

        /// <summary>
        /// 获取图片素材列表
        /// <param name="offset">从全部素材的该偏移位置开始返回，0表示从第一个素材 返回</param>
        /// <param name="count">返回素材的数量，取值在1到20之间</param>
        /// </summary>
        public static async Task<MediaList_OthersResult> GetOthersMediaList(int offset = 0, int count = 20)
        {

            return await MediaApi.GetOthersMediaListAsync(AppId, UploadMediaFileType.image, offset, count);

        }

        /// <summary>
        /// 根据mediaId获取永久图文素材
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        public static async Task<GetNewsResultJson> GetForeverNews(string mediaId)
        {
            return await MediaApi.GetForeverNewsAsync(AppId, mediaId);
        }

        /// <summary>
        /// 获取永久素材(除了图文、视频)
        /// </summary>
        /// <param name="mediaId">要获取的素材的media_id</param>
        /// <param name="s">写入文件流</param>
        /// <returns></returns>
        public static async Task<WxJsonResult> GetForeverMedia(string mediaId, Stream s)
        {
            return await MediaApi.GetForeverMediaAsync(AppId, mediaId, s);
        }

        /// <summary>
        ///  新增其他类型永久素材(图片（image）、语音（voice）和缩略图（thumb）)
        /// </summary>
        /// <param name="file">上传文件的绝对路径</param>
        /// <returns></returns>
        public static async Task<UploadForeverMediaResult> UploadForeverMedia(string file, UploadForeverMediaType type)
        {
            return await MediaApi.UploadForeverMediaAsync(AppId, file, type);

        }

        #endregion

        #region 菜单管理

        /// <summary>
        /// 创建菜单
        /// </summary>
        /// <param name="buttonData"></param>
        public static WxJsonResult ToCreateMenu()
        {
            var buttonData = new ButtonGroup();
            buttonData.button = new List<BaseButton>()
            {
                //new SingleMiniProgramButton() { name = "俱乐部", appid = PubConstant.Config.WxOpenAppId, pagepath = "pages/index/index", type ="click" , url="http://www.szfeigaogao.com"},
            };

            var result = CreateMenu(buttonData);
            return result;

        }


        /// <summary>
        /// 创建菜单
        /// </summary>
        /// <param name="buttonData"></param>
        public static WxJsonResult CreateMenu(ButtonGroup buttonData)
        {
            try
            {
                var result = CommonApi.CreateMenu(AppId, buttonData);
                return result;
            }
            catch (Exception ex)
            {
                WxWriteLogError(ex);
            }
            return null;
        }

        /// <summary>
        /// 查询菜单
        /// </summary>
        /// <returns></returns>
        public static GetMenuResult GetMenu()
        {
            try
            {
                var result = CommonApi.GetMenu(AppId);
                return result;
            }
            catch (Exception ex)
            {
                WxWriteLogError(ex);
            }
            return null;
        }


        #endregion



    }
}
