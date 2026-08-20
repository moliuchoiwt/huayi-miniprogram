using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using System;
using System.Text;

namespace YW.Common
{
   public class RedisCacheHelper
    {
        private static RedisCache _redisCache = null;
        private static RedisCacheOptions options = null;
        /// <summary>
        ///
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="instanceName"></param>
        public RedisCacheHelper(string connectionString, string instanceName)
        {
            options = new RedisCacheOptions();
            options.Configuration = connectionString;
            options.InstanceName = instanceName;
            _redisCache = new RedisCache(options);
        }
        /// <summary>
        /// 初始化Redis
        /// </summary>
        public static void InitRedis(string connectionString, string instanceName)
        {
            options = new RedisCacheOptions();
            options.Configuration = connectionString;
            options.InstanceName = instanceName;
            _redisCache = new RedisCache(options);
        }
        /// <summary>
        /// 添加string数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="ExprireTime">过期时间 单位小时</param>
        /// <returns></returns>
        public static bool SetStringValue(string key, string value, int ExprireTime = 24)
        {
            if (string.IsNullOrEmpty(key)|| _redisCache==null)
            {
                return false;
            }
            try
            {
                _redisCache.SetString(key, value, new DistributedCacheEntryOptions()
                {
                    AbsoluteExpiration = DateTime.Now.AddHours(ExprireTime)
                });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 获取string数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static string GetStringValue(string key)
        {
            if (string.IsNullOrEmpty(key) || _redisCache == null)
            {
                return null;
            }
            try
            {
                return _redisCache.GetString(key);
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 获取数据（对象）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键</param>
        /// <returns></returns>
        public static T Get<T>(string key)
        {
            string value = GetStringValue(key);
            if (string.IsNullOrEmpty(value))
            {
                return default(T);
            }
            try
            {
                var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(value);
                return obj;
            }
            catch (Exception)
            {
                return default(T);
            }
        }
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="key">键</param>
        public static bool Remove(string key)
        {
            if (string.IsNullOrEmpty(key) || _redisCache == null)
            {
                return false;
            }
            try
            {
                _redisCache.Remove(key);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 刷新数据
        /// </summary>
        /// <param name="key">键</param>
        public static bool Refresh(string key)
        {
            if (string.IsNullOrEmpty(key) || _redisCache == null)
            {
                return false;
            }
            try
            {
                _redisCache.Refresh(key);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// 重置数据
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="expireTime">过期时间 单位小时</param>
        public static bool Replace(string key, string value, int expireTime = 24)
        {
            if (Remove(key))
            {
                return SetStringValue(key, value, expireTime);
            }
            else
            {
                return false;
            }
        }
    }
}

