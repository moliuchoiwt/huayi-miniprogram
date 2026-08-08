using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///用户反馈
    // </summary>	

    [SugarTable("Feedback")]
    public partial class Feedback
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 用户Id
        /// </summary>

        public int userId { get; set; }



        /// <summary>
        /// 联系方式
        /// </summary>

        public string contact { get; set; }

        /// <summary>
        /// 标题
        /// </summary>

        public string title { get; set; }

        /// <summary>
        /// 内容
        /// </summary>

        public string contents { get; set; }

        /// <summary>
        /// 图片
        /// </summary>

        public string imgUrl { get; set; }

        /// <summary>
        /// 状态 0-展示 99-删除
        /// </summary>

        public int status { get; set; }

        /// <summary>
        /// CreateTime
        /// </summary>

        public DateTime createTime { get; set; } = DateTime.Now;

    }
    /// <summary>
    ///用户反馈
    // </summary>	

    public partial class FeedbackView : Feedback
    {
        /// <summary>
        /// UserName
        /// </summary>

        public string userName { get; set; }
    }

    [Mapper]
    public partial class FeedbackMapper
    {
        public partial FeedbackView ToView(Feedback model);
        public partial List<FeedbackView> ToViewList(List<Feedback> list);
        public partial Feedback ToModel(FeedbackView model);
    }
}

