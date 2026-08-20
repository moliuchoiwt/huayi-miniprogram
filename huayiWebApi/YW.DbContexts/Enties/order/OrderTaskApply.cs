using Riok.Mapperly.Abstractions;

namespace YW.DbContexts
{
    /// <summary>
    /// 订单任务申请列表
    ///</summary>
    [SugarTable("OrderTaskApply")]
    public class OrderTaskApply
    {


        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "Id", IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; } = 0;


        /// <summary>
        /// 备  注:用户id
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "userId")]

        public int userId { get; set; } = 0;


        /// <summary>
        /// 备  注:订单号
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "orderNo")]

        public string orderNo { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:状态 0.已申请 1.已被商家接取 99.已取消
        /// 默认值:0
        ///</summary>
        [SugarColumn(ColumnName = "status")]

        public int status { get; set; } = 0;


        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "createTime")]

        public DateTime createTime { get; set; } = DateTime.Now;
        public DateTime updateTime { get; set; } = DateTime.Now;



    }

    /// <summary>
    /// 订单任务申请列表
    // </summary>	

    public partial class OrderTaskApplyView : OrderTaskApply
    {
        public string nickName { get; set; } = string.Empty;
        public string avatar { get; set; } = string.Empty;
        public string gender { get; set; } = string.Empty;
        public string intro { get; set; } = string.Empty;
        public string province { get; set; } = string.Empty;
        public string city { get; set; } = string.Empty;
        public string area { get; set; } = string.Empty;
        public List<string> goodsImgList { get; set; }
        public int shopId { get; set; } = 0;
        public decimal price { get; set; } = 0M;
        public DateTime receivingTime { get; set; } = DateTime.Now;
        public string relatedDemand { get; set; } = string.Empty;
        public int orderId { get; set; } = 0;
        public string statusName { get; set; } = string.Empty;
    }

    public class OrderTaskApplyQuery : QueryModel
    {
        public string orderNo { get; set; } = string.Empty;
    }

    /// <summary>
    /// 用户任务订单状态枚举
    /// </summary>
    public enum TaskOrderApplyStateEnum
    {
        已申请 = 0,
        进行中 = 1,
        待确认 = 2,
        已完成 = 3,
        售后完成 = 4,
        已删除 = 99
    }


    [Mapper]
    public partial class OrderTaskApplyMapper
    {
        public partial OrderTaskApplyView ToView(OrderTaskApply model);
        public partial List<OrderTaskApplyView> ToViewList(List<OrderTaskApply> list);
        public partial OrderTaskApply ToModel(OrderTaskApplyView model);
    }

}