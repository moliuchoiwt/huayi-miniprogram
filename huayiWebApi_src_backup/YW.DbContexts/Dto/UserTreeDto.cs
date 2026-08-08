using System;
using System.Collections.Generic;

namespace YW.DbContexts.Dto
{
    public class UserTreeDto
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int userId { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string userName { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string avatar { get; set; }
        /// <summary>
        ///
        /// </summary>
        public int parentId { get; set; }
        /// <summary>
        /// 直属人数
        /// </summary>
        public int childenNum { get; set; }

        /// <summary>
        /// 直属列表
        /// </summary>
        public List<UserTreeDto> childenList { get; set; }
        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime createTime { get; set; }

    }
}
