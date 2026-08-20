using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///点赞记录
    // </summary>	

    [SugarTable("Likes")]
    public partial class Likes
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 类型 0-文章 1-留言
        /// </summary>

        public int LikesType { get; set; }

        /// <summary>
        /// 点赞对象Id
        /// </summary>

        public int LikesId { get; set; }

        /// <summary>
        /// 点赞对象标题
        /// </summary>

        public string Title { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>

        public int UserId { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>

        public string UserName { get; set; }

        /// <summary>
        /// 被点赞对象用户Id
        /// </summary>

        public int ToUserId { get; set; }

        /// <summary>
        /// 状态 0-取消点赞  1-已点赞 99-删除
        /// </summary>

        public int State { get; set; }

        /// <summary>
        /// 阅读状态 0-未读  1-已读 99-删除
        /// </summary>

        public int ReadState { get; set; }

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
    ///点赞记录
    // </summary>	

    public partial class LikesView : Likes
    {


    }

    [Mapper]
    public partial class LikesMapper
    {
        public partial LikesView ToView(Likes model);
        public partial List<LikesView> ToViewList(List<Likes> list);
        public partial Likes ToModel(LikesView model);
    }
}

