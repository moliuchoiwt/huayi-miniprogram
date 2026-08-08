using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///轮播图表
    // </summary>	

    [SugarTable("Banner")]
    public partial class Banner
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 轮播类型 0-首页 具体分类看枚举BannerEnum
        /// </summary>

        public int bType { get; set; } = 0;

        /// <summary>
        /// Title
        /// </summary>

        public string title { get; set; } = string.Empty;

        /// <summary>
        /// Intro
        /// </summary>

        public string intro { get; set; } = string.Empty;

        /// <summary>
        /// ImgUrl
        /// </summary>

        public string imgUrl { get; set; } = string.Empty;

        /// <summary>
        /// Link
        /// </summary>

        public string link { get; set; } = string.Empty;

        /// <summary>
        /// Sort
        /// </summary>

        public int sort { get; set; } = 0;

        /// <summary>
        /// State
        /// </summary>

        public int status { get; set; } = 0;

        /// <summary>
        /// CreateTime
        /// </summary>

        public DateTime createTime { get; set; } = DateTime.Now;

        /// <summary>
        /// UpdateTime
        /// </summary>

        public DateTime updateTime { get; set; } = DateTime.Now;

    }


    /// <summary>
    ///轮播图表
    // </summary>	

    public partial class BannerView : Banner
    {


    }

    [Mapper]
    public partial class BannerMapper
    {
        public partial BannerView ToView(Banner model);
        public partial List<BannerView> ToViewList(List<Banner> list);
        public partial Banner ToModel(BannerView model);
    }
}

