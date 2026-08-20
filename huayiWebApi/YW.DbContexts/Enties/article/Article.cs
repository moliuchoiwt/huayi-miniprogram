using Riok.Mapperly.Abstractions;

namespace YW.DbContexts
{

    /// <summary>
    ///文章
    // </summary>	

    [SugarTable("Article")]
    public partial class Article
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// ArticleType 0-合作商家
        /// </summary>

        public int articleType { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public decimal longitude { get; set; } = 0M;

        /// <summary>
        /// 纬度
        /// </summary>
        public decimal latitude { get; set; } = 0M;
        /// <summary>
        /// 发布地址
        /// </summary>
        public string address { get; set; }

        /// <summary>
        /// 标题
        /// </summary>

        public string title { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>

        public string coverUrl { get; set; }

        /// <summary>
        /// 摘要
        /// </summary>

        public string intro { get; set; }

        /// <summary>
        /// 详情
        /// </summary>

        public string contents { get; set; }

        /// <summary>
        /// 视频链接
        /// </summary>

        public string url { get; set; }

        /// <summary>
        /// 时长(s)
        /// </summary>

        public decimal times { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>

        public int userId { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>

        public string userName { get; set; }

        /// <summary>
        /// 浏览数
        /// </summary>

        public int browseNum { get; set; }

        /// <summary>
        /// 点赞数
        /// </summary>

        public int likeNum { get; set; }

        /// <summary>
        /// 收藏数
        /// </summary>

        public int collectionNum { get; set; }

        /// <summary>
        /// 留言数
        /// </summary>

        public int msgNum { get; set; }
        /// <summary>
        /// 排序
        /// </summary>
        public int sort { get; set; } = 0;

        /// <summary>
        /// 状态 0.显示   1.待审核   2-拒绝 99-删除
        /// </summary>

        public int status { get; set; } = 0;

        /// <summary>
        /// 审核信息
        /// </summary>

        public string auditIntro { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime createTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>

        public DateTime updateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 联系方式
        /// </summary>
        public string contactInfo { get; set; } = string.Empty;
    }

    /// <summary>
    ///文章
    // </summary>	

    public partial class ArticleView : Article
    {
        /// <summary>
        /// 是否点赞文章
        /// </summary>
        [MapperIgnore]
        public bool isLike { get; set; } = false;
        /// <summary>
        /// 是否关注用户
        /// </summary>
        [MapperIgnore]
        public bool isFollow { get; set; } = false;
    }


    [Mapper]
    public partial class ArticleMapper
    {
        public partial ArticleView ToView(Article model);
        public partial List<ArticleView> ToViewList(List<Article> list);
        public partial Article ToModel(ArticleView model);
    }

}

