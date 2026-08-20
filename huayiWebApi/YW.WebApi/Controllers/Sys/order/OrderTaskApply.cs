using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysOrderTaskApplyController
    /// </summary>
    public class SysOrderTaskApplyController : BaseController
    {
        private readonly OrderTaskApplyMapper _mapper = new();
        private readonly IOrderTaskApplyService _orderTaskApplyService;
        public SysOrderTaskApplyController(IClaimsAccessor claimsAccessor, OrderTaskApplyService orderTaskApplyService)
        {
            _claimsAccessor = claimsAccessor;
            _orderTaskApplyService = orderTaskApplyService;
        }

        #region 操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<OrderTaskApply>();






            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                exWhere.And(a => queryModel.queryName.Contains(a.Id.ToString()));
            }
            if (queryModel.startTime.HasValue) { exWhere.And(a => a.createTime >= queryModel.startTime.Value); }
            if (queryModel.endTime.HasValue) { exWhere.And(a => a.createTime <= queryModel.endTime.Value); }

            var data = await _orderTaskApplyService.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
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
        public async Task<ResultModel> Operation(OrderTaskApplyView model)
        {
            var res = new ResultModel();
            var info = _mapper.ToModel(model);
            bool isok = false;
            if (info.Id > 0)
            {

                isok = await _orderTaskApplyService.UpdateAsync(info);
            }
            else
            {

                info.createTime = DateTime.Now;

                isok = await _orderTaskApplyService.InsertAsync(info);
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
            if (del.ids == null || del.ids.Length <= 0)
            {
                res.msg = "请选择删除数据";
                return res;
            }

            var isok = await _orderTaskApplyService.UpdateAsync(it => new OrderTaskApply { status = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));

            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");
            return res;
        }
        #endregion
    }
}