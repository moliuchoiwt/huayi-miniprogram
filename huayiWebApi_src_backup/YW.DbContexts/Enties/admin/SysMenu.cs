using Riok.Mapperly.Abstractions;

namespace YW.DbContexts
{
    /// <summary>
    /// 后台菜单表
    ///</summary>
    [SugarTable("sysMenu")]
    public class SysMenu
    {


        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]

        public int Id { get; set; } = 0;


        /// <summary>
        /// 备  注:父级id
        /// 默认值:
        ///</summary>
        public int pid { get; set; } = 0;


        /// <summary>
        /// 备  注:路径
        /// 默认值:
        ///</summary>
        public string path { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:路由名称
        /// 默认值:
        ///</summary>
        public string name { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:标题
        /// 默认值:
        ///</summary>
        public string title { get; set; } = string.Empty;


        /// <summary>
        /// 备  注: 图标
        /// 默认值:
        ///</summary>
        public string icon { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        public string component { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        public string redirect { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:排序
        /// 默认值:
        ///</summary>        
        public int sort { get; set; } = 0;



        /// <summary>
        /// 备  注:菜单类型 0.目录 1.菜单 2.按钮
        /// 默认值:
        ///</summary>
        public int menuType { get; set; } = 0;



        /// <summary>
        /// 是否在菜单中隐藏, 需要高亮的 path (通常用作详情页高亮父级菜单)
        /// </summary>        
        public string activeMenu { get; set; } = string.Empty;

        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>        
        public bool isHide { get; set; }


        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>        

        public string isLink { get; set; } = string.Empty;


        /// <summary>
        /// 备  注:菜单是否全屏
        /// 默认值:
        ///</summary>       
        public bool isFull { get; set; }


        /// <summary>
        /// 备  注:菜单是否固定在标签页
        /// 默认值:
        ///</summary>     
        public bool isAffix { get; set; }


        /// <summary>
        /// 备  注:当前路由是否缓存
        /// 默认值:
        ///</summary>      
        public bool isKeepAlive { get; set; }


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
        /// 备  注:是否删除
        /// 默认值:
        ///</summary>
        public bool delFlag { get; set; }


        /// <summary>
        /// 备  注:删除人ID
        /// 默认值:
        ///</summary>
        public int deleteId { get; set; } = 0;


        /// <summary>
        /// 备  注:
        /// 默认值:
        ///</summary>
        public DateTime deleteTime { get; set; } = DateTime.Now;


    }

    /// <summary>
    /// 后台菜单表
    // </summary>	

    public partial class SysMenuView : SysMenu
    {
        [MapperIgnore]
        public List<SysMenuView> children { get; set; }

    }

    public class SysUserMenu
    {
        [SqlSugar.SugarColumn(IsPrimaryKey = true)]
        public int Id { get; set; } = 0;
        public int pid { get; set; } = 0;
        public string path { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string redirect { get; set; } = string.Empty;
        public string component { get; set; } = string.Empty;
        public SysUserMenuMeta meta { get; set; }
        [SqlSugar.SugarColumn(IsIgnore = true)]
        public List<SysUserMenu> children { get; set; }

        public int sort { get; set; } = 0;

        public int menuType { get; set; } = 0;
    }

    public class SysUserMenuMeta
    {
        public string icon { get; set; } = string.Empty;

        public string title { get; set; } = string.Empty;

        public string activeMenu { get; set; } = string.Empty;

        public string isLink { get; set; } = string.Empty;

        public bool isHide { get; set; } = false;

        public bool isFull { get; set; } = false;

        public bool isAffix { get; set; } = false;

        public bool isKeepAlive { get; set; } = false;

    }


    /// <summary>
    /// 系统菜单类型
    /// </summary>
    public enum sysMenuTypeEnum
    {
        目录 = 0,
        菜单 = 1,
        按钮 = 2
    }


    [Mapper]
    public partial class SysMenuMapper
    {
        public partial SysMenuView ToView(SysMenu model);
        public partial List<SysMenuView> ToViewList(List<SysMenu> list);
        public partial SysMenu ToModel(SysMenuView model);
    }

}