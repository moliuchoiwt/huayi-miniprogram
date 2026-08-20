using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysWebCategoryController
    /// </summary>
    public class SysWebCategoryController : BaseController
    {

        private readonly IWebCategoryService _webCategoryService;
        private readonly IWebChannelService _webChannelService;
        private readonly IArticleService _articleService;

        private readonly WebCategoryMapper mapper = new();
        public SysWebCategoryController(IClaimsAccessor claimsAccessor, WebCategoryService webCategoryService, WebChannelService webChannelService, ArticleService articleService)
        {
            _claimsAccessor = claimsAccessor;
            _webCategoryService = webCategoryService;
            _webChannelService = webChannelService;
            _articleService = articleService;
        }

        #region webCategory操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<WebCategory>();
            exWhere.And(a => a.States < 99 && a.ParentId == 0);
            if (queryModel.queryState.HasValue) { exWhere.And(a => a.States == queryModel.queryState.Value); }
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                exWhere.And(a => queryModel.queryName.Contains(a.Id.ToString()));
            }
            if (queryModel.startTime.HasValue) { exWhere.And(a => a.CreateTime >= queryModel.startTime.Value); }
            if (queryModel.endTime.HasValue) { exWhere.And(a => a.CreateTime <= queryModel.endTime.Value); }
            if (queryModel.channelId.HasValue) exWhere.And(it => it.channelId == queryModel.channelId.Value);
            var list = new List<WebCategoryView>();
            var data = await _webCategoryService.GetPageListAsync(exWhere, p, it => new { it.Sort, it.CreateTime }, OrderByType.Desc);
            if (data != null && data.Count > 0)
            {
                var pIds = data.Select(it => it.Id).ToList();
                var cList = await _webCategoryService.GetListAsync(it => SqlFunc.ContainsArray(pIds, it.ParentId) && it.States < 99);
                cList = cList.OrderByDescending(it => new { it.Sort, it.CreateTime }).ToList();

                list = mapper.ToViewList(data);
                foreach (var item in list)
                {
                    item.ChildrenList = cList.FindAll(it => it.ParentId == item.Id);
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(WebCategoryView model)
        {
            var res = new ResultModel();
            var info = mapper.ToModel(model);
            //频道
            var channel = await _webChannelService.GetByIdAsync(info.channelId);
            if (channel != null)
            {
                info.channelName = channel.title;
            }

            bool isok = false;
            if (info.Id > 0)
            {
                info.UpdateTime = DateTime.Now;

                isok = await _webCategoryService.UpdateAsync(info);
            }
            else
            {
                info.CreateTime = DateTime.Now;
                info.UpdateTime = DateTime.Now;
                isok = await _webCategoryService.InsertAsync(info);
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
        public async Task<ResultModel> DelWebCategory(DelModel del)
        {
            var res = new ResultModel();
            if (del.ids == null || del.ids.Length <= 0)
            {
                res.msg = "请选择删除数据";
                return res;
            }
            var isok = await _webCategoryService.UpdateAsync(it => new WebCategory { States = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");
            return res;
        }
        #endregion
    }
}