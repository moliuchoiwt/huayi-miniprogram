using Senparc.Weixin.Entities;
using Senparc.Weixin.TenPay.V3;
using Senparc.Weixin.TenPayV3.Apis.FundApp;
using System.IO;


namespace YW.Service.WeChat
{
    /// <summary>
    /// 微信支付相关
    /// </summary>
    public class TenPayClient
    {
        #region [内部变量]


        private static string WxPhoneAppId = PubConstant.Config.WxPhoneAppId;
        private static string WxPhoneSecret = PubConstant.Config.WxPhoneAppId;


        private static string WxOpenAppId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenSetting.WxOpenAppId;
        private static string WxOpenSecret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenSetting.WxOpenAppSecret;

        private static string AppId = Senparc.Weixin.Config.SenparcWeixinSetting.MpSetting.WeixinAppId;
        private static string Secret = Senparc.Weixin.Config.SenparcWeixinSetting.MpSetting.WeixinAppSecret;

        private static string MchId = Senparc.Weixin.Config.SenparcWeixinSetting.TenpayV3Setting.TenPayV3_MchId;
        private static string MchIdkey = Senparc.Weixin.Config.SenparcWeixinSetting.TenpayV3Setting.TenPayV3_Key;

        private static string CertPath = CommonHelper.GetMapPath(Senparc.Weixin.Config.SenparcWeixinSetting.TenpayV3Setting.TenPayV3_CertPath);
        private static string CertPassword = Senparc.Weixin.Config.SenparcWeixinSetting.TenpayV3Setting.TenPayV3_CertSecret;


        #endregion

        private readonly static ISenparcWeixinSettingForTenpayV3 SenparcWeixinTenpayV3Setting = Senparc.Weixin.Config.SenparcWeixinSetting.TenpayV3Setting;

        #region [微信支付相关]
        /// <summary>
        /// 调起微信支付JSAPI
        /// </summary>
        /// <param name="openId"></param>
        /// <param name="orderNo">订单号</param>
        /// <param name="body"></param>
        /// <param name="price">金额 单位分</param>
        /// <param name="hostAddress">请求地址ip</param>
        /// <param name="refUrl">支付完成后的回调处理页面</param>
        /// <param name="notifyUrl">回调地址</param>
        /// <param name="payType">默认0 公众号支付，1小程序支付,2 App支付</param>
        /// <param name="tenPay">默认0  JSAPI = 0,NATIVE = 1, APP = 2,MWEB = 3</param>
        /// <returns></returns>
        public static async Task<object> TenPayByJsapi(string openId, string orderNo,
            string body, int price, string refUrl, string notifyUrl, int payType = 0, int tenPay = 0)
        {

            string spBillno = orderNo;
            int totalFee = price;
            string ip = CommonHelper.GetIP();// hostAddress;
            string payAppid = AppId, parSecret = Secret;// 公众号配置
            string payMchId = MchId, payMchIdkey = MchIdkey;
            if (payType == 1)
            {
                //小程序配置
                payAppid = WxOpenAppId;
                parSecret = WxOpenSecret;
            }
            else if (payType == 2)
            {
                //微信App开发平台配置
                payAppid = WxPhoneAppId;
                parSecret = WxPhoneSecret;
            }

            //LogHelper.WriteLog("微信下单payAppid:"+ payAppid);
            TenPayV3Info payInfo = new TenPayV3Info(payAppid, parSecret, payMchId, payMchIdkey, CertPath, CertPassword, refUrl, notifyUrl);
            var timeStamp = TenPayV3Util.GetTimestamp();
            var nonceStr = TenPayV3Util.GetNoncestr();

            var xmlDataInfo = new TenPayV3UnifiedorderRequestData(payInfo.AppId, payInfo.MchId, body,
                spBillno, totalFee, ip, payInfo.TenPayV3_WxOpenNotify, (Senparc.Weixin.TenPay.TenPayV3Type)tenPay, openId, payInfo.Key, nonceStr);

            var result = await TenPayV3.UnifiedorderAsync(xmlDataInfo);//调用统一订单接口
            LogHelper.Info("微信统一下单结果:" + Newtonsoft.Json.JsonConvert.SerializeObject(result));
            if (tenPay == (int)Senparc.Weixin.TenPay.TenPayV3Type.MWEB)
            {
                var data = new { mweburl = result.mweb_url };
                return data;
            }
            else
            {
                //参数生成
                var package = string.Format("prepay_id={0}", result.prepay_id);
                var paySign = TenPayV3.GetJsPaySign(payInfo.AppId, timeStamp, nonceStr, package, payInfo.Key);
                if (tenPay == (int)Senparc.Weixin.TenPay.TenPayV3Type.APP) paySign = paySign.Substring(0, 30);

                WxJSAPIPay payPara = new WxJSAPIPay()
                {
                    appId = payAppid,
                    timeStamp = timeStamp,
                    nonceStr = nonceStr,
                    package = package,
                    paySign = paySign,
                    signType = "MD5"
                };
                return payPara;
            }
        }


