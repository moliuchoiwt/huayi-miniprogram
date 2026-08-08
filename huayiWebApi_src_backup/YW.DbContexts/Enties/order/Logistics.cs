using Riok.Mapperly.Abstractions;

namespace YW.DbContexts
{

    /// <summary>
    ///订单物流
    // </summary>	

    [SugarTable("Logistics")]
    public partial class Logistics
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; } = 0;

        /// <summary>
        /// ShopId
        /// </summary>

        public int shopId { get; set; } = 0;

        /// <summary>
        /// 订单编号
        /// </summary>

        public string orderNo { get; set; } = string.Empty;

        /// <summary>
        /// 订单详情编号
        /// </summary>

        public string detailNo { get; set; } = string.Empty;

        /// <summary>
        /// 物流类型 0.物流发货  1.线下自提
        /// </summary>

        public int logisticsType { get; set; } = 0;



        /// <summary>
        /// GoodsInfo
        /// </summary>

        public string goodsInfo { get; set; } = string.Empty;

        /// <summary>
        /// 用户
        /// </summary>

        public int userId { get; set; } = 0;



        /// <summary>
        /// 收货人
        /// </summary>

        public string consignee { get; set; } = string.Empty;

        /// <summary>
        /// 收货号码
        /// </summary>

        public string mobile { get; set; } = string.Empty;

        /// <summary>
        /// 收货地址
        /// </summary>

        public string address { get; set; } = string.Empty;

        /// <summary>
        /// 快递Id
        /// </summary>

        public int expressId { get; set; } = 0;

        /// <summary>
        /// 快递图片
        /// </summary>

        public string expressUrl { get; set; } = string.Empty;

        /// <summary>
        /// 快递Code
        /// </summary>

        public string expressCode { get; set; } = string.Empty;

        /// <summary>
        /// 快递名称
        /// </summary>

        public string expressName { get; set; } = string.Empty;

        /// <summary>
        /// 快递单号
        /// </summary>

        public string logisticsNo { get; set; } = string.Empty;

        /// <summary>
        /// 状态 -1:待支付 0-待发货 1-已发货 2-已收货
        /// </summary>

        public int status { get; set; } = -1;
        /// <summary>
        /// 是否自提 1-是
        /// </summary>
        public bool isUp { get; set; } = false;

        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime createTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>

        public DateTime updateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 收货方式 0-物流发货 1-自提
        /// </summary>
        public int deliverType { get; set; } = 0;

        /// <summary>
        /// 交货时间
        /// </summary>
        public DateTime deliverTime { get; set; } = DateTime.Now;


    }
    /// <summary>
    ///订单物流
    // </summary>	

    public partial class LogisticsView : Logistics
    {
        /// <summary>
        /// GoodsUrl
        /// </summary>

        public string goodsUrl { get; set; } = string.Empty;

        /// <summary>
        /// 用户
        /// </summary>

        public string userName { get; set; } = string.Empty;

        /// <summary>
        /// 商家名称
        /// </summary>
        public string shopName { get; set; } = string.Empty;
    }

    [Mapper]
    public partial class LogisticsMapper
    {
        public partial LogisticsView ToView(Logistics model);
        public partial List<LogisticsView> ToViewList(List<Logistics> list);
        public partial Logistics ToModel(LogisticsView model);
    }

}

