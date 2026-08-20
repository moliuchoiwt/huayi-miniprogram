using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///售后记录
    // </summary>	

    [SugarTable("AfterSale")]
    public partial class AfterSale
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// ShopId
        /// </summary>

        //public int shopId { get; set; }

        /// <summary>
        /// 类型 0-用户申请退款 
        /// </summary>

        public int type { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>

        public int userId { get; set; }


        /// <summary>
        /// 订单编号
        /// </summary>

        public string orderNo { get; set; }

        /// <summary>
        /// 订单详情编号
        /// </summary>

        public string detailNo { get; set; }

        /// <summary>
        /// OrderState
        /// </summary>

        public int orderState { get; set; }

        /// <summary>
        /// PayNo
        /// </summary>

        public string payNo { get; set; }

        /// <summary>
        /// 商品ID
        /// </summary>

        public int goodsId { get; set; }



        /// <summary>
        /// 规格ID
        /// </summary>

        public int skuId { get; set; }



        /// <summary>
        /// 售价
        /// </summary>

        public decimal price { get; set; }

        /// <summary>
        /// 数量
        /// </summary>

        public int num { get; set; }

        /// <summary>
        /// 总金额
        /// </summary>

        public decimal total { get; set; }

        /// <summary>
        /// 优惠券ID
        /// </summary>

        public decimal couponId { get; set; }

        /// <summary>
        /// 优惠券抵扣金额
        /// </summary>

        public decimal couponPay { get; set; }

        /// <summary>
        /// 折扣金额
        /// </summary>

        public decimal disCountTotal { get; set; }

        /// <summary>
        /// 实付金额=总结-优惠券-折扣金额
        /// </summary>

        public decimal realTotal { get; set; }
        /// <summary>
        /// 现金券
        /// </summary>
        public decimal cashPay { get; set; }

        /// <summary>
        /// 余额支付金额
        /// </summary>

        public decimal balancePay { get; set; }

        /// <summary>
        /// 积分支付金额
        /// </summary>

        public decimal integralPay { get; set; }

        /// <summary>
        /// 使用积分数
        /// </summary>

        public decimal useIntegral { get; set; }

        /// <summary>
        /// 支付金额
        /// </summary>

        public decimal amount { get; set; }

        /// <summary>
        /// 退款原因 
        /// </summary>

        public string note { get; set; }

        /// <summary>
        /// 文件编号 
        /// </summary>

        public string url { get; set; }

        /// <summary>
        /// 备注 
        /// </summary>

        public string remark { get; set; }

        /// <summary>
        /// 状态  0-待审核  1-通过 2-驳回 
        /// </summary>

        public int status { get; set; }

        /// <summary>
        /// 审核信息 
        /// </summary>

        public string auditIntro { get; set; }

        /// <summary>
        /// AuditUrl
        /// </summary>

        public string auditUrl { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime createTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>

        public DateTime updateTime { get; set; } = DateTime.Now;

    }

    public class AfterSaleView : AfterSale
    {
        /// <summary>
        /// 订单id
        /// </summary>
        public int? orderId { get; set; }
        /// <summary>
        /// 图片列表
        /// </summary>
        public List<string> imgList { get; set; }
        /// <summary>
        /// 商品名称
        /// </summary>

        public string goodsName { get; set; }

        /// <summary>
        /// 商品图
        /// </summary>

        public string goodsImg { get; set; }
        /// <summary>
        /// 规格名称
        /// </summary>

        public string skuName { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>

        public string userName { get; set; }
    }

    [Mapper]
    public partial class AfterSaleMapper
    {
        public partial AfterSaleView ToView(AfterSale model);
        public partial List<AfterSaleView> ToViewList(List<AfterSale> list);
        public partial AfterSale ToModel(AfterSaleView model);
    }
}