        /// <summary>
        /// 订单退款...
        /// 需要添加退款通知地址
        /// </summary>
        /// <param name="billNo">订单号</param>
        /// <param name="billFee">退款金额（单位：分）</param>
        /// <param name="tenPayV3Notif">回调地址</param>
        /// <returns></returns>
        public static string Refund(IServiceProvider serviceProvider, string billNo, int billFee, int payType = 0, string tenPayV3Notif = "")
        {
            string payAppId = payType != 0 ? WxOpenAppId : AppId;//小程序:公众号
            try
            {
                TenPayV3Info tenPayV3Info = new TenPayV3Info(payAppId, Secret, MchId, MchIdkey, CertPath, CertPassword, tenPayV3Notif, tenPayV3Notif);
                string nonceStr = TenPayV3Util.GetNoncestr();
                string outTradeNo = billNo;
                string outRefundNo = "OutRefunNo-" + DateTime.Now.Ticks;
                int totalFee = billFee;
                int refundFee = totalFee;
                string opUserId = tenPayV3Info.MchId;
                var dataInfo = new TenPayV3RefundRequestData(
                    tenPayV3Info.AppId,
                    tenPayV3Info.MchId,
                    tenPayV3Info.Key,
                    null, //描述
                    nonceStr,
                    null, //微信订单号	
                    outTradeNo,//商户订单号 二选一
                    outRefundNo,//商户退款单号
                    totalFee,//订单金额
                    refundFee,//退款金额
                    opUserId,
                    null//退款资金来源
                    );
                var result = TenPayV3.Refund(serviceProvider, dataInfo);
                LogHelper.Info("微信订单退款 result:" + Newtonsoft.Json.JsonConvert.SerializeObject(result));
                return result.result_code == "FAIL" ? result.err_code_des : "成功";
            }
            catch (Exception ex)
            {
                LogHelper.Error("【微信订单退款】" + ex.TargetSite.Name + "【异常信息Message】：" + ex.Message);
            }
            return "";

        }



