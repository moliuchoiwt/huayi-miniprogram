using Alipay.AopSdk.Core;
using Alipay.AopSdk.Core.Domain;
using Alipay.AopSdk.Core.Request;
using Alipay.AopSdk.Core.Response;
using Alipay.AopSdk.Core.Util;
namespace YW.Service.AliPay
{
    public class AliPayClient
    {
        #region //支付宝变量

        //支付宝网关地址
        private static string serviceUrl = PubConstant.Config.Ali_serviceUrl;

        //应用ID
        public static string appId = PubConstant.Config.Ali_appId;

        //开发者私钥，由开发者自己生成
        public static string privateKey = PubConstant.Config.Ali_privateKey;

        //支付宝的应用公钥
        public static string publicKey = PubConstant.Config.Ali_publicKey;

        //支付宝的支付公钥
        public static string payKey = PubConstant.Config.Ali_payKey;

        //服务器异步通知页面路径
        public static string notify_url = PubConstant.Config.Ali_notify_url;

        //页面跳转同步通知页面路径
        public static string return_url = PubConstant.Config.Ali_return_url;

        //参数返回格式，只支持json
        public static string format = PubConstant.Config.Ali_format;

        // 调用的接口版本，固定为：1.0
        public static string version = PubConstant.Config.Ali_version;

        // 商户生成签名字符串所使用的签名算法类型，目前支持RSA2和RSA，推荐使用RSA2
        public static string signType = PubConstant.Config.Ali_signType;

        // 字符编码格式 目前支持utf-8
        public static string charset = PubConstant.Config.Ali_charset;
        // 日志记录
        public static string LogPath = PubConstant.Config.Ali_LogPath;

        // false 表示不从文件加载密钥
        public static bool keyFromFile = false;

        #endregion


        public static IAopClient GetAlipayClient()
        {
            IAopClient client = new DefaultAopClient(serviceUrl, appId, privateKey, format, version, signType, publicKey, charset, keyFromFile);

            return client;
        }

        /// <summary>
        ///  统一下单
        /// </summary>
        /// <param name="orderNo">订单编号</param>
        /// <param name="payPrice">支付金额</param>
        /// <returns></returns>
        public static string AlipayTrade(string orderNo, decimal payPrice, string notifyurl = "")
        {
            if (string.IsNullOrWhiteSpace(notifyurl)) notifyurl = notify_url;
            IAopClient client = GetAlipayClient();
            AlipayTradeAppPayRequest request = new AlipayTradeAppPayRequest();
            //SDK已经封装掉了公共参数，这里只需要传入业务参数。以下方法为sdk的model入参方式(model和biz_content同时存在的情况下取biz_content)。
            AlipayTradeAppPayModel model = new AlipayTradeAppPayModel();
            model.Subject = "商品购买";
            model.TotalAmount = payPrice.ToString("F2");
            model.ProductCode = "QUICK_MSECURITY_PAY";
            model.OutTradeNo = orderNo;
            model.TimeoutExpress = "30m";
            request.SetBizModel(model);
            request.SetNotifyUrl(notify_url);
            //request.SetReturnUrl(return_url);
            //这里和普通的接口调用不同，使用的是sdkExecute
            AlipayTradeAppPayResponse response = client.SdkExecute(request);
            return response.Body;
        }


        /// <summary>
        /// 订单退款
        /// </summary>
        /// <param name="orderNo">订单编号</param>
        /// <param name="refund_amount">退款金额</param>
        /// <returns></returns>
        public static bool TradeRefund(string orderNo, decimal refund_amount, string out_request_no = "")
        {

            IAopClient client = GetAlipayClient();
            AlipayTradeRefundRequest request = new AlipayTradeRefundRequest();
            Dictionary<string, object> bizContent = new Dictionary<string, object>();
            bizContent.Add("out_trade_no", orderNo);
            bizContent.Add("refund_amount", refund_amount);
            bizContent.Add("out_request_no", out_request_no);

            string Contentjson = JsonConvert.SerializeObject(bizContent);
            request.BizContent = Contentjson;
            AlipayTradeRefundResponse response = client.Execute(request);
            if (response.Code == "10000" && response.Msg == "Success")
            {
                return true;
            }
            else
            {
                LogHelper.Info("订单支付宝退款 result:" + JsonConvert.SerializeObject(response.Body));
                return false;
            }

        }

