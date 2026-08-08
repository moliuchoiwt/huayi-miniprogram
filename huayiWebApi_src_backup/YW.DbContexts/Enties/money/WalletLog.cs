using Riok.Mapperly.Abstractions;

namespace YW.DbContexts
{

    /// <summary>
    ///流水记录
    // </summary>	

    [SugarTable("WalletLog")]
    public partial class WalletLog
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 类型 0.余额   1.积分  2.佣金
        /// </summary>

        public int wType { get; set; }
        /// <summary>
        /// 关联店铺ID
        /// </summary>
        public int shopId { get; set; }

        /// <summary>
        /// 消费/来源类型
        /// </summary>

        public int sourceType { get; set; }

        /// <summary>
        /// 用户类型 0-用户 
        /// </summary>

        public int userType { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>

        public int userId { get; set; }



        /// <summary>
        /// 标题
        /// </summary>

        public string title { get; set; }

        /// <summary>
        /// 关联单号
        /// </summary>

        public string orderNo { get; set; }

        /// <summary>
        /// 金额
        /// </summary>

        public decimal change { get; set; }


        /// <summary>
        /// 备注
        /// </summary>

        public string remark { get; set; }

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
    ///流水记录
    // </summary>	

    public partial class WalletLogView : WalletLog
    {
        /// <summary>
        /// 用户昵称
        /// </summary>

        public string userName { get; set; } = string.Empty;
    }

    [Mapper]
    public partial class WalletLogMapper
    {
        public partial WalletLogView ToView(WalletLog model);
        public partial List<WalletLogView> ToViewList(List<WalletLog> list);
        public partial WalletLog ToModel(WalletLogView model);
    }
}

