using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///评论记录
    // </summary>	

    [SugarTable("Comment")]
    public partial class Comment
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 类型 0-商品 1-店铺
        /// </summary>

        public int cType { get; set; }

        /// <summary>
        /// 上级评论Id
        /// </summary>

        public int parentId { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>

        public int userId { get; set; }


        /// <summary>
        /// 评论对象Id
        /// </summary>

        public int comId { get; set; }

        /// <summary>
        /// 评论名称
        /// </summary>

        public string name { get; set; }

        /// <summary>
        /// 评分
        /// </summary>

        public decimal score { get; set; }

        /// <summary>
        /// 文件编号
        /// </summary>

        public string url { get; set; }

        /// <summary>
        /// 评论内容
        /// </summary>

        public string intro { get; set; }

        /// <summary>
        /// 状态 0-展示 1-隐藏 99-删除
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
        /// <summary>
        /// 关联订单号
        /// </summary>
        public string orderNo { get; set; }
        /// <summary>
        /// 关联店铺
        /// </summary>
        //public int shopId { get; set; }
    }
    public class CommentView : Comment
    {

        /// <summary>
        /// 用户名称
        /// </summary>

        public string userName { get; set; } = string.Empty;
        /// <summary>
        /// 图片列表
        /// </summary>
        public List<string> imgList { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string avatar { get; set; }

        /// <summary>
        /// 子集集合
        /// </summary>
        public List<CommentView> children { get; set; }
    }
    public class CommentQuery
    {
        public List<CommentView> commentList { get; set; }
    }

    [Mapper]
    public partial class CommentMapper
    {
        public partial CommentView ToView(Comment model);
        public partial List<CommentView> ToViewList(List<Comment> list);
        public partial Comment ToModel(CommentView model);
        public partial List<Comment> ToModelList(List<CommentView> list);

    }
}

