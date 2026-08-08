using Riok.Mapperly.Abstractions;

namespace YW.DbContexts
{

    /// <summary>
    ///用户地址
    // </summary>	

    [SugarTable("UserAddress")]
    public partial class UserAddress
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; } = 0;

        /// <summary>
        /// 用户ID
        /// </summary>

        public int userId { get; set; } = 0;



        /// <summary>
        /// 收货人
        /// </summary>

        public string consignee { get; set; } = string.Empty;

        /// <summary>
        /// 联系号码
        /// </summary>

        public string mobile { get; set; } = string.Empty;

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

        /// <summary>
        /// 备注
        /// </summary>

        public string remark { get; set; } = string.Empty;

        /// <summary>
        /// 是否默认地址 1-是
        /// </summary>

        public bool isDefault { get; set; } = false;

        /// <summary>
        /// 状态 0-可用 99-删除
        /// </summary>

        public int status { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime createTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>

        public DateTime updateTime { get; set; } = DateTime.Now;

    }

    /// <summary>
    ///用户地址
    // </summary>	

    public partial class UserAddressView : UserAddress
    {
        /// <summary>
        /// 用户名称
        /// </summary>

        public string userName { get; set; } = string.Empty;
    }

    [Mapper]
    public partial class UserAddressMapper
    {
        public partial UserAddressView ToView(UserAddress model);
        public partial List<UserAddressView> ToViewList(List<UserAddress> list);
        public partial UserAddress ToModel(UserAddressView model);
    }

}

