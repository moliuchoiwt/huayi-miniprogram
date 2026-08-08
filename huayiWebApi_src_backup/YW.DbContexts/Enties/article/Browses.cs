using Riok.Mapperly.Abstractions;

namespace YW.DbContexts
{

    /// <summary>
    ///浏览记录
    // </summary>	

    [SugarTable("Browses")]
    public partial class Browses
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 浏览类型 0-商品 1-文章
        /// </summary>

        public int BrowseType { get; set; } = 0;

        /// <summary>
        /// 浏览对象Id
        /// </summary>

        public int BrowsesId { get; set; }

        /// <summary>
        /// 浏览对象标题
        /// </summary>

        public string BrowsesTitle { get; set; }

        /// <summary>
        /// 浏览对象图片
        /// </summary>
        public string BrowsesImage { get; set; } = string.Empty;

        /// <summary>
        /// 浏览对象价格
        /// </summary>
        public decimal BrowsesPrice { get; set; } = 0M;

        /// <summary>
        /// 浏览时长(s)
        /// </summary>

        public decimal ReadTimes { get; set; }

        /// <summary>
        /// 开始浏览时间
        /// </summary>

        public DateTime BrowsesTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 结束浏览时间
        /// </summary>

        public DateTime BrowsesEndTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 用户Id
        /// </summary>

        public int UserId { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>

        public string UserName { get; set; }

        /// <summary>
        /// 分享用户Id
        /// </summary>

        public int ShareUserId { get; set; }

        /// <summary>
        /// 分享用户名称
        /// </summary>

        public string ShareName { get; set; }

        /// <summary>
        /// 状态 0-展示 1-隐藏 99-删除
        /// </summary>

        public int State { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime CreateTime { get; set; } = DateTime.Now;

    }

    /// <summary>
    ///浏览记录
    // </summary>	

    public partial class BrowsesView : Browses
    {
        /// <summary>
        /// 用户头像
        /// </summary>
        public string UserAvatar { get; set; }

        /// <summary>
        /// 分享用户头像
        /// </summary>
        public string ShareAvatar { get; set; }
    }



    [Mapper]
    public partial class BrowsesMapper
    {
        public partial BrowsesView ToView(Browses model);
        public partial List<BrowsesView> ToViewList(List<Browses> list);
        public partial Browses ToModel(BrowsesView model);
    }


}

