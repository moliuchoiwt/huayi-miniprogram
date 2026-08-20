using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace YW.Common
{
    public class ExpressHelper
    {
        /// <summary>
        /// 快递100 获取数据接口
        /// </summary>
        /// <param name="customer">快递100分配的的公司编号</param>
        /// <param name="key">客户授权key</param>
        /// <param name="expressNo">快递单号</param>
        /// <param name="expressCode">快递Code</param>
        /// <returns></returns>
        public static async Task<string> getExpressData(string customer, string key, string expressNo, string expressCode, string phone)
        {
            try
            {
                string param = synQueryData(expressCode, expressNo, phone, "", "", 1);
                var sign = CommonHelper.Md5(param + key + customer);
                //LogHelper.Info("快递100 获取数据接口请求参数=" + param);
                FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    {"customer",customer },
                    {"sign",sign.ToUpper() },
                    {"param",param }
                });
                var myRequest = await new HttpClient().PostAsync("https://poll.kuaidi100.com/poll/query.do", content);

                if (myRequest.StatusCode == HttpStatusCode.OK)
                {
                    string res = await myRequest.Content.ReadAsStringAsync();
                    return res;
                }
                else
                {
                    //访问失败
                    return "False";
                }
            }
            catch (Exception e)
            {

                return e.Message;
            }
        }


        #region 获取快递Code

        private static async Task<string> getExpressCode(string key, string num)
        {
            string code = "";
            try
            {
                UTF8Encoding encoding = new UTF8Encoding();
                string postStrTpl = string.Format("http://www.kuaidi100.com/autonumber/auto?num={0}&key={1}", num, key);

                var myRequest = await new HttpClient().PostAsync(postStrTpl, null);
                if (myRequest.StatusCode == HttpStatusCode.OK)
                {
                    string res = await myRequest.Content.ReadAsStringAsync();
                    JObject jos = (JObject)JsonConvert.DeserializeObject(res);
                    code = jos["comCode"].ToString();
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("getExpressCode", e);
            }
            return code;

        }
        #endregion

        #region 实时查询快递单号

        /// <summary>
        ///  实时查询快递单号
        /// </summary>
        /// <param name="com">快递公司编码</param>
        /// <param name="num">快递单号</param>
        /// <param name="phone">手机号</param>
        /// <param name="from">出发地城市</param>
        /// <param name="to">目的地城市</param>
        /// <param name="resultv2">开通区域解析功能：0-关闭；1-开通</param>
        /// <returns></returns>
        private static string synQueryData(String com, String num, String phone, String from, String to, int resultv2)
        {

            StringBuilder param = new StringBuilder("{");
            param.Append("\"com\":\"").Append(com).Append("\"");
            param.Append(",\"num\":\"").Append(num).Append("\"");
            param.Append(",\"phone\":\"").Append(phone).Append("\"");
            param.Append(",\"from\":\"").Append(from).Append("\"");
            param.Append(",\"to\":\"").Append(to).Append("\"");
            if (1 == resultv2)
            {
                param.Append(",\"resultv2\":1");
            }
            else
            {
                param.Append(",\"resultv2\":0");
            }
            param.Append("}");

            return param.ToString();
        }
        #endregion
    }
}
