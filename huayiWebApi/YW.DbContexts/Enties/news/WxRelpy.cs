using SqlSugar;
using System;


namespace YW.DbContexts
{
    /// <summary>
    ///微信回复
    // </summary>	

    [SugarTable("WxReply")]
    public partial class WxReply
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 状态 99-删除
        /// </summary>

        public int States { get; set; }

        /// <summary>
        /// CrateTime
        /// </summary>

        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// UpdateTime
        /// </summary>

        public DateTime UpdateTime { get; set; } = DateTime.Now;

    }



}
