namespace YW.Service
{
    public partial interface IBannerService : IBaseRepository<Banner>
    {
        Task<ResultModel> FrontEndList(QueryModel view);

        Task<ResultModel> BackEndList(QueryModel view);

    }
    public partial class BannerService : BaseRepository<Banner>, IBannerService
    {

        #region 列表
        public async Task<ResultModel> FrontEndList(QueryModel view)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<Banner>(a => a.status == 0);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                int tId = 0;
                int.TryParse(view.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.title.Contains(view.queryName) || a.intro.Contains(view.queryName) || a.link.Contains(view.queryName));

            }

            if (view.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= view.startTime.Value);
            }
            if (view.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= view.endTime.Value);
            }
            if (view.Ids != null && view.Ids.Count > 0)
            {
                exWhere.And(it => SqlFunc.ContainsArray(view.Ids, it.Id) && it.bType == 1);
            }
            else
            {
                if (view.queryType.HasValue)
                {
                    exWhere.And(a => a.bType == view.queryType.Value);
                }
                else exWhere.And(it => it.bType == 0);
            }

            var bList = new Dictionary<int, Banner>();
            var data = await base.GetPageListAsync(exWhere, p, it => new { it.sort, it.createTime }, OrderByType.Desc);

            if (data != null && data.Count > 0)
            {
                foreach (var item in data)
                {
                    item.imgUrl = WebFileHelper.GetUrl(item.imgUrl);
                    if (view.Ids != null && view.Ids.Count > 0) bList.Add(item.Id, item);
                }
            }

            if (view.Ids != null && view.Ids.Count > 0)
            {
                res.data = new { total = p.TotalCount, items = bList };
            }
            else
            {
                res.data = new { total = p.TotalCount, items = data };
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            return res;
        }

        public async Task<ResultModel> BackEndList(QueryModel view)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<Banner>();
            exWhere.And(a => a.status != 99);
            if (view.queryState.HasValue)
            {
                exWhere.And(a => a.status == view.queryState.Value);
            }
            if (view.queryType.HasValue)
            {
                exWhere.And(a => a.bType == view.queryType.Value);
            }
            else exWhere.And(it => it.bType == 0);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                int tId = 0;
                int.TryParse(view.queryName, out tId);
                exWhere.And(a => a.title.Contains(view.queryName) || a.intro.Contains(view.queryName) || a.link.Contains(view.queryName) || a.Id == tId);
            }
            if (view.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= view.startTime.Value);
            }
            if (view.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= view.endTime.Value);
            }

            var data = await base.GetPageListAsync(exWhere, p, it => new { it.sort, it.createTime }, OrderByType.Desc);
            if (data != null && data.Count > 0)
            {
                foreach (var item in data)
                {
                    item.imgUrl = WebFileHelper.GetUrl(item.imgUrl);
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = data };
            return res;
        }
        #endregion
    }
}
