using Riok.Mapperly.Abstractions;
using SqlSugar;
using System;
using System.Collections.Generic;

namespace YW.DbContexts
{
    /// <summary>
    /// 系统用户表
    ///</summary>
    [SugarTable("sysUser")]
    public class SysUser
    {


        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; } = 0;


        /// <summary>
        /// 权限id
        /// </summary>
        public int roleId { get; set; } = 0;

        /// <summary>
        /// 备  注:用户名
        /// 默认值:
        ///</summary>        

        public string userName { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:密码
        /// 默认值:
        ///</summary>

        public string pwd { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:手机号
        /// 默认值:
        ///</summary>

        public string phone { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:昵称
        /// 默认值:
        ///</summary>        

        public string nickName { get; set; } = string.Empty;

        /// <summary>
        /// 头像
        /// </summary>
        public string avatar { get; set; } = string.Empty;

        /// <summary>
        /// 备  注:账户状态
        /// 默认值:
        ///</summary>

        public int accountStatus { get; set; } = 0;


        /// <summary>
        /// 备  注:最近一次登录时间
        /// 默认值:
        ///</summary>

        public DateTime lastLoginTime { get; set; } = DateTime.Now;


        /// <summary>
        /// 备  注:创建时间
        /// 默认值:
        ///</summary>

        public DateTime createTime { get; set; } = DateTime.Now;


        /// <summary>
        /// 备  注:更新时间
        /// 默认值:
        ///</summary>        

        public DateTime updateTime { get; set; } = DateTime.Now;


        /// <summary>
        /// 备  注:是否删除
        /// 默认值:
        ///</summary>        

        public bool delFlag { get; set; }


        /// <summary>
        /// 备  注:创建人ID
        /// 默认值:
        ///</summary>

        public int createId { get; set; } = 0;


        /// <summary>
        /// 备  注:更新人ID
        /// 默认值:
        ///</summary>

        public int updateId { get; set; } = 0;

        /// <summary>
        /// ip
        /// </summary>
        public string Ip { get; set; } = string.Empty;

        /// <summary>
        /// 关联分类id
        /// </summary>
        public string classIds { get; set; } = string.Empty;

    }

    /// <summary>
    /// 系统用户表
    // </summary>	

    public partial class SysUserView : SysUser
    {


    }

    [Mapper]
    public partial class SysUserMapper
    {
        public partial SysUserView ToView(SysUser model);
        public partial List<SysUserView> ToViewList(List<SysUser> list);
        public partial SysUser ToModel(SysUserView model);
    }

}