        /// <summary>
        /// 订单退款...
        /// 需要添加退款通知地址
        /// </summary>
        /// <param name="billNo">订单号</param>
        /// <param name="billFee">退款金额（单位：分）</param>
        /// <param name="orderFee">原订单金额（单位：分）</param>
        /// <param name="tenPayV3Notif">回调地址</param>
        /// <returns></returns>
        public static string Refund(IServiceProvider serviceProvider, string billNo, int billFee, int orderFee, int payType = 0, string tenPayV3Notif = "")
        {
            string payAppId = payType != 0 ? WxOpenAppId : AppId;//小程序:公众号
            try
            {
                TenPayV3Info tenPayV3Info = new TenPayV3Info(payAppId, Secret, MchId, MchIdkey, CertPath, CertPassword, tenPayV3Notif, tenPayV3Notif);
                string nonceStr = TenPayV3Util.GetNoncestr();
                string outTradeNo = billNo;
                string outRefundNo = "OutRefunNo-" + DateTime.Now.Ticks;
                int totalFee = orderFee;
                int refundFee = totalFee;
                string opUserId = tenPayV3Info.MchId;
                var dataInfo = new TenPayV3RefundRequestData(
                    tenPayV3Info.AppId,
                    tenPayV3Info.MchId,
                    tenPayV3Info.Key,
                    null, //描述
                    nonceStr,
                    null, //微信订单号	
                    outTradeNo,//商户订单号 二选一
                    outRefundNo,//商户退款单号
                    totalFee,//订单金额
                    refundFee,//退款金额
                    opUserId,
                    null//退款资金来源
                    );
                var result = TenPayV3.Refund(serviceProvider, dataInfo);
                LogHelper.Info("微信订单退款 result:" + Newtonsoft.Json.JsonConvert.SerializeObject(result));
                return result.result_code == "FAIL" ? result.err_code_des : "成功";
            }
            catch (Exception ex)
            {
                LogHelper.Error("【微信订单退款】" + ex.TargetSite.Name + "【异常信息Message】：" + ex.Message);
            }
            return "";

        }


        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public static async Task<string> OrderQuery(string orderId)
        {
            try
            {
                string nonceStr = TenPayV3Util.GetNoncestr();
                //RequestHandler packageReqHandler = new RequestHandler(null);

                //设置package订单参数
                //packageReqHandler.SetParameter("appid", AppId);       //公众账号ID
                //packageReqHandler.SetParameter("mch_id", MchId);          //商户号
                //packageReqHandler.SetParameter("transaction_id", "");       //填入微信订单号 
                //packageReqHandler.SetParameter("out_trade_no", orderId);         //填入商家订单号
                //packageReqHandler.SetParameter("nonce_str", nonceStr);             //随机字符串
                //string sign = packageReqHandler.CreateMd5Sign("key", MchIdkey);
                //packageReqHandler.SetParameter("sign", sign);                       //签名
                //string data = packageReqHandler.ParseXML();
                //var result = TenPayV3.OrderQuery(data);
                //var res = XDocument.Parse(result);

                //return result;


                var dataInfo = new TenPayV3OrderQueryRequestData(AppId, MchId, "", nonceStr, orderId, MchId);
                var result = await TenPayV3.OrderQueryAsync(dataInfo);
                return result.ResultXml;

            }
            catch (Exception ex)
            {
                LogHelper.Error("【微信企业付款】" + ex.TargetSite.Name + "【异常信息Message】：" + ex.Message);
            }
            return null;
        }

