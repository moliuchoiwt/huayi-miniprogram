using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    /// 网站导航
    // </summary>	

    [SugarTable("WebNavMenu")]
    public partial class WebNavMenu
    {
        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; } = 0;
        /// <summary>
        ///  展示图
        /// </summary>
        public string ImgUrl { get; set; } = string.Empty;

        /// <summary>
        /// 激活后的图片
        /// </summary>
        public string ActivateImgUrl { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string title { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string linkUrl { get; set; } = string.Empty;
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
    /// 网站导航
    // </summary>	

    public partial class WebNavMenuView : WebNavMenu
    {

    }

    [Mapper]
    public partial class WebNavMenuMapper
    {
        public partial WebNavMenuView ToView(WebNavMenu model);
        public partial List<WebNavMenuView> ToViewList(List<WebNavMenu> list);
        public partial WebNavMenu ToModel(WebNavMenuView model);
    }
}