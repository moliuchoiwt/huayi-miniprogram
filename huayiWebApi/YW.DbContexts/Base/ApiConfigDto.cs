using Riok.Mapperly.Abstractions;

namespace YW.DbContexts
{


    public class ApiConfigDto
    {
        #region 基本信息

        /// <summary>
        /// 网站名称
        /// </summary>
        public string SiteName { get; set; }
        /// <summary>
        /// 网站Logo
        /// </summary>
        public string SiteLogo { get; set; }

        /// <summary>
        /// 网站域名 
        /// </summary>
        public string DomianName { get; set; }

        /// <summary>
        /// 网站资源域名 
        /// </summary>
        public string DomianStaticName { get; set; }
        /// <summary>
        /// 公司名称
        /// </summary>
        public string WebCompany { get; set; }
        /// <summary>
        /// 公司地址
        /// </summary>
        public string WebAddress { get; set; }
        /// <summary>
        /// 客服信息
        /// </summary>
        public string CustomerInfo { get; set; }
        /// <summary>
        /// 客服号码
        /// </summary>
        public string CustomerMobile { get; set; }
        /// <summary>
        /// Ios版本号
        /// </summary>
        public string Versions { get; set; }
        /// <summary>
        /// IosAppstore 是否Appstore更新 0-跳转Appstore更新 1-跳转企业签名地址更新
        /// </summary>
        public string IosAppstore { get; set; }
        /// <summary>
        /// Android版本号
        /// </summary>
        public string AndroidVersions { get; set; }


        #endregion

        #region 短信配置

        /// <summary>
        /// 短信调方式  1-阿里云
        /// </summary>
        public int SMSType { get; set; }

        #region 互亿无线

        /// <summary>
        /// 互亿无线短信账号
        /// </summary>
        public string SMSaccount { get; set; }
        /// <summary>
        /// 互亿无线短信秘钥
        /// </summary>
        public string SMSpassword { get; set; }
        /// <summary>
        /// 互亿无线请求路径
        /// </summary>
        public string SMSPostUrl { get; set; }
        #endregion

        #region 阿里云

        /// <summary>
        /// 阿里云accessKeyId
        /// </summary>
        public string Ayl_accessKeyId { get; set; }
        /// <summary>
        /// 阿里云accessKeySecret
        /// </summary>
        public string Ayl_accessKeySecret { get; set; }
        /// <summary>
        /// 阿里云-短信签名
        /// </summary>
        public string Ayl_SignName { get; set; }

        /// <summary>
        /// 阿里云-验证码模板Code
        /// </summary>
        public string Ayl_TemplateCode { get; set; }


        #endregion

        #endregion

        #region  微信配置

        /// <summary>
        /// 微信App开发平台AppId
        /// </summary>
        public string WxPhoneAppId { get; set; }
        /// <summary>
        /// 微信App开发平台Secret
        /// </summary>
        public string WxPhoneSecret { get; set; }

        /// <summary>
        /// 微信私钥文件路径
        /// </summary>
        public string privateKeyPath { get; set; } = string.Empty;
        /// <summary>
        /// 微信证书路径
        /// </summary>
        public string certPath { get; set; } = string.Empty;
        /// <summary>
        /// 微信证书秘钥
        /// </summary>
        public string certPwd { get; set; }

        /// <summary>
        /// 微信关注后回复
        /// </summary>
        public string WxWelcome { get; set; }

        /// <summary>
        /// 微信自动回复
        /// </summary>
        public string WxReplyContent { get; set; }
        #endregion

        #region 支付宝
        /// <summary>
        /// 支付宝网关地址
        /// </summary>
        public string Ali_serviceUrl { get; set; }
        /// <summary>
        /// 应用ID
        /// </summary>
        public string Ali_appId { get; set; }
        /// <summary>
        /// 开发者私钥，由开发者自己生成
        /// </summary>
        public string Ali_privateKey { get; set; }
        /// <summary>
        /// 支付宝的支付公钥
        /// </summary>
        public string Ali_publicKey { get; set; }
        /// <summary>
        /// 支付宝的支付公钥
        /// </summary>
        public string Ali_payKey { get; set; }
        /// <summary>
        /// 服务器异步通知页面路径
        /// </summary>
        public string Ali_notify_url { get; set; }
        /// <summary>
        /// 页面跳转同步通知页面路径
        /// </summary>
        public string Ali_return_url { get; set; }
        /// <summary>
        /// 参数返回格式，只支持json
        /// </summary>
        public string Ali_format { get; set; }
        /// <summary>
        /// 调用的接口版本，固定为：1.0
        /// </summary>
        public string Ali_version { get; set; }
        /// <summary>
        /// 商户生成签名字符串所使用的签名算法类型，目前支持RSA2和RSA，推荐使用RSA2
        /// </summary>
        public string Ali_signType { get; set; }
        /// <summary>
        /// 字符编码格式 目前支持utf-8
        /// </summary>
        public string Ali_charset { get; set; }
        /// <summary>
        /// 日志记录
        /// </summary>
        public string Ali_LogPath { get; set; }

        #endregion

        #region  快递100 
        /// <summary>
        /// 快递100  分配的公司编号
        /// </summary>
        public string Customer { get; set; }
        /// <summary>
        /// 快递100 授权key
        /// </summary>
        public string CustomerKey { get; set; }
        #endregion

        #region 提现配置
        /// <summary>
        /// 用户提现最小金额
        /// </summary>
        public decimal UserMinMoney { get; set; }
        /// <summary>
        ///  用户提现手续费
        /// </summary>
        public decimal UserWithdrawalRate { get; set; }
        /// <summary>
        /// 用户提现说明
        /// </summary>
        public string UserWithdrawIntro { get; set; }

        #endregion

        #region 兑换
        /// <summary>
        /// 积分兑换比例
        /// </summary>

        public decimal IntegralRate { get; set; }

        #endregion

        /// <summary>
        /// 隐私条例
        /// </summary>
        public string PrivacyInfo { get; set; } = string.Empty;

        /// <summary>
        /// 视频链接
        /// </summary>
        public string videoUrl { get; set; } = string.Empty;

        /// <summary>
        /// 接单须知
        /// </summary>
        public string richText1 { get; set; } = string.Empty;
        /// <summary>
        /// 备用
        /// </summary>
        public string richText2 { get; set; } = string.Empty;

        /// <summary>
        /// 订单最小价格
        /// </summary>
        public decimal orderMinPrice { get; set; } = 0M;

        /// <summary>
        /// 平台抽成比例
        /// </summary>
        public decimal PlatformProportion { get; set; } = 0M;

        /// <summary>
        /// 任务发布提示
        /// </summary>
        public string textContents { get; set; } = string.Empty;

        /// <summary>
        /// 小程序审核开关
        /// </summary>
        public bool MpWxOpenCheck { get; set; } = false;
        /// <summary>
        /// 小程序审核版本号
        /// </summary>
        public string MpWxOpenVersion { get; set; } = string.Empty;
    }




    public class ConfigView : ApiConfigDto
    {

        /// <summary>
        /// 优惠券新人礼包集合
        /// </summary>
       // public List<CouponBag> BagList { get; set; }
    }

    [Mapper(UseDeepCloning = true)]
    public partial class ApiConfigDtoMapper
    {
        public partial ConfigView ToView(ApiConfigDto model);
        public partial List<ConfigView> ToViewList(List<ApiConfigDto> list);
        public partial ApiConfigDto ToModel(ConfigView model);
    }
}
