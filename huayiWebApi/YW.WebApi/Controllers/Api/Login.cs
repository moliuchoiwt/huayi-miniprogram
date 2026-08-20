using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace YW.WebApi.ApiControllers
{
    [ApiController]
    [AllowAnonymous]
    [ApiExplorerSettings(GroupName = "v1")]
    [Route("api/[controller]/[action]")]
    public class LoginController : ControllerBase
    {
        private readonly IUserInfoService _userInfoService;
        private readonly ISmsService _smsService;

        private readonly SmsMapper smsMapper = new();
        public LoginController(UserInfoService userInfoService, SmsService smsService)
        {
            _smsService = smsService;
            _userInfoService = userInfoService;
        }

        #region 号码是否已注册

        /// <summary>
        /// 号码是否已注册
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> IsMobile(SmsView view)
        {
            ResultModel result = new ResultModel();

            if (view == null || string.IsNullOrWhiteSpace(view.Mobile))
            {
                result.msg = "号码参数错误";
                return result;
            }
            var isBind = await _userInfoService.CountAsync(a => a.status != 99 && a.mobile == view.Mobile) > 0;
            result.data = isBind;
            result.code = (int)ResultEnum.success;
            result.msg = "ok";


            return result;
        }

        #endregion

        #region 验证码发送

        /// <summary>
        /// 发送验证码
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> SendCode(SmsView view)
        {
            ResultModel result = new ResultModel();


            var isBind = await _userInfoService.CountAsync(a => a.status != 99 && a.mobile == view.Mobile) > 0;
            switch (view.SmsType)
            {
                case (int)SmsEnum.注册:
                    if (isBind)
                    {
                        result.msg = "该手机号已注册绑定，请勿重复注册绑定";
                        return result;
                    }
                    break;
                case (int)SmsEnum.登录:
                    if (!isBind)
                    {
                        result.msg = "该手机号未注册绑定，请请先注册绑定";
                        return result;
                    }
                    break;
            }


            string code = RandHelper.Number(4);//验证码
            string smsContent = string.Format("您的验证码是：{0}。请不要把验证码泄露给其他人。", code);
            //发送短信返回的msg string smsContent = string.Format("您的验证码是：{0}。请不要把验证码泄露给其他人。", code);
            string templateParam = "{\"code\":\"" + code + "\"}";
            //发送短信返回的msg
            var resMsg = Service.AliPay.AliYunClient.send(view.Mobile, templateParam, PubConstant.Config.Ayl_accessKeyId, PubConstant.Config.Ayl_accessKeySecret, PubConstant.Config.Ayl_SignName, PubConstant.Config.Ayl_TemplateCode);
            var sModel = smsMapper.ToModel(view);

            sModel.Title = Enum.Parse(typeof(SmsEnum), view.SmsType.ToString()).ToString();
            sModel.CreateTime = DateTime.Now;
            sModel.ExpireUtc = DateTime.Now.AddMinutes(5);
            sModel.Ip = CommonHelper.GetIP();
            sModel.Code = code;
            sModel.Content = smsContent;
            sModel.Fail = resMsg;
            var isOK = await _smsService.InsertAsync(sModel);
            result.code = isOK ? (int)ResultEnum.success : (int)ResultEnum.fail;
            result.msg = "发送" + (isOK ? "成功" : "失败");


            return result;
        }

        #endregion

        #region 小程序授权手机登录或注册
        [HttpPost]
        public async Task<ResultModel> WxOpenMobileLogin(LoginView model) => await _userInfoService.WxOpenMobileLogin(model);
        #endregion
    }
}
