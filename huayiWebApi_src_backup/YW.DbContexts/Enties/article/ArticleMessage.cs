using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///文章留言
    // </summary>	

    [SugarTable("ArticleMessage")]
    public partial class ArticleMessage
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 上级Id
        /// </summary>

        public int ParentId { get; set; }

        /// <summary>
        /// 文章Id
        /// </summary>

        public int ArticleId { get; set; }

        /// <summary>
        /// 文章标题
        /// </summary>

        public string ArticleTitle { get; set; }

        /// <summary>
        /// 被留言用户Id
        /// </summary>

        public int ArticleUserId { get; set; }

        /// <summary>
        /// 文件编号
        /// </summary>

        public string Url { get; set; }

        /// <summary>
        /// 留言内容
        /// </summary>

        public string Intro { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>

        public int UserId { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>

        public string UserName { get; set; }

        /// <summary>
        /// 阅读状态 0-未读 1-已读
        /// </summary>

        public int ReadState { get; set; }

        /// <summary>
        /// 状态 0-展示 1-隐藏 99-删除
        /// </summary>

        public int State { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>

        public DateTime UpdateTime { get; set; } = DateTime.Now;

    }

    /// <summary>
    ///文章留言
    // </summary>	

    public partial class ArticleMessageView : ArticleMessage
    {

    }

    [Mapper]
    public partial class ArticleMessageMapper
    {
        public partial ArticleMessageView ToView(ArticleMessage model);
        public partial List<ArticleMessageView> ToViewList(List<ArticleMessage> list);
        public partial ArticleMessage ToModel(ArticleMessageView model);
    }
}

