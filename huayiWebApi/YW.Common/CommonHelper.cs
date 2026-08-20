using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace YW.Common
{
    public class CommonHelper
    {
        /// <summary>
        /// 获取当前请求域名
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDomain(HttpRequest request)
        {
            if (request == null) return string.Empty;
            return $"{request.Scheme}://{request.Host}{request.PathBase}";
        }

        /// <summary>
        /// 获取绝对路径
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static string GetMapPath(string strPath)
        {
            if (strPath.ToLower().StartsWith("http://"))
            {
                return strPath;
            }
            else //非web程序引用
            {
                strPath = strPath.Replace("/", "\\");
                if (strPath.StartsWith("\\"))
                {
                    strPath = strPath.Substring(strPath.IndexOf('\\', 0)).TrimStart('\\');
                }
                return System.IO.Path.Combine(Directory.GetCurrentDirectory(), strPath);
            }
        }

        /// <summary>
        /// 获取客户端IP
        /// </summary>
        /// <returns></returns>
        public static string GetIP()
        {
            HttpContextAccessor context = new HttpContextAccessor();
            var ip = context.HttpContext?.Connection.RemoteIpAddress.ToString();
            return ip;
        }

        /// <summary>
        /// 生成唯一字符串
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static string GenerateUniqueText(string code = "c")
        {
            var gid = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
            return (code + Timestamp() + gid).ToUpper();
        }

        /// <summary>
        /// 获取当前的时间戳
        /// </summary>
        /// <returns></returns>
        public static string Timestamp()
        {
            long ts = ConvertDateTimeToInt(DateTime.Now);
            return ts.ToString();
        }

        /// <summary>  
        /// 将c# DateTime时间格式转换为Unix时间戳格式  
        /// </summary>  
        /// <param name="time">时间</param>  
        /// <returns>long</returns>  
        public static long ConvertDateTimeToInt(System.DateTime time)
        {
            //System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            //long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            long t = (time.Ticks - 621356256000000000) / 10000;
            return t;
        }




        /// <summary>
        /// Md5加密
        /// </summary>
        /// <param name="txt"></param>
        /// <returns></returns>
        public static string Md5(string txt)
        {
            byte[] sor = Encoding.UTF8.GetBytes(txt);
            MD5 md5 = MD5.Create();
            byte[] result = md5.ComputeHash(sor);
            StringBuilder strbul = new StringBuilder(40);
            for (int i = 0; i < result.Length; i++)
            {
                //加密结果"x2"结果为32位,"x3"结果为48位,"x4"结果为64位
                strbul.Append(result[i].ToString("x2"));
            }
            return strbul.ToString();
        }


        #region 经纬度获取距离

        //地球半径，单位米
        private const double EARTH_RADIUS = 6378137;

        /// <summary>
        /// 计算两点位置的距离，返回两点的距离，单位：米
        /// 该公式为GOOGLE提供，误差小于0.2米
        /// </summary>
        /// <param name="lng1">第一点经度</param>
        /// <param name="lat1">第一点纬度</param>        
        /// <param name="lng2">第二点经度</param>
        /// <param name="lat2">第二点纬度</param>
        /// <returns></returns>
        public static double GetDistance(decimal lng1, decimal lat1, decimal lng2, decimal lat2)
        {

            double radLat1 = Rad(lat1);
            double radLng1 = Rad(lng1);
            double radLat2 = Rad(lat2);
            double radLng2 = Rad(lng2);
            double a = radLat1 - radLat2;
            double b = radLng1 - radLng2;
            double result = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2))) * EARTH_RADIUS;
            return result;
        }

        /// <summary>
        /// 经纬度转化成弧度
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        private static double Rad(decimal d)
        {
            return (double)d * Math.PI / 180d;
        }

        #endregion  
        /// <summary>
        /// ip获取地址
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public static async Task<Newtonsoft.Json.Linq.JObject> GetAddressByIP()
        {
            Newtonsoft.Json.Linq.JObject json = null;
            try
            {
                var ip = GetIP();
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync($"https://apis.map.qq.com/ws/location/v1/ip?ip={ip}&key=3J7BZ-UADC4-RERUZ-XSXSQ-LRBM3-BTBGO");
                response.EnsureSuccessStatusCode();
                //回复结果直接读成字符串
                string resp = await response.Content.ReadAsStringAsync();
                json = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(resp);

            }
            catch (Exception ex)
            {
                LogHelper.Error("经纬度转化成弧度错误：", ex);
            }

            return json;
        }

        /// <summary>
        /// 获取用来存放 管理员token 的 RedisKey
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public static string GetRedisAdminTokenKeyName(int adminId)
        {
            return $"AdminToken_{adminId}";
        }

        /// <summary>
        /// 获取用来存放 用户token 的 RedisKey
        /// </summary>
        /// <param name="adminId"></param>
        /// <returns></returns>
        public static string GetRedisUserTokenKeyName(int userId)
        {
            return $"UserToken_{userId}";
        }

        /// <summary>
        /// 当前时间是否在指定时间范围内     例如:8:00-22:00
        /// </summary>
        /// <param name="time">8:00-22:00</param>
        /// <returns></returns>
        public static bool IsCurrentTimeWithinTheSpecifiedTimeFrame(string time)
        {
            if (!time.Contains("-") && !time.Contains(":"))
            {
                return false;
            }
            try
            {
                var timeStr = time.Substring(time.Length - 11);
                var timeArr = timeStr.Split('-');
                var currTime = DateTime.Now;
                //判断时间                          

                //开始时间
                var StartTime = Convert.ToDateTime($"{currTime.ToString("yyyy/MM/dd")} {timeArr[0]}");
                //结束时间
                var EndTime = Convert.ToDateTime($"{currTime.ToString("yyyy/MM/dd")} {timeArr[1]}");

                if (currTime > StartTime && currTime < EndTime)
                {
                    return true;
                }
            }
            catch
            {

            }
            return false;
        }

    }
}
