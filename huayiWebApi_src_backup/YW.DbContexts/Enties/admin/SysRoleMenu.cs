using Riok.Mapperly.Abstractions;
using SqlSugar;
using System.Collections.Generic;

namespace YW.DbContexts
{
    /// <summary>
    /// 系统角色菜单权限
    ///</summary>
    [SugarTable("sysRoleMenu")]
    public class SysRoleMenu
    {


        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; } = 0;


        /// <summary>
        /// 备  注:菜单id
        /// 默认值:
        ///</summary>

        public int menuId { get; set; } = 0;


        /// <summary>
        /// 备  注:角色id
        /// 默认值:
        ///</summary>

        public int roleId { get; set; } = 0;


    }

    /// <summary>
    /// 系统角色菜单权限
    // </summary>	

    public partial class SysRoleMenuView : SysRoleMenu
    {


    }

    [Mapper]
    public partial class SysRoleMenuMapper
    {
        public partial SysRoleMenuView ToView(SysRoleMenu model);
        public partial List<SysRoleMenuView> ToViewList(List<SysRoleMenu> list);
        public partial SysRoleMenu ToModel(SysRoleMenuView model);
    }


}