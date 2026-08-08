using Riok.Mapperly.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace YW.DbContexts
{

    /// <summary>
    ///商城订单
    // </summary>	

    [SugarTable("GoodsOrder")]
    public partial class GoodsOrder
    {

        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; } = 0;

        /// <summary>
        /// 订单类型 
        /// </summary>

        public int orderType { get; set; } = 0;

        /// <summary>
        /// 店铺ID
        /// </summary>

        public int shopId { get; set; } = 0;

        ///// <summary>
        ///// 是否发货 0-否 1-是
        ///// </summary>

        //public bool isDelivery { get; set; } = false;

        /// <summary>
        /// 用户ID
        /// </summary>

        public int userId { get; set; } = 0;



        /// <summary>
        /// 分享用户ID
        /// </summary>

        public int shareUserId { get; set; } = 0;

        /// <summary>
        /// 订单编号
        /// </summary>

        public string orderNo { get; set; } = string.Empty;


        /// <summary>
        /// 总金额
        /// </summary>

        public decimal total { get; set; } = 0M;

        /// <summary>
        /// 优惠券
        /// </summary>

        public decimal couponId { get; set; } = 0M;

        /// <summary>
        /// 优惠券抵扣金额
        /// </summary>

        public decimal couponPay { get; set; } = 0M;

        /// <summary>
        /// 折扣金额
        /// </summary>

        public decimal disCountTotal { get; set; } = 0M;

        /// <summary>
        /// 现金券支付金额
        /// </summary>
        public decimal cashPay { get; set; } = 0M;

        /// <summary>
        /// 余额支付金额
        /// </summary>

        public decimal balancePay { get; set; } = 0M;

        /// <summary>
        /// 积分支付金额
        /// </summary>

        public decimal integralPay { get; set; } = 0M;

        /// <summary>
        /// 使用积分数
        /// </summary>

        public decimal useIntegral { get; set; } = 0M;

        /// <summary>
        /// 支付金额
        /// </summary>

        public decimal amount { get; set; } = 0M;

        /// <summary>
        /// 是否支付 0-否 1-是
        /// </summary>

        public bool isPay { get; set; } = false;

        /// <summary>
        /// 支付单号
        /// </summary>

        public string payNo { get; set; } = string.Empty;

        /// <summary>
        /// 支付时间
        /// </summary>

        public DateTime payTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 支付类型
        /// </summary>

        public int payType { get; set; } = 0;

        /// <summary>
        /// 支付方式
        /// </summary>

        public string payMent { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>

        public string remarks { get; set; } = string.Empty;

        /// <summary>
        /// 状态
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

        /// <summary>
        /// 是否微信发货
        /// </summary>
        public bool isWxDeliverGoods { get; set; } = false;
        /// <summary>
        /// 运费
        /// </summary>
        public decimal freight { get; set; } = 0M;

        /// <summary>
        /// 商品图片
        /// </summary>
        public string goodsImgs { get; set; } = string.Empty;


    }

    public class GoodsOrderView : GoodsOrder
    {
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string shopName { get; set; }

        /// <summary>
        /// 订单详情
        /// </summary>
        public List<GoodsOrderDetailView> dlist { get; set; }
        /// <summary>
        /// 订单售后
        /// </summary>
        public List<AfterSaleView> rlist { get; set; }
        /// <summary>
        /// 订单发货
        /// </summary>
        public List<Logistics> loglist { get; set; }
        /// <summary>
        /// 购买数量
        /// </summary>
        public decimal buyTotal { get; set; } = 0M;
        /// <summary>
        /// 收货地址ID
        /// </summary>
        [RegularExpression(@"^[+]{0,1}(\d+)$", ErrorMessage = "收货地址ID不能小于0")]
        public int addressId { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>

        public string userName { get; set; } = string.Empty;
    }

    public class OrderView
    {
        /// <summary>
        /// 订单类型  1.商品订单
        /// </summary>
        public int type { get; set; } = 0;

        /// <summary>
        /// 店铺ID
        /// </summary>
        [RegularExpression(@"^[+]{0,1}(\d+)$", ErrorMessage = "店铺ID不能小于0")]
        public int shopId { get; set; }
        /// <summary>
        /// 商品ID
        /// </summary>
        [RegularExpression(@"^[+]{0,1}(\d+)$", ErrorMessage = "商品ID不能小于0")]
        public int goodsId { get; set; }
        /// <summary>
        /// 商品规格ID
        /// </summary>
        [RegularExpression(@"^[+]{0,1}(\d+)$", ErrorMessage = "规格ID不能小于0")]
        public int skuId { get; set; }

        /// <summary>
        /// 购买数量
        /// </summary>
        [RegularExpression(@"^[+]{0,1}(\d+)$", ErrorMessage = "购买数量不能小于0")]
        public int num { get; set; }

        /// <summary>
        /// 购物车ID集合
        /// </summary>
        public List<int> cartIds { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        [RegularExpression(@"^((0{1}\.\d{1,2})|([1-9]\d*\.{1}\d{1,2})|([1-9]+\d*)|0)$", ErrorMessage = "金额不能小于0")]
        public decimal money { get; set; } = 0M;

        /// <summary>
        /// 优惠券
        /// </summary>
        [Display(Name = "优惠券ID")]
        [RegularExpression(@"^[+]{0,1}(\d+)$", ErrorMessage = "优惠券ID不能小于0")]
        public int couponConsumeId { get; set; }
        /// <summary>
        /// 是否积分支付
        /// </summary>                
        public bool isIntegral { get; set; } = false;

        /// <summary>
        /// 是否余额支付
        /// </summary>

        public bool isBalance { get; set; } = false;
        /// <summary>
        /// 用户订单备注
        /// </summary>
        public string remarks { get; set; } = string.Empty;
        /// <summary>
        /// 收货地址ID
        /// </summary>
        [RegularExpression(@"^[+]{0,1}(\d+)$", ErrorMessage = "收货地址ID不能小于0")]
        public int? addressId { get; set; }

        /// <summary>
        /// 收货方式 0.快递 1.自提
        /// </summary>
        public int deliverType { get; set; } = 0;

        /// <summary>
        /// 收货时间
        /// </summary>
        public DateTime deliverTime { get; set; } = DateTime.Now;


    }




    /// <summary>
    /// 订单发起支付
    /// </summary>
    public class OrderPayView
    {
        /// <summary>
        /// 订单编号
        /// </summary>

        [Display(Name = "订单编号")]
        [Required(ErrorMessage = "{0}必填")]
        public string orderNo { get; set; }
        /// <summary>
        /// 支付方式
        /// </summary>
        [RegularExpression(@"^[+]{0,1}(\d+)$", ErrorMessage = "支付方式不能小于0")]
        public int payType { get; set; }
    }

    public class GoodsOrderQuery : QueryModel
    {
        public int? status { get; set; }
        public int? orderType { get; set; }

    }


    [Mapper]
    public partial class GoodsOrderMapper
    {
        public partial GoodsOrderView ToView(GoodsOrder model);
        public partial List<GoodsOrderView> ToViewList(List<GoodsOrder> list);
        public partial GoodsOrder ToModel(GoodsOrderView model);
    }
}

