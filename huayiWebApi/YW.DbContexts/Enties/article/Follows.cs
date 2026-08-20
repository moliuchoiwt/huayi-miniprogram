using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///关注记录
    // </summary>	

    [SugarTable("Follows")]
    public partial class Follows
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>

        public int UserId { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>

        public string UserName { get; set; }

        /// <summary>
        /// 被关注用户Id
        /// </summary>

        public int ToUserId { get; set; }

        /// <summary>
        /// 被关注用户名称
        /// </summary>

        public string ToUserName { get; set; }

        /// <summary>
        /// 状态 0-取消关注  1-已关注 99-删除
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
    ///关注记录
    // </summary>	

    public partial class FollowsView : Follows
    {

        /// <summary>
        /// 用户头像
        /// </summary>
        public string UserAvatar { get; set; }

        /// <summary>
        /// 被关注用户头像
        /// </summary>
        public string ToUserAvatar { get; set; }

        /// <summary>
        /// 是否关注 1-是
        /// </summary>
        public int IsFollow { get; set; }


    }

    [Mapper]
    public partial class FollowsMapper
    {
        public partial FollowsView ToView(Follows model);
        public partial List<FollowsView> ToViewList(List<Follows> list);
        public partial Follows ToModel(FollowsView model);
    }
}

