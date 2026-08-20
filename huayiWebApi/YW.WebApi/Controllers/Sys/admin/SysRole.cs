using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysSysRoleController
    /// </summary>
    public class SysRoleController : BaseController
    {

        private readonly ISysRoleService _roleInfoService;
        private readonly ISysRoleMenuService _roleNavMenuService;

        private readonly SysRoleMapper sysRoleMapper = new();
        public SysRoleController(SysRoleService roleInfoService, SysRoleMenuService roleNavMenuService)
        {
            _roleInfoService = roleInfoService;
            _roleNavMenuService = roleNavMenuService;
        }

        #region roleInfo操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<SysRole>();
            exWhere.And(a => !a.delFlag);
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                int.TryParse(queryModel.queryName, out tId);

                exWhere.And(a => a.Id == tId || a.roleName.Contains(queryModel.queryName) || a.remark.Contains(queryModel.queryName));

            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= queryModel.endTime.Value);
            }

            var data = await _roleInfoService.GetPageListAsync(exWhere, p, it => new { it.Id, it.createTime }, OrderByType.Asc);

            var roleList = new List<SysRoleView>();
            if (data != null && data.Count > 0)
            {
                roleList = sysRoleMapper.ToViewList(data);
                var rids = data.Select(a => a.Id).ToList();
                var where = PredicateBuilder.New<SysRoleMenu>(a => SqlFunc.ContainsArray(rids, a.roleId));

                var nvaList = await _roleNavMenuService.GetListAsync(where);
                if (nvaList != null && nvaList.Count > 0)
                {
                    foreach (var item in roleList)
                    {
                        if (nvaList.Count(a => a.roleId == item.Id) > 0)
                        {
                            item.menuIds = nvaList.Where(a => a.roleId == item.Id).Select(it => it.menuId).ToList();
                        }
                    }
                }
            }
            res.data = new { total = p.TotalCount, items = roleList };

            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            return res;
        }


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(SysRoleView model)
        {
            var res = await _roleInfoService.Operation(model);
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
                if (adminService.Count(it => SqlFunc.ContainsArray(del.ids, it.roleId) && !it.delFlag) > 0)
                {
                    res.msg = "该角色下有账号，无法删除";
                    return res;
                }

                var isok = await _roleInfoService.UpdateAsync(it => new SysRole { delFlag = true }, it => SqlFunc.ContainsArray(del.ids, it.Id) && !it.isLock);
                if (isok) await _roleNavMenuService.DeleteAsync(it => SqlFunc.ContainsArray(del.ids, it.roleId));

                res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "删除" + (isok ? "成功" : "失败");
            }

            return res;

        }
        #endregion
    }
}