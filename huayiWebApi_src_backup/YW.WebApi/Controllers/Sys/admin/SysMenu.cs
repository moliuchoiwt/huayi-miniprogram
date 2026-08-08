using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysSysMenuController
    /// </summary>
    public class SysMenuController : BaseController
    {

        private readonly ISysMenuService _sysMenuService;
        private readonly ISysRoleMenuService _roleMenuService;

        private readonly SysMenuMapper mapper = new();
        public SysMenuController(SysMenuService sysMenuService, SysRoleMenuService roleMenuService)
        {
            _sysMenuService = sysMenuService;
            _roleMenuService = roleMenuService;
        }

        #region navMenu操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<SysMenu>();
            exWhere.And(a => !a.delFlag && a.pid == 0);
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                exWhere.And(a => a.name.Contains(queryModel.queryName));
            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= queryModel.endTime.Value);
            }
            var data = await _sysMenuService.GetPageListAsync(exWhere, p, it => new { it.sort, it.createTime }, OrderByType.Desc);

            if (data != null && data.Count > 0)
            {
                var nList = mapper.ToViewList(data);
                var plist = new List<SysMenuView>();

                //var where = PredicateBuilder.New<SysMenu>(a => !a.DelFlag && a.Pid > 0);

                //var list = await _navMenuService.GetListAsync(where);
                //if (list != null && list.Count > 0)
                //{
                //    list = list.OrderByDescending(a => a.Sort).ToList();
                //    plist = _mapper.Map<List<SysMenu>, List<SysMenuView>>(list);
                //    nList.ForEach(item => item.children = plist.Where(a => a.ParentId.Equals(item.Id)).ToList());
                //}
                res.data = new { total = p.TotalCount, items = nList };
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            return res;
        }


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(SysMenu model)
        {
            var res = new ResultModel();

            bool isok = false;
            if (model.Id > 0)
            {
                model.updateTime = DateTime.Now;
                model.updateId = adminId;
                isok = await _sysMenuService.UpdateAsync(model);
            }
            else
            {
                model.createTime = DateTime.Now;
                model.createId = adminId;
                model.updateTime = DateTime.Now;
                model.updateId = adminId;
                isok = await _sysMenuService.InsertAsync(model);
            }
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");

            return res;
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Delete(DelModel del)
        {
            var res = new ResultModel();

            if (del.ids != null && del.ids.Length > 0)
            {
                var isok = await _sysMenuService.UpdateAsync(it => new SysMenu { delFlag = true, deleteId = adminId, deleteTime = DateTime.Now }, it => SqlFunc.ContainsArray(del.ids, it.Id));

                res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "删除" + (isok ? "成功" : "失败");
            }

            return res;

        }
        #endregion


        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> TreeList(QueryModel queryModel) => await _sysMenuService.TreeList(queryModel);

        /// <summary>
        /// 菜单列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]        
        public async Task<ResultModel> UserMenuList()
        {
            var view = new QueryModel();
            var res = new ResultModel();
            var user = await adminService.GetByIdAsync(adminId);
            if (user == null)
            {
                res.msg = "系统错误 请刷新页面";
                return res;
            }
            var roleMenuList = await _roleMenuService.GetListAsync(it => it.roleId == user.roleId);
            view.Ids = roleMenuList.Select(it => it.menuId).ToList();
            view.queryTypeArr = new List<int> { (int)sysMenuTypeEnum.目录, (int)sysMenuTypeEnum.菜单 };
            res = await _sysMenuService.TreeList(view);
            return res;
        }

        /// <summary>
        /// 菜单权限
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> AuthButtons()
        {
            var res = new ResultModel();
            var user = await adminService.GetByIdAsync(adminId);
            if (user == null)
            {
                res.msg = "系统错误 请刷新页面";
                return res;
            }
            var roleMenuList = await _roleMenuService.GetListAsync(it => it.roleId == user.roleId);
            var menuIds = roleMenuList.Select(it => it.menuId).ToList();
            var menuTypeArr = new List<int> { (int)sysMenuTypeEnum.按钮, (int)sysMenuTypeEnum.菜单 };
            var allMenuList = await _sysMenuService.GetListAsync(it => SqlFunc.ContainsArray(menuIds, it.Id) && SqlFunc.ContainsArray(menuTypeArr, it.menuType));
            var menuList = allMenuList.FindAll(it => it.menuType == (int)sysMenuTypeEnum.菜单);
            var buttonList = allMenuList.FindAll(it => it.menuType == (int)sysMenuTypeEnum.按钮);
            var list = new Dictionary<string, List<string>>();
            if (menuList != null && menuList.Count > 0)
            {
                foreach (var menu in menuList)
                {
                    var permissionsList = buttonList.Where(it => it.pid == menu.Id).Select(it => it.name).ToList();
                    list.Add(menu.name, permissionsList);
                }
            }
            res.data = list;
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            return res;
        }
    }
}