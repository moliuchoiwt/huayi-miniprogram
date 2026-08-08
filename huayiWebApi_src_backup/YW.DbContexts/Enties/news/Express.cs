using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///运送方式表
    // </summary>	

    [SugarTable("Express")]
    public partial class Express
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 快递/物流名称
        /// </summary>

        public string name { get; set; }

        /// <summary>
        /// Code
        /// </summary>

        public string code { get; set; }

        /// <summary>
        /// 图标
        /// </summary>

        public string url { get; set; }

        /// <summary>
        /// 描述
        /// </summary>

        public string intro { get; set; }

        /// <summary>
        /// 状态 99-删除
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
    ///运送方式表
    // </summary>	

    public partial class ExpressView : Express
    {

    }
    [Mapper]
    public partial class ExpressMapper
    {
        public partial ExpressView ToView(Express model);
        public partial List<ExpressView> ToViewList(List<Express> list);
        public partial Express ToModel(ExpressView model);
    }
}

