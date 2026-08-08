using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///短信表
    // </summary>	

    [SugarTable("Sms")]
    public partial class Sms
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 类型 0-注册 具体看枚举SmsEnum
        /// </summary>

        public int SmsType { get; set; }

        /// <summary>
        /// Title
        /// </summary>

        public string Title { get; set; }

        /// <summary>
        /// Mobile
        /// </summary>

        public string Mobile { get; set; }

        /// <summary>
        /// Code
        /// </summary>

        public string Code { get; set; }

        /// <summary>
        /// Content
        /// </summary>

        public string Content { get; set; }

        /// <summary>
        /// Fail
        /// </summary>

        public string Fail { get; set; }

        /// <summary>
        /// 状态 0-可用 1-已使用
        /// </summary>

        public int State { get; set; }

        /// <summary>
        /// Ip
        /// </summary>

        public string Ip { get; set; }

        /// <summary>
        /// CreateTime
        /// </summary>

        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// ExpireUtc
        /// </summary>

        public DateTime ExpireUtc { get; set; } = DateTime.Now;

    }

    /// <summary>
    ///短信表
    // </summary>	

    public partial class SmsView : Sms
    {


    }

    [Mapper]
    public partial class SmsMapper
    {
        public partial SmsView ToView(Sms model);
        public partial List<SmsView> ToViewList(List<Sms> list);
        public partial Sms ToModel(SmsView model);
    }
}

