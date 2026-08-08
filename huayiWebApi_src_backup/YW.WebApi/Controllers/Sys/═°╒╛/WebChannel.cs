using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysWebChannelController
    /// </summary>
    public class SysWebChannelController : BaseController
    {

        private readonly IWebChannelService _webChannelService;
        private readonly IWebCategoryService _webCategoryService;
        private readonly IArticleService _articleService;

        private readonly WebChannelMapper mapper = new();
        public SysWebChannelController(IClaimsAccessor claimsAccessor, WebChannelService webChannelService, WebCategoryService webCategoryService, ArticleService articleService)
        {
            _claimsAccessor = claimsAccessor;
            _webChannelService = webChannelService;
            _webCategoryService = webCategoryService;
            _articleService = articleService;
        }

        #region webChannel操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<WebChannel>();
            exWhere.And(a => a.States < 99);
            if (queryModel.queryState.HasValue) { exWhere.And(a => a.States == queryModel.queryState.Value); }
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                exWhere.And(a => queryModel.queryName.Contains(a.Id.ToString()));
            }
            if (queryModel.startTime.HasValue) { exWhere.And(a => a.CreateTime >= queryModel.startTime.Value); }
            if (queryModel.endTime.HasValue) { exWhere.And(a => a.CreateTime <= queryModel.endTime.Value); }
            var list = new List<WebChannelView>();
            var data = await _webChannelService.GetPageListAsync(exWhere, p, it => new { it.CreateTime, it.Id }, OrderByType.Desc);
            if (data != null)
            {
                list = mapper.ToViewList(data);
                foreach (var item in list)
                {
                    if (!string.IsNullOrWhiteSpace(item.ImgUrlList)) item.ImgList = GetListUrl(item.ImgUrlList);
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
        public async Task<ResultModel> Operation(WebChannelView model)
        {
            var res = new ResultModel();
            var info = mapper.ToModel(model);
            bool isok = false;
            if (info.Id > 0)
            {
                info.UpdateTime = DateTime.Now;

                isok = await _webChannelService.UpdateAsync(info);
            }
            else
            {
                info.CreateTime = DateTime.Now;
                info.UpdateTime = DateTime.Now;
                isok = await _webChannelService.InsertAsync(info);
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
        public async Task<ResultModel> DelWebChannel(DelModel del)
        {
            var res = new ResultModel();
            if (del.ids == null || del.ids.Length <= 0)
            {
                res.msg = "请选择删除数据";
                return res;
            }
            var isok = await _webChannelService.UpdateAsync(it => new WebChannel { States = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");
            return res;
        }
        #endregion
    }
}