using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    /// 
    // </summary>	

    [SugarTable("WebCategory")]
    public partial class WebCategory
    {
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        /// 父级名称
        /// </summary>
        public int ParentId { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        public int channelId { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        public string channelName { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string title { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string Intro { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string ImgUrl { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string ImgUrlList { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public int Sort { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        public int States { get; set; } = 0;
        /// <summary>
        /// 
        /// </summary>
        public DateTime CreateTime { get; set; } = DateTime.Now;
        /// <summary>
        /// 
        /// </summary>
        public DateTime UpdateTime { get; set; } = DateTime.Now;

        public string contents { get; set; } = string.Empty;

    }
    /// <summary>
    /// 
    // </summary>	

    public partial class WebCategoryView : WebCategory
    {

        public List<WebCategory> ChildrenList { get; set; }
    }
    [Mapper]
    public partial class WebCategoryMapper
    {
        public partial WebCategoryView ToView(WebCategory model);
        public partial List<WebCategoryView> ToViewList(List<WebCategory> list);
        public partial WebCategory ToModel(WebCategoryView model);
    }
}