using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///商城商品规格
    // </summary>	

    [SugarTable("GoodSku")]
    public partial class GoodSku
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; } = 0;

        /// <summary>
        /// 商品ID
        /// </summary>

        public int goodsId { get; set; } = 0;

        /// <summary>
        /// 规格名称
        /// </summary>

        public string name { get; set; } = string.Empty;

        /// <summary>
        /// 文件号
        /// </summary>

        public string url { get; set; } = string.Empty;

        /// <summary>
        /// 市场价
        /// </summary>

        public decimal markPrice { get; set; } = 0M;

        /// <summary>
        /// 单价
        /// </summary>

        public decimal price { get; set; } = 0M;

        /// <summary>
        /// 实时库存
        /// </summary>

        public int stock { get; set; } = 0;

        /// <summary>
        /// 销量
        /// </summary>

        public int sale { get; set; } = 0;

        /// <summary>
        /// 排序
        /// </summary>

        public int sort { get; set; } = 0;

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

    }

    public class GoodSkuView : GoodSku
    {
        /// <summary>
        /// 商品名称
        /// </summary>
        public string goodsName { get; set; }

    }

    [Mapper]
    public partial class GoodSkuMapper
    {
        public partial GoodSkuView ToView(GoodSku model);
        public partial List<GoodSkuView> ToViewList(List<GoodSku> list);
        public partial GoodSku ToModel(GoodSkuView model);
    }
}

