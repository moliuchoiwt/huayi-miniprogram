using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysExpressController
    /// </summary>
    public class SysExpressController : BaseController
    {

        private readonly IExpressService _expressService;

        private readonly ExpressMapper mapper = new();
        public SysExpressController(IClaimsAccessor claimsAccessor, ExpressService expressService)
        {
            _claimsAccessor = claimsAccessor;
            _expressService = expressService;
        }

        #region express操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<Express>();
            exWhere.And(a => a.status != 99);
            if (queryModel.queryState.HasValue)
            {
                exWhere.And(a => a.status == queryModel.queryState.Value);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                if (int.TryParse(queryModel.queryName, out tId)) exWhere.And(a => a.Id == tId);
                else exWhere.And(a => a.name.Contains(queryModel.queryName) || a.code.Contains(queryModel.queryName) || a.url.Contains(queryModel.queryName) || a.intro.Contains(queryModel.queryName));

            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= queryModel.endTime.Value);
            }

            var data = await _expressService.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
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
        public async Task<ResultModel> Operation(ExpressView model)
        {
            var res = new ResultModel();
            var info = mapper.ToModel(model);
            bool isok = false;
            if (info.Id > 0)
            {
                info.updateTime = DateTime.Now;
                isok = await _expressService.UpdateAsync(info);
            }
            else
            {
                info.createTime = DateTime.Now;
                info.updateTime = DateTime.Now;
                isok = await _expressService.InsertAsync(info);
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
        public async Task<ResultModel> DelExpress(DelModel del)
        {
            var res = new ResultModel();

            if (del.ids != null && del.ids.Length > 0)
            {
                var isok = await _expressService.UpdateAsync(it => new Express { status = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));

                res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "删除" + (isok ? "成功" : "失败");
            }

            return res;

        }
        #endregion
    }
}