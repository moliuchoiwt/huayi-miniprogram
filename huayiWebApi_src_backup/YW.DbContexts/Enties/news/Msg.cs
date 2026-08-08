using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///消息表
    // </summary>	

    [SugarTable("Msg")]
    public partial class Msg
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>

        public int msgType { get; set; }

        /// <summary>
        /// 标题
        /// </summary>

        public string title { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>

        public string url { get; set; }

        /// <summary>
        /// 摘要
        /// </summary>

        public string intro { get; set; }

        /// <summary>
        /// 内容详情
        /// </summary>

        public string contents { get; set; }

        /// <summary>
        /// 跳转链接
        /// </summary>

        public string tagUrl { get; set; }

        /// <summary>
        /// 状态 0-正常 99-删除
        /// </summary>

        public int status { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>

        public DateTime createTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 发布时间
        /// </summary>

        public DateTime updateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 推送对象类型 0-用户 1-店铺 9-所有
        /// </summary>

        public int sendType { get; set; }

        /// <summary>
        /// 推送对象Id
        /// </summary>

        public int userId { get; set; }

        /// <summary>
        /// 阅读状态 0-未读 1-已读
        /// </summary>

        public int readState { get; set; }

        /// <summary>
        /// 阅读时间
        /// </summary>

        public DateTime readTime { get; set; } = DateTime.Now;

    }

    /// <summary>
    ///消息表
    // </summary>	

    public partial class MsgView : Msg
    {

    }
    [Mapper]
    public partial class MsgMapper
    {
        public partial MsgView ToView(Msg model);
        public partial List<MsgView> ToViewList(List<Msg> list);
        public partial Msg ToModel(MsgView model);
    }

}

