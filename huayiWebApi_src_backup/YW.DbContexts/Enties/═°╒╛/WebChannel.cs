using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    /// 频道列表
    // </summary>	

    [SugarTable("WebChannel")]
    public partial class WebChannel
    {
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; } = 0;
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
        public string contents { get; set; } = string.Empty;


        /// <summary>
        /// 网站标题
        /// </summary>
        public string seo_title { get; set; } = string.Empty;
        /// <summary>
        /// 网站描述
        /// </summary>
        public string seo_description { get; set; } = string.Empty;
        /// <summary>
        /// 网站关键词
        /// </summary>
        public string seo_keywords { get; set; } = string.Empty;
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
    }

    /// <summary>
    /// 频道列表
    // </summary>	

    public partial class WebChannelView : WebChannel
    {
        /// <summary>
        /// 图片列表
        /// </summary>
        public List<string> ImgList { get; set; }

    }

    [Mapper]
    public partial class WebChannelMapper
    {
        public partial WebChannelView ToView(WebChannel model);
        public partial List<WebChannelView> ToViewList(List<WebChannel> list);
        public partial WebChannel ToModel(WebChannelView model);
    }
}