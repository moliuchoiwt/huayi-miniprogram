using Riok.Mapperly.Abstractions;

namespace YW.DbContexts
{
    /// <summary>
    /// 系统角色
    ///</summary>
    [SugarTable("sysRole")]
    public class SysRole
    {


        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; } = 0;


        /// <summary>
        /// 备  注:角色名称
        /// 默认值:
        ///</summary>

        public string roleName { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:简介
        /// 默认值:
        ///</summary>       

        public string remark { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:是否删除
        /// 默认值:
        ///</summary>

        public bool delFlag { get; set; }


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
        /// 备  注:是否锁定
        /// 默认值:
        ///</summary>

        public bool isLock { get; set; } = false;



    }

    /// <summary>
    /// 系统角色
    // </summary>	

    public partial class SysRoleView : SysRole
    {
        /// <summary>
        /// 菜单id
        /// </summary>
        [MapperIgnore]
        public List<int> menuIds { get; set; }

    }

    [Mapper]
    public partial class SysRoleMapper
    {
        public partial SysRoleView ToView(SysRole model);
        public partial List<SysRoleView> ToViewList(List<SysRole> list);
        public partial SysRole ToModel(SysRoleView model);
    }
}