        /// <summary>
        /// 统一下单回调
        /// </summary>
        /// <param name="context"></param>
        /// <param name="outTradeNo">返回的订单号</param>
        /// <returns></returns>
        public static bool PayNotifyUrl(HttpContext context, out string outTradeNo, out string xml)
        {
            try
            {
                ResponseHandler resHandler = new ResponseHandler(context);
                // LogHelper.WriteLog("统一下单回调：" + resHandler.ParseXML());
                string returnCode = resHandler.GetParameter("return_code");
                string return_Msg = resHandler.GetParameter("return_msg");
                xml = string.Format(@"<xml><return_code><![CDATA[{0}]]></return_code><return_msg><![CDATA[{1}]]></return_msg></xml>", returnCode, return_Msg);
                resHandler.SetKey(MchIdkey);
                var isSign = resHandler.IsTenpaySign();
                // LogHelper.WriteLog("统一下单回调验签：" + isSign+";xml结果"+ xml);
                //验证请求是否从微信发过来（安全）
                if (isSign && returnCode.ToUpper() == "SUCCESS")
                {
                    outTradeNo = resHandler.GetParameter("out_trade_no");
                    return true;
                    //直到这里，才能认为交易真正成功了，可以进行数据库操作，但是别忘了返回规定格式的消息！
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error("【微信】" + ex.TargetSite.Name + "【异常信息Message】：" + ex.Message);
            }
            outTradeNo = ""; xml = "";
            return false;

        }


        /// <summary>
        /// 企业付款
        /// </summary>
        /// <param name="outTradeNo">流水号</param>
        /// <param name="openId"></param>
        /// <param name="amount">金额 decimal（单位 元，最少一元起付）</param>
        /// <param name="desc">描述</param>
        /// <param name="ip">请求Ip</param>
        /// <param name="payType">默认0 公众号支付，1小程序支付</param>
        /// <returns></returns>
        public static string Transfers(IServiceProvider serviceProvider, string outTradeNo, string openId, decimal amount, string desc, string ip, int payType = 0)
        {
            string payAppId = payType != 0 ? WxOpenAppId : AppId;//小程序:公众号
            try
            {
                const string deviceInfo = "";
                string nonceStr = TenPayV3Util.GetNoncestr();
                var xmlDataInfo = new TenPayV3TransfersRequestData(
                    payAppId, //
                    MchId, //商户号
                    deviceInfo,//设备号 非必填
                    nonceStr, //随机字符串
                    outTradeNo, //partner_trade_no商户订单号(只能是字母或者数字，不能包含有符号)
                    openId,//
                    MchIdkey, //商户号key
                    "NO_CHECK",// 校验用户姓名选项 NO_CHECK：不校验真实姓名 
                    "", //收款用户姓名 
                    amount, //金额
                    desc, //企业付款描述信息
                    ip //调用接口的机器Ip地址
                    );
                string cert = CertPath;//证书绝对路径
                string certPwd = CertPassword;//证书密码
                var result = TenPayV3.Transfers(serviceProvider, xmlDataInfo);
                LogHelper.Info("企业付款 result:" + Newtonsoft.Json.JsonConvert.SerializeObject(result));
                return result.result_code == "SUCCESS" ? "成功" : result.err_code_des;
            }
            catch (Exception ex)
            {
                LogHelper.Error("【微信企业付款】" + ex.TargetSite.Name + "【异常信息Message】：" + ex.Message);
            }
            return "";

        }


        /// <summary>
        /// 企业付款到银行卡
        /// </summary>
        /// <param name="outTradeNo">流水号</param>
        /// <param name="BankCode">收款方开户行。银行卡所在开户行编号,详见银行编号列表 https://pay.weixin.qq.com/wiki/doc/api/tools/mch_pay.php?chapter=24_4 </param>
        /// <param name="EncBankNumber">收款方银行卡号（采用标准RSA算法，公钥由微信侧提供）,详见获取RSA加密公钥API https://pay.weixin.qq.com/wiki/doc/api/tools/mch_pay.php?chapter=24_7 </param>
        /// <param name="EncTrueName">收款方用户名（采用标准RSA算法，公钥由微信侧提供）,详见获取RSA加密公钥API https://pay.weixin.qq.com/wiki/doc/api/tools/mch_pay.php?chapter=24_7 </param>
        /// <param name="amount">金额 decimal（单位 元，最少一元起付）</param>
        /// <param name="desc">描述</param>
        /// <returns></returns>
        public static string TransfersBank(IServiceProvider serviceProvider, string outTradeNo, string BankCode, string EncBankNumber, string EncTrueName, decimal amount, string desc)
        {

            try
            {
                string cert = CertPath;//证书绝对路径
                string certPwd = CertPassword;//证书密码
                string nonceStr = TenPayV3Util.GetNoncestr();
                //获取rsa公钥
                // var presult = TenPayV3.GetPublicKey(null, new TenPayV3GetPublicKeyRequestData(MchId, nonceStr, MchIdkey));

                string pkey = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "Pay/Cert/publickey.pem");
                string encBankNumber = RASHelper.RSAEncrypt(EncBankNumber, pkey),
                    encTrueName = RASHelper.RSAEncrypt(EncTrueName, pkey);

                var xmlDataInfo = new TenPayV3PayBankRequestData(
                    MchId, //商户号
                    nonceStr, //随机字符串
                    MchIdkey, //商户号key
                    outTradeNo, //partner_trade_no商户订单号(只能是字母或者数字，不能包含有符号)
                    encBankNumber,
                    encTrueName,
                    BankCode,
                    amount.ToString("0"), //金额
                    desc //付款描述信息
                    );
                var result = TenPayV3.PayBank(serviceProvider, xmlDataInfo);
                LogHelper.Info("企业付款到银行卡 result:" + Newtonsoft.Json.JsonConvert.SerializeObject(result));
                return result.result_code == "SUCCESS" ? "成功" : result.err_code_des;
            }
            catch (Exception ex)
            {
                LogHelper.Error("【企业付款到银行卡】" + ex.TargetSite.Name + "【异常信息Message】：" + ex.Message);
            }
            return "";

        }


        #region 发起转账


