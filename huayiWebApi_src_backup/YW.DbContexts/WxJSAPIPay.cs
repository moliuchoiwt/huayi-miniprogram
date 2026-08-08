namespace YW.DbContexts
{
    /// <summary>
    /// 微信支付信息 公众号/小程序
    /// </summary>
    public class WxJSAPIPay
    {
        public string appId { get; set; }

        public string timeStamp { get; set; }

        public string nonceStr { get; set; }

        public string package { get; set; }

        public string paySign { get; set; }

        public string signType { get; set; }
    }
}
