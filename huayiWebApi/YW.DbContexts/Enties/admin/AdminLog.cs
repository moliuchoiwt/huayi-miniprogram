using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{

    /// <summary>
    ///后台操作日志表
    // </summary>	

    [SugarTable("AdminLog")]
    public partial class AdminLog
    {
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }
        /// <summary>
        /// 类型0-总平台 1-供应商 2-店铺
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// AdminId
        /// </summary>

        public int adminId { get; set; }

        /// <summary>
        /// Name
        /// </summary>

        public string name { get; set; }

        /// <summary>
        /// 请求链接
        /// </summary>
        public string apiUrl { get; set; } = string.Empty;

        /// <summary>
        /// Content
        /// </summary>

        public string contents { get; set; }

        /// <summary>
        /// ClientIp
        /// </summary>

        public string clientIp { get; set; }

        /// <summary>
        /// CreateTime
        /// </summary>

        public DateTime createTime { get; set; } = DateTime.Now;

    }

    public class AdminLogView : AdminLog
    {

    }

    [Mapper]
    public partial class AdminLogMapper
    {
        public partial AdminLogView ToView(AdminLog model);
        public partial List<AdminLogView> ToViewList(List<AdminLog> list);
        public partial AdminLog ToModel(AdminLogView model);
    }

}

