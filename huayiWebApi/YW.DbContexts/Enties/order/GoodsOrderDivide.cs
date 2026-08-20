using Riok.Mapperly.Abstractions;

namespace YW.DbContexts
{

    /// <summary>
    ///商城订单分成记录
    // </summary>	

    [SugarTable("GoodsOrderDivide")]
    public partial class GoodsOrderDivide
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; } = 0;

        /// <summary>
        /// 订单编号
        /// </summary>

        public string orderNo { get; set; } = string.Empty;


        /// <summary>
        /// 用户ID
        /// </summary>

        public int userId { get; set; } = 0;

        /// <summary>
        /// 分红类型
        /// </summary>
        public int dType { get; set; } = 0;

        /// <summary>
        /// 分红比例
        /// </summary>
        public decimal dRatio { get; set; } = 0M;

        /// <summary>
        /// 分红金额
        /// </summary>
        public decimal dAmount { get; set; } = 0M;

        /// <summary>
        /// 关联流水
        /// </summary>
        public int wLogId { get; set; } = 0;

        /// <summary>
        /// 状态 0.未支付 1.未分红 2.已分红
        /// </summary>

        public int status { get; set; } = 0;

        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime createTime { get; set; } = DateTime.Now;


    }

    /// <summary>
    /// 订单分红记录类型
    /// </summary>
    public enum OrderDivideTypeEnum
    {
        直推分红 = 0
    }

    /// <summary>
    ///商城订单分成记录
    // </summary>	

    public partial class GoodsOrderDivideView : GoodsOrderDivide
    {
    }

    [Mapper]
    public partial class GoodsOrderDivideMapper
    {
        public partial GoodsOrderDivideView ToView(GoodsOrderDivide model);
        public partial List<GoodsOrderDivideView> ToViewList(List<GoodsOrderDivide> list);
        public partial GoodsOrderDivide ToModel(GoodsOrderDivideView model);
    }
}

