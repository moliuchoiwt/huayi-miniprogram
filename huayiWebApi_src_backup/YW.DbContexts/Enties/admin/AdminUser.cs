using System;
using SqlSugar;

namespace YW.DbContexts
{
	        
	/// <summary>
 	///后台管理员信息表
	// </summary>	
	 
    [SugarTable("AdminUser")]
	public partial class AdminUser
	{
        /// <summary>
        /// Id
        /// </summary>

        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; }

        /// <summary>
        /// RoleId
        /// </summary>

        public int RoleId { get; set; }

        /// <summary>
        /// LoginName
        /// </summary>

        public string LoginName { get; set; }

        /// <summary>
        /// Pwd
        /// </summary>

        public string Pwd { get; set; }

        /// <summary>
        /// NickName
        /// </summary>

        public string NickName { get; set; }

        /// <summary>
        /// Avatar
        /// </summary>

        public string Avatar { get; set; }

        /// <summary>
        /// Mobile
        /// </summary>

        public string Mobile { get; set; }

        /// <summary>
        /// Rules
        /// </summary>

        public string Rules { get; set; }

        /// <summary>
        /// Remark
        /// </summary>

        public string Remark { get; set; }

        /// <summary>
        /// RankNum
        /// </summary>

        public int RankNum { get; set; }

        /// <summary>
        /// 状态 0-可用  99-删除
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

        public string Token { get; set; } = string.Empty;
        public DateTime ExpireUtc { get; set; }
        public string Ip { get; set; } = string.Empty;
    }
}

