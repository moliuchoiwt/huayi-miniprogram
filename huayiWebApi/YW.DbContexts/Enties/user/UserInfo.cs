using Riok.Mapperly.Abstractions;

namespace YW.DbContexts
{

    /// <summary>
    ///用户信息表
    // </summary>	

    [SugarTable("UserInfo")]
    public partial class UserInfo
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; } = 0;


        /// <summary>
        /// Code
        /// </summary>

        public string code { get; set; } = string.Empty;

        /// <summary>
        /// 微信昵称
        /// </summary>

        public string nickName { get; set; } = string.Empty;

        /// <summary>
        /// Avatar
        /// </summary>

        public string avatar { get; set; } = string.Empty;

        /// <summary>
        /// Mobile
        /// </summary>

        public string mobile { get; set; } = string.Empty;

        /// <summary>
        /// 性别
        /// </summary>

        public string gender { get; set; } = string.Empty;

        /// <summary>
        /// 微信UnionId
        /// </summary>

        public string wxUnionId { get; set; } = string.Empty;

        /// <summary>
        /// 小程序openid
        /// </summary>

        public string wxAppletsOpenId { get; set; } = string.Empty;

        /// <summary>
        /// 公众号openid
        /// </summary>

        public string wxMpOpenId { get; set; } = string.Empty;

        /// <summary>
        /// 开发平台openid
        /// </summary>

        public string wxAppOpenId { get; set; } = string.Empty;

        /// <summary>
        /// 别名
        /// </summary>

        public string alias { get; set; } = string.Empty;

        /// <summary>
        /// ParentId
        /// </summary>

        public int parentId { get; set; } = 0;

        #region 所在地址
        /// <summary>
        /// 省
        /// </summary>

        public string province { get; set; } = string.Empty;

        /// <summary>
        /// 市
        /// </summary>

        public string city { get; set; } = string.Empty;

        /// <summary>
        /// 区
        /// </summary>

        public string area { get; set; } = string.Empty;
        /// <summary>
        /// 地址
        /// </summary>
        public string address { get; set; } = string.Empty;

        #endregion

        /// <summary>
        ///摘要/履历 个人简介
        /// </summary>
        public string intro { get; set; } = string.Empty;
        /// <summary>
        /// 年龄
        /// </summary>
        public int age { get; set; } = 0;
        /// <summary>
        /// 生日
        /// </summary>
        public DateOnly birthday { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        /// <summary>
        /// 积分
        /// </summary>

        public decimal integral { get; set; } = 0M;

        /// <summary>
        /// 余额
        /// </summary>

        public decimal amount { get; set; } = 0M;

        /// <summary>
        /// 佣金
        /// </summary>
        public decimal commission { get; set; } = 0M;

        /// <summary>
        /// Ip
        /// </summary>

        public string ip { get; set; }

        /// <summary>
        /// 状态 0-正常 1-冻结 99-删除
        /// </summary>

        public int status { get; set; }

        /// <summary>
        /// CreateTime
        /// </summary>

        public DateTime createTime { get; set; } = DateTime.Now;

        /// <summary>
        /// UpdateTime
        /// </summary>

        public DateTime updateTime { get; set; } = DateTime.Now;

    }

    /// <summary>
    ///用户信息表
    // </summary>	

    public partial class UserInfoView : UserInfo
    {
        /// <summary>
        /// ParentName
        /// </summary>

        public string parentName { get; set; } = string.Empty;

    }

    public class UserInfoQuery : QueryModel
    {

    }


    [Mapper]
    public partial class UserInfoMapper
    {
        public partial UserInfoView ToView(UserInfo model);
        public partial List<UserInfoView> ToViewList(List<UserInfo> list);
        public partial UserInfo ToModel(UserInfoView model);
    }

}