        public static bool AliPayNotifyUrl(HttpContext context, out string payNo)
        {
            payNo = string.Empty;
            IDictionary<string, string> map = GetRequestPost(context);

            if (map.Count > 0)
            {
                string alipayPublicKey = payKey;
                bool keyFromFile = false;

                bool verify_result = AlipaySignature.RSACheckV1(map, alipayPublicKey, charset, signType, keyFromFile);
                LogHelper.Info("AliPayNotifyUrl验签" + verify_result + "");

                //验签成功后，按照支付结果异步通知中的描述，对支付结果中的业务内容进行二次校验，校验成功后再response中返回success并继续商户自身业务处理，校验失败返回false
                if (verify_result)
                {
                    //商户订单号
                    string out_trade_no = map["out_trade_no"];
                    //支付宝交易号
                    string trade_no = map["trade_no"];
                    //交易创建时间
                    string gmt_create = map["gmt_create"];
                    //交易付款时间
                    string gmt_payment = map["gmt_payment"];
                    //通知时间
                    string notify_time = map["notify_time"];
                    //通知类型  trade_status_sync
                    string notify_type = map["notify_type"];
                    //通知校验ID
                    string notify_id = map["notify_id"];
                    //开发者的app_id
                    string app_id = map["app_id"];
                    //卖家支付宝用户号
                    string seller_id = map["seller_id"];
                    //买家支付宝用户号
                    string buyer_id = map["buyer_id"];
                    //实收金额
                    string receipt_amount = map["receipt_amount"];
                    //交易状态
                    string return_code = map["trade_status"];

                    //交易状态TRADE_FINISHED的通知触发条件是商户签约的产品不支持退款功能的前提下，买家付款成功；
                    //或者，商户签约的产品支持退款功能的前提下，交易已经成功并且已经超过可退款期限
                    //状态TRADE_SUCCESS的通知触发条件是商户签约的产品支持退款功能的前提下，买家付款成功
                    if (return_code == "TRADE_FINISHED" || return_code == "TRADE_SUCCESS")
                    {
                        LogHelper.Info("AliPayNotifyUrl" + receipt_amount + "==" + trade_no + "==" + return_code + "==" + out_trade_no + "==" + gmt_payment);
                        payNo = out_trade_no;
                        ////判断该笔订单是否在商户网站中已经做过处理
                        /////支付回调的业务处理
                        ////bool res = OrderBll.Value.CompleteAliPay(receipt_amount, trade_no, return_code, out_trade_no, gmt_payment, out msg);
                        //bool res = true;

                        //if (res == false)
                        //{
                        //    return "添加支付信息失败!";
                        //}
                        //return "success";  //请不要修改或删除
                        return true;
                    }
                }
                else
                {
                    //验证失败
                    LogHelper.Info("AliPayNotifyUrl:支付验证失败");
                }
            }
            return false;
        }

        /// <summary>
        /// 获取支付宝POST过来通知消息，并以“参数名=参数值”的形式组成数组
        /// </summary>
        /// <returns>request回来的信息组成的数组</returns>
        public static IDictionary<string, string> GetRequestPost(HttpContext context)
        {
            IDictionary<string, string> sArray = new Dictionary<string, string>();
            // Get names of all forms into a string array.
            var keys = context.Request.Form.Keys;
            if (keys != null)
            {
                foreach (string key in keys)
                {
                    sArray.Add(key, context.Request.Form[key]);
                }
            }

            return sArray;
        }
    }
}
