using SqlSugar;

namespace YW.DbContexts
{
	        
	/// <summary>
 	///角色菜单权限表
	// </summary>	
	 
    [SugarTable("RoleNavMenu")]
	public partial class RoleNavMenu
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
        /// RoleId
        /// </summary>

        public int RoleId { get; set; }

        /// <summary>
        /// NvaMenuId
        /// </summary>

        public int NvaMenuId { get; set; }

        /// <summary>
        /// RuleType
        /// </summary>

        public string RuleType { get; set; }

    }
}

