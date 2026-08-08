using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///商城订单详情
    // </summary>	

    [SugarTable("GoodsOrderDetail")]
    public partial class GoodsOrderDetail
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 订单编号
        /// </summary>

        public string orderNo { get; set; }

        /// <summary>
        /// 详情编号
        /// </summary>

        public string detailNo { get; set; }

        /// <summary>
        /// 订单类型
        /// </summary>

        public int orderType { get; set; }

        /// <summary>
        /// 所属店铺ID
        /// </summary>

        //public int shopId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>

        public int userId { get; set; }

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
        /// 现金券支付金额
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

        ///// <summary>
        ///// 是否发货 0-否 1-是
        ///// </summary>
        //public bool isDelivery { get; set; } = false;

        /// <summary>
        /// 备注
        /// </summary>

        public string remark { get; set; }

        /// <summary>
        /// 状态
        /// </summary>

        public int status { get; set; }


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
    ///商城订单详情
    // </summary>	

    public partial class GoodsOrderDetailView : GoodsOrderDetail
    {
        /// <summary>
        /// 商品名称
        /// </summary>

        public string goodsName { get; set; } = string.Empty;

        /// <summary>
        /// 商品图
        /// </summary>

        public string goodsImg { get; set; } = string.Empty;
        /// <summary>
        /// 规格名称
        /// </summary>

        public string skuName { get; set; } = string.Empty;
        /// <summary>
        /// 商品分类id
        /// </summary>
        public int goodsClassId { get; set; } = 0;
    }

    [Mapper]
    public partial class GoodsOrderDetailMapper
    {
        public partial GoodsOrderDetailView ToView(GoodsOrderDetail model);
        public partial List<GoodsOrderDetailView> ToViewList(List<GoodsOrderDetail> list);
        public partial GoodsOrderDetail ToModel(GoodsOrderDetailView model);
        public partial List<GoodsOrderDetail> ToModelList(List<GoodsOrderDetailView> list);

    }
}

