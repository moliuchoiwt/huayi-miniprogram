using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysWebNavMenuController
    /// </summary>
    public class SysWebNavMenuController : BaseController
    {

        private readonly IWebNavMenuService _webNavMenuService;

        private readonly WebNavMenuMapper mapper = new();
        public SysWebNavMenuController(IClaimsAccessor claimsAccessor, WebNavMenuService webNavMenuService)
        {
            _claimsAccessor = claimsAccessor;
            _webNavMenuService = webNavMenuService;
        }

        #region webNavMenu操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<WebNavMenu>();
            exWhere.And(a => a.States < 99);
            if (queryModel.queryState.HasValue) { exWhere.And(a => a.States == queryModel.queryState.Value); }
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                exWhere.And(a => a.title.Contains(queryModel.queryName));
            }
            if (queryModel.startTime.HasValue) { exWhere.And(a => a.CreateTime >= queryModel.startTime.Value); }
            if (queryModel.endTime.HasValue) { exWhere.And(a => a.CreateTime <= queryModel.endTime.Value); }

            var data = await _webNavMenuService.GetPageListAsync(exWhere, p, it => new { it.CreateTime, it.Id }, OrderByType.Desc);
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = data };
            return res;
        }


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(WebNavMenuView model)
        {
            var res = new ResultModel();
            var info = mapper.ToModel(model);
            bool isok = false;
            if (info.Id > 0)
            {
                info.UpdateTime = DateTime.Now;

                isok = await _webNavMenuService.UpdateAsync(info);
            }
            else
            {
                info.CreateTime = DateTime.Now;
                info.UpdateTime = DateTime.Now;
                isok = await _webNavMenuService.InsertAsync(info);
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
        public async Task<ResultModel> DelWebNavMenu(DelModel del)
        {
            var res = new ResultModel();
            if (del.ids == null || del.ids.Length <= 0)
            {
                res.msg = "请选择删除数据";
                return res;
            }
            var isok = await _webNavMenuService.UpdateAsync(it => new WebNavMenu { States = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");
            return res;
        }
        #endregion
    }
}