using System;
using SqlSugar;

namespace YW.DbContexts
{
	        
	/// <summary>
 	///角色信息表
	// </summary>	
	 
    [SugarTable("RoleInfo")]
	public partial class RoleInfo
	{
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }
        /// <summary>
        /// 类型 0-总平台 1-商户平台 2-供应商
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 角色名称
        /// </summary>

        public string Name { get; set; }

        /// <summary>
        /// 角色权限:用逗号隔开 0-查看 1-新增 2-编辑 3-删除
        /// </summary>

        public string RuleType { get; set; }

        /// <summary>
        /// Remark
        /// </summary>

        public string Remark { get; set; }

        /// <summary>
        /// 状态 0-正常 99-删除
        /// </summary>

        public int State { get; set; }

        /// <summary>
        /// CreateTime
        /// </summary>

        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// UpdateTime
        /// </summary>

        public DateTime UpdateTime { get; set; } = DateTime.Now;

    }
}

