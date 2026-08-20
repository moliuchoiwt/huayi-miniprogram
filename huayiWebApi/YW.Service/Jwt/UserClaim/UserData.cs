namespace YW.Service.Jwt.UserClaim
{
    public class JwtData
    {
        /// <summary>
        /// 登录用户ID
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 登录用户名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 登录用户权限名称
        /// </summary>
        public string RoleName { get; set; }
        /// <summary>
        /// 登录用户权限
        /// </summary>
        public string Rules { get; set; }
    }
}
