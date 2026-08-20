namespace YW.DbContexts.WebCore
{
    public class SiteSetting
    {
        /// <summary>
        /// 雪花算法的参数
        /// </summary>
        public long WorkerId { get; set; }
        /// <summary>
        /// 雪花算法的参数
        /// </summary>
        public long DataCenterId { get; set; }
        /// <summary>
        /// 是用户登录失败的次数限制
        /// </summary>
        public int LoginFailedCountLimits { get; set; }
        /// <summary>
        /// 用户锁定后，多久可以重新登录
        /// </summary>
        public int LoginLockedTimeout { get; set; }
    }
}