        /// <summary>
        /// 发起转账
        /// </summary>
        /// <param name="outBillNo">流水号</param>
        /// <param name="openId"></param>
        /// <param name="amount">金额 decimal（单位 元，最少一元起付）</param>
        /// <param name="desc">描述</param>
        /// <param name="ip">请求Ip</param>
        /// <param name="payType">默认0 公众号支付，1小程序支付</param>
        /// <returns></returns>
        public static async Task<TransferBillReturnJson> Transfers(string outBillNo, string openId, decimal amount, string desc, string ip, int payType = 0)
        {
            var api = new FundAppApis();
            string payAppId = payType != 0 ? WxOpenAppId : AppId;//小程序:公众号
            try
            {
                var TransferAmount = Convert.ToInt32(amount * 100);
                var request = new TransferBillRequestData
                {
                    appid = payAppId,
                    out_bill_no = outBillNo,
                    transfer_scene_id = "1005",
                    openid = openId,
                    transfer_amount = TransferAmount,
                    transfer_remark = desc,
                    transfer_scene_report_infos =
                    [
                        new Transfer_Scene_Report_Info
                        {
                             info_type="岗位类型",
                              info_content=$"用户提现"
                        },
                        new Transfer_Scene_Report_Info
                        {
                             info_type="报酬说明",
                              info_content=$"提现金额{amount}元"
                        }
                    ]
                };
                LogHelper.Info("发起转账 request:" + Newtonsoft.Json.JsonConvert.SerializeObject(request));
                var result = await api.TransferBillAsync(request);
                LogHelper.Info("发起转账 result:" + Newtonsoft.Json.JsonConvert.SerializeObject(result));
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error("【发起转账】" + ex.TargetSite.Name + "【异常信息Message】：" + ex.Message);
            }
            return null;
        }

        public async static Task<QueryTransferReturnJson> TransferBillQueryByOutBillNo(string outBillNo)
        {
            var api = new FundAppApis();
            try
            {
                var request = new QueryTransferByOutBillNoRequestData
                {
                    out_bill_no = outBillNo
                };
                var result = await api.QueryTransferByOutBillNoAsync(request);
                LogHelper.Info("查询转账结果 result:" + Newtonsoft.Json.JsonConvert.SerializeObject(result));
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Error("【查询转账结果】" + ex.TargetSite.Name + "【异常信息Message】：" + ex.Message);
            }
            return null;
        }


        #endregion

        #endregion

        #region 红包
        /// <summary>
        /// 目前支持向指定微信用户的openid发放指定金额红包
        /// 注意total_amount、min_value、max_value值相同
        /// total_num=1固定
        /// 单个红包金额介于[1.00元，200.00元]之间
        /// </summary>
        public static void SendRedPack(string openId, string hostAddress, int amount, string sendName, string wishing, string act_name, string remark, int payType = 0, string scene_id = null)
        {
            string ip = hostAddress;
            string payAppid = AppId, parSecret = Secret;// 公众号配置
            string payMchId = MchId, payMchIdkey = MchIdkey;
            if (payType != 0)
            {
                //小程序配置
                payAppid = WxOpenAppId;
                parSecret = WxOpenSecret;
            }
            TenPayV3Info payInfo = new TenPayV3Info(payAppid, parSecret, payMchId, payMchIdkey, CertPath, CertPassword, null, null);

            string nonceStr;//随机字符串
            string paySign;//签名
            var cert = CertPath;//根据自己的证书位置修改

            var sendNormalRedPackResult = RedPackApi.SendNormalRedPack(
                payInfo.AppId, payInfo.MchId, payInfo.Key,
                cert,     //证书物理地址
               openId,   //接受收红包的用户的openId
               sendName,             //红包发送者名称
                ip,      //IP
                amount,                          //付款金额，单位分
                wishing,                 //红包祝福语
                act_name,                   //活动名称
                remark,                   //备注信息
                out nonceStr,
                out paySign,
                scene_id,                         //场景id（非必填）
                null,                         //活动信息（非必填）
                null                          //资金授权商户号，服务商替特约商户发放时使用（非必填）
                );

        }

        /// <summary>
        /// 裂变红包
        /// </summary>
        public static void SendGroupRedPack()
        {

        }
        #endregion
    }
}
