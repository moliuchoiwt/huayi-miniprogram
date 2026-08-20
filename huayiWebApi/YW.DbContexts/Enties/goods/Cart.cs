using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///购物车
    // </summary>	

    [SugarTable("Cart")]
    public partial class Cart
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 店铺ID
        /// </summary>

        //public int shopId { get; set; }

        /// <summary>
        /// 用户
        /// </summary>

        public int userId { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>

        public int goodsId { get; set; }


        /// <summary>
        /// 规格Id
        /// </summary>

        public int skuId { get; set; }


        /// <summary>
        /// 数量
        /// </summary>

        public int num { get; set; }

        /// <summary>
        /// 状态 99-删除
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

    public class CartView : Cart
    {

        /// <summary>
        /// 库存数量
        /// </summary>
        public int stock { get; set; }

        /// <summary>
        /// 规格名称
        /// </summary>

        public string skuName { get; set; }
        /// <summary>
        /// 商品图
        /// </summary>

        public string goodsUrl { get; set; }

        /// <summary>
        /// 商品名称
        /// </summary>

        public string goodsName { get; set; }

        /// <summary>
        /// 单价
        /// </summary>

        public decimal price { get; set; }
        /// <summary>
        /// 店铺名称
        /// </summary>
        public string shopName { get; set; }

    }

    [Mapper]
    public partial class CartMapper
    {
        public partial CartView ToView(Cart model);
        public partial List<CartView> ToViewList(List<Cart> list);
        public partial Cart ToModel(CartView model);
    }

}

