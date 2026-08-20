namespace YW.Service.Jwt
{
    public class JwtSetting
    {
        /// <summary>
        /// 颁发者
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// 受众
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// 安全密钥，至少要16个字符
        /// </summary>
        public string SecurityKey { get; set; }

        /// <summary>
        /// 过期时间,单位:分钟，注意JWT有自己默认的缓冲过期时间（五分钟）
        /// </summary>
        public double LifeTime { get; set; }
    }
}
