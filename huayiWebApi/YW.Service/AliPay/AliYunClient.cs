using Aliyun.Acs.Core;
using Aliyun.Acs.Core.Exceptions;
using Aliyun.Acs.Core.Profile;
using Aliyun.Acs.Dysmsapi.Model.V20170525;

namespace YW.Service.AliPay
{
    public class AliYunClient
    {

        /// <summary>
        /// 阿里云调用短信
        /// </summary>
        /// <param name="mobile">必填:待发送手机号。支持以逗号分隔的形式进行批量调用，批量上限为20个手机号码,批量调用相对于单条调用及时性稍有延迟,验证码类型的短信推荐使用单条调用的方式</param>
        /// <param name="accessKeyId">你的accessKeyId</param>
        /// <param name="accessKeySecret">你的accessKeySecret</param>
        /// <param name="signName">必填:短信签名-可在短信控制台中找到</param>
        /// <param name="templateCode">必填:短信模板-可在短信控制台中找到</param>
        /// <param name="templateParam">可选:模板中的变量替换JSON串,如模板内容为"亲爱的${name},您的验证码为${code}"时,此处的值为"{\"code\":\"" + code + "\"}"</param>
        /// <returns></returns>
        public static string send(string mobile, string templateParam, string accessKeyId, string accessKeySecret, string signName, string templateCode)
        {
            try
            {
                IClientProfile profile = DefaultProfile.GetProfile("cn-hangzhou", accessKeyId, accessKeySecret);
                profile.AddEndpoint("cn-hangzhou", "cn-hangzhou", "Dysmsapi", "dysmsapi.aliyuncs.com");
                IAcsClient acsClient = new DefaultAcsClient(profile);
                SendSmsRequest request = new SendSmsRequest();

                request.PhoneNumbers = mobile;
                request.SignName = signName;

                request.TemplateCode = templateCode;
                request.TemplateParam = templateParam;
                //请求失败这里会抛ClientException异常
                SendSmsResponse sendSmsResponse = acsClient.GetAcsResponse(request);
                //result表示执行结果，是由阿里云返回给本地服务器的
                if (!sendSmsResponse.Code.Equals("OK")) LogHelper.Error($"阿里云短信调用：{sendSmsResponse.Message}");
                String result = sendSmsResponse.Message;
                return result;
            }
            catch (ServerException e)
            {
                LogHelper.Error("阿里云短信调用", e);
                return e.Message;
            }
            catch (ClientException e)
            {
                LogHelper.Error("阿里云短信调用", e);
                return e.Message;
            }
        }
    }
}
