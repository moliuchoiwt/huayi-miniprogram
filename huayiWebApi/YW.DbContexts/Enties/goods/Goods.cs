using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///商城商品
    // </summary>	

    [SugarTable("Goods")]
    public partial class Goods
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; } = 0;

        /// <summary>
        /// 分类ID
        /// </summary>

        public int classId { get; set; } = 0;

        /// <summary>
        /// 店铺id
        /// </summary>
        //public int shopId { get; set; } = 0;

        /// <summary>
        /// 商品类型 类型 0-生活区（普通商品）  1.感恩区（套餐商品）
        /// </summary>
        public int gType { get; set; } = 0;

        /// <summary>
        /// 名称
        /// </summary>

        public string name { get; set; } = string.Empty;

        ///// <summary>
        ///// 是否需要发货 
        ///// </summary>

        //public bool isDelivery { get; set; } = true;

        /// <summary>
        /// 封面图片
        /// </summary>

        public string coverPicture { get; set; } = string.Empty;

        /// <summary>
        /// 图片列表
        /// </summary>

        public string pictureList { get; set; } = string.Empty;

        /// <summary>
        /// 说明
        /// </summary>

        public string intro { get; set; } = string.Empty;

        /// <summary>
        /// 详情
        /// </summary>

        public string contents { get; set; } = string.Empty;


        /// <summary>
        /// Params
        /// </summary>

        public string parameter { get; set; } = string.Empty;


        /// <summary>
        /// 单价
        /// </summary>

        public decimal price { get; set; } = 0M;

        /// <summary>
        /// 库存
        /// </summary>

        public int stock { get; set; } = 0;


        /// <summary>
        /// 销售
        /// </summary>

        public int sale { get; set; } = 0;

        /// <summary>
        /// 排序
        /// </summary>

        public int sort { get; set; } = 0;

        /// <summary>
        /// 分享人奖励比例
        /// </summary>
        public decimal shareRate { get; set; } = 0M;


        /// <summary>
        /// 直推奖励比例
        /// </summary>
        public decimal oneRate { get; set; } = 0M;


        /// <summary>
        /// 间推奖励比例
        /// </summary>
        public decimal twoRate { get; set; } = 0M;

        /// <summary>
        /// 赠送积分数
        /// </summary>
        public decimal sendPoint { get; set; } = 0M;

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
        /// 热销
        /// </summary>
        public bool isHot { get; set; } = false;
        /// <summary>
        /// 是否新品
        /// </summary>
        public bool isNew { get; set; } = false;

        /// <summary>
        /// 是否首页推荐
        /// </summary>
        public bool isIndex { get; set; } = false;

        /// <summary>
        /// 创建管理员id
        /// </summary>
        public int createAdminId { get; set; } = 0;

    }

    /// <summary>
    ///商城商品
    // </summary>	

    public partial class GoodsView : Goods
    {
        /// <summary>
        /// 规格列表
        /// </summary>
        public List<GoodSku> skuList { get; set; }

        /// <summary>
        /// 图片列表
        /// </summary>
        public List<string> imgList { get; set; } = new List<string>();
        /// <summary>
        /// 店铺图
        /// </summary>
        public string shopUrl { get; set; }
        /// <summary>
        /// 店铺评分
        /// </summary>
        public decimal shopScore { get; set; }

        /// <summary>
        /// 是否收藏 
        /// </summary>
        public bool isCollection { get; set; } = false;

        /// <summary>
        /// 分类名称
        /// </summary>
        public string className { get; set; } = string.Empty;

    }




    /// <summary>
    ///  商城订单页面View
    /// </summary>
    public class OrderShopPageDto
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; }

        public string ShopLogo { get; set; }
        public List<OrderPageDto> GoodsList { get; set; }
    }

    /// <summary>
    ///商城订单页面商品列表
    /// </summary>
    public class OrderPageDto
    {
        //public int shopId { get; set; }
        /// <summary>
        /// 商品类型 类型 0-普通商品 1-秒杀商品 2-拼团商品 3-积分商品
        /// </summary>
        public int goodsType { get; set; }
        /// <summary>
        /// 商品ID
        /// </summary>
        public int goodsId { get; set; }
        public string goodsName { get; set; }
        /// <summary>
        /// 商品图
        /// </summary>
        public string goodsUrl { get; set; }
        /// <summary>
        /// 商品分类id
        /// </summary>
        public int goodsClassId { get; set; } = 0;
        /// <summary>
        /// 规格ID
        /// </summary>
        public int skuId { get; set; }
        /// <summary>
        /// 规格名称
        /// </summary>
        public string skuName { get; set; }
        /// <summary>
        /// 购买数量
        /// </summary>
        public int num { get; set; }
        /// <summary>
        /// 购买价格
        /// </summary>
        public decimal price { get; set; }
        /// <summary>
        /// 合计
        /// </summary>
        public decimal total { get; set; }
        ///// <summary>
        ///// 是否需要发货 0-否 1-是
        ///// </summary>
        //public bool isDelivery { get; set; } = false;

    }

    [Mapper]
    public partial class GoodsMapper
    {
        public partial GoodsView ToView(Goods model);
        public partial List<GoodsView> ToViewList(List<Goods> list);
        public partial Goods ToModel(GoodsView model);
    }

}

