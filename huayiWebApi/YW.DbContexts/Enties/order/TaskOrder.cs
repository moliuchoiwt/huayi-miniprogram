using Riok.Mapperly.Abstractions;

namespace YW.DbContexts
{
    /// <summary>
    /// 任务订单
    ///</summary>
    [SugarTable("TaskOrder")]
    public class TaskOrder
    {


        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "Id", IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; } = 0;

        public string orderNo { get; set; } = string.Empty;

        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "shopId")]

        public int shopId { get; set; } = 0;


        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "userId")]

        public int userId { get; set; } = 0;


        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "goodsImgs")]

        public string goodsImgs { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "price")]

        public decimal price { get; set; } = 0M;


        #region 支付信息
        /// <summary>
        /// 备  注:是否支付 
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "isPay")]

        public bool isPay { get; set; }


        /// <summary>
        /// 备  注:支付单号
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "payNo")]

        public string payNo { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:支付时间
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "payTime")]

        public DateTime payTime { get; set; } = DateTime.Now;


        /// <summary>
        /// 备  注:支付类型
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "payType")]

        public int payType { get; set; } = 0;


        /// <summary>
        /// 备  注:支付方式
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "payMent")]

        public string payMent { get; set; } = string.Empty;


        #endregion

        /// <summary>
        /// 备  注:备注
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "remarks")]

        public string remarks { get; set; } = string.Empty;

        #region 收货信息

        /// <summary>
        /// 备  注:收货人
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "consignee")]

        public string consignee { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:收货号码
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "mobile")]

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
        /// 备  注:收货地址
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "address")]

        public string address { get; set; } = string.Empty;
        /// <summary>
        /// 收货方式 0:物流 1:自提
        /// </summary>
        public int receivingType { get; set; } = 0;
        /// <summary>
        /// 收货时间
        /// </summary>
        public DateTime receivingTime { get; set; } = DateTime.Now;

        #endregion

        #region 发货信息

        /// <summary>
        /// 备  注:快递Code
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "expressCode")]

        public string expressCode { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:快递名称
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "expressName")]

        public string expressName { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:快递单号
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "logisticsNo")]

        public string logisticsNo { get; set; } = string.Empty;

        /// <summary>
        /// 配送方式 0:物流 1:自提
        /// </summary>
        public int deliveryType { get; set; } = 0;

        /// <summary>
        /// 配送时间
        /// </summary>
        public DateTime deliveryTime { get; set; } = DateTime.Now;

        #endregion

        /// <summary>
        /// 备  注:状态
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "status")]

        public int status { get; set; } = 0;


        /// <summary>
        /// 备 注:内容审核状态 0-待审核 1-已通过 2-已拒绝
        /// </summary>
        [SugarColumn(ColumnName = "auditState")]
        public int auditState { get; set; } = 0;

        /// <summary>
        /// 备 注:审核意见
        /// </summary>
        [SugarColumn(ColumnName = "auditIntro")]
        public string auditIntro { get; set; } = string.Empty;

        /// <summary>
        /// 备  注:创建时间
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "createTime")]

        public DateTime createTime { get; set; } = DateTime.Now;


        /// <summary>
        /// 备  注:更新时间
        /// 默认值:
        ///</summary>
        [SugarColumn(ColumnName = "updateTime")]

        public DateTime updateTime { get; set; } = DateTime.Now;


        /// <summary>
        /// 相关需求
        /// </summary>
        public string relatedDemand { get; set; } = string.Empty;
        public decimal userRefundAmount { get; set; } = 0M;
        public decimal shopRefundAmount { get; set; } = 0M;
    }

    /// <summary>
    /// 任务订单
    // </summary>	

    public class TaskOrderView : TaskOrder
    {
        [MapperIgnore]
        public List<string> goodsImgList { get; set; }

        [MapperIgnore]
        public string shopName { get; set; } = string.Empty;
        [MapperIgnore]
        public string userName { get; set; } = string.Empty;
        [MapperIgnore]
        public string receivingTypeName { get; set; } = string.Empty;

    }

    public class TaskOrderQuery : QueryModel
    {
        public int? status { get; set; }
    }

    /// <summary>
    /// 任务订单状态枚举
    /// </summary>
    public enum TaskOrderStateEnum
    {
        待付款 = 0,
        待审核 = 1,
        已发布 = 2,
        进行中 = 3,
        待收货 = 4,
        已完成 = 5,
        售后中 = 6,
        售后完成 = 7,
        已取消 = 8,
        已驳回 = 9,
        已删除 = 99
    }

    /// <summary>
    /// 订单收货方式枚举
    /// </summary>
    public enum TaskOrderReceivingTypeEnum
    {
        物流 = 0,
        自提 = 1
    }

    [Mapper]
    public partial class TaskOrderMapper
    {
        public partial TaskOrderView ToView(TaskOrder model);
        public partial List<TaskOrderView> ToViewList(List<TaskOrder> list);
        public partial TaskOrder ToModel(TaskOrderView model);
    }

}