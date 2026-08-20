using System;
using SqlSugar;

namespace YW.DbContexts
{
	        
	/// <summary>
 	///后台菜单表
	// </summary>	
	 
    [SugarTable("NavMenu")]
	public partial class NavMenu
	{
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// 类型 0-总平台 
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Name
        /// </summary>

        public string Name { get; set; }

        /// <summary>
        /// Code
        /// </summary>

        public string Code { get; set; }

        /// <summary>
        /// Icon
        /// </summary>

        public string Icon { get; set; }

        /// <summary>
        /// PathUrl
        /// </summary>

        public string PathUrl { get; set; }

        /// <summary>
        /// ParentId
        /// </summary>

        public int ParentId { get; set; }

        /// <summary>
        /// RankNum
        /// </summary>

        public int RankNum { get; set; }

        /// <summary>
        /// 状态 0-显示 1-隐藏 99-删除
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

        /// <summary>
        /// 是否固定在 tagsView 栏上
        /// </summary>
        public bool isAffix { get; set; } = false;

    }
}

