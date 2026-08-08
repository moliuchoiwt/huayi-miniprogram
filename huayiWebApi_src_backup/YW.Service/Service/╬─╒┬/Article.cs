namespace YW.Service
{
    public partial interface IArticleService : IBaseRepository<Article>
    {
        /// <summary>
        ///  后台列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<ResultModel> backEndList(QueryModel queryModel);

        /// <summary>
        /// 前端列表
        /// </summary>
        /// <param name="view"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<ResultModel> frontEndList(QueryModel view, UserInfo user);

        /// <summary>
        /// 前端详情
        /// </summary>
        /// <param name="view"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<ResultModel> frontEndDetails(QueryModel view, UserInfo user);
    }

    public partial class ArticleService : BaseRepository<Article>, IArticleService
    {
        private readonly ArticleMapper mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public ArticleService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

        #region 列表
        public async Task<ResultModel> backEndList(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<Article>(a => a.status != 99);
            if (queryModel.queryState.HasValue)
            {
                exWhere.And(a => a.status == queryModel.queryState.Value);
            }
            if (queryModel.queryType.HasValue)
            {
                exWhere.And(a => a.articleType == queryModel.queryType.Value);
            }
            else exWhere.And(it => it.articleType == 0);
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                int.TryParse(queryModel.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.userId == tId || a.title.Contains(queryModel.queryName)
                || a.intro.Contains(queryModel.queryName) || a.contents.Contains(queryModel.queryName)
                || a.userName.Contains(queryModel.queryName) || a.auditIntro.Contains(queryModel.queryName));
            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= queryModel.endTime.Value);
            }

            var data = await ArticleDb.GetPageListAsync(exWhere, p, it => new { it.sort, it.createTime }, OrderByType.Desc);
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = data };
            return res;
        }

        public async Task<ResultModel> frontEndList(QueryModel view, UserInfo user)
        {
            var res = new ResultModel();
            var _mapper = new ArticleMapper();
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<Article>(a => a.status != 99);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                exWhere.And(a => a.userName.Contains(view.queryName) || a.title.Contains(view.queryName) || a.intro.Contains(view.queryName));

            }
            if (view.queryType.HasValue)
            {
                exWhere.And(a => a.articleType == view.queryType.Value);
            }
            else exWhere.And(it => it.articleType == 0);
            if (view.userId.HasValue)
            {
                exWhere.And(a => a.userId == view.userId.Value);
            }
            if (view.queryState.HasValue)
            {
                exWhere.And(a => a.status == view.queryState.Value);
            }
            else exWhere.And(it => it.status == 0);
            if (view.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= view.startTime.Value);
            }
            if (view.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= view.endTime.Value);
            }
            ////我关注用户发布的文章
            //if (queryModel.isFollow.HasValue && queryModel.isFollow.Value == 1)
            //{
            //    var uIds = new List<int>();
            //    var flist = _followsService.GetListAsync(a => a.State == 1 && a.UserId == _claimsAccessor.UserId).Result;
            //    if (flist != null && flist.Count > 0)
            //    {
            //        uIds = flist.Select(a => a.ToUserId).Distinct().ToList();
            //    }
            //    exWhere.And(a => SqlFunc.ContainsArray(uIds, a.userId));
            //}
            ////我点赞过的文章
            //if (queryModel.isLike.HasValue && queryModel.isLike.Value == 1)
            //{
            //    var ids = new List<int>();
            //    var alist = await _likesService.GetListAsync(a => a.LikesType == 0 && a.State == 1 && a.UserId == _claimsAccessor.UserId);
            //    if (alist != null && alist.Count > 0)
            //    {
            //        ids = alist.Select(a => a.LikesId).Distinct().ToList();
            //    }
            //    exWhere.And(a => SqlFunc.ContainsArray(ids, a.Id));
            //}
            ////我收藏的文章
            //if (queryModel.isCollection.HasValue && queryModel.isCollection.Value)
            //{
            //    var ids = new List<int>();
            //    var alist = await _collectionRecordService.GetListAsync(a => a.cType == 0 && a.status == 1 && a.userId == _claimsAccessor.UserId);
            //    if (alist != null && alist.Count > 0)
            //    {
            //        ids = alist.Select(a => a.cId).Distinct().ToList();
            //    }
            //    exWhere.And(a => SqlFunc.ContainsArray(ids, a.Id));
            //}

            System.Linq.Expressions.Expression<Func<Article, object>> order = it => new { it.updateTime, it.Id };
            var orderbytype = OrderByType.Desc;
            //if (queryModel.orderBy.HasValue)
            //{
            //    switch (queryModel.orderBy.Value)
            //    {
            //        case 1:
            //            //推荐
            //            order = it => new { it.updateTime, it.Id };
            //            break;
            //        default:
            //            break;
            //    }
            //}

            var data = await base.GetPageListAsync(exWhere, p, order, orderbytype);
            var list = new List<ArticleView>();
            if (data.Count > 0)
            {
                list = _mapper.ToViewList(data);
                //var uids = list.Select(a => a.userId).ToList();
                //var ulist = await _userInfoService.GetListAsync(a => SqlFunc.ContainsArray(uids, a.Id));
                foreach (var item in list)
                {
                    item.contents = "";
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }

        #endregion

        #region 详情
        public async Task<ResultModel> frontEndDetails(QueryModel view, UserInfo user)
        {
            var res = new ResultModel();
            var _mapper = new ArticleMapper();

            if (view == null || !view.queryId.HasValue)
            {
                res.msg = "参数错误";
                return res;
            }
            var data = await base.GetByIdAsync(view.queryId.Value);
            if (data == null || data.Id <= 0)
            {
                res.msg = "文章参数错误";
                return res;
            }

            var ainfo = _mapper.ToView(data);

            ainfo.contents = WebFileHelper.getContent(ainfo.contents);

            ainfo.url = ainfo.articleType > 0 ? WebFileHelper.GetUrl(ainfo.url) : String.Join(';', WebFileHelper.GetListUrl(ainfo.url));
            ainfo.coverUrl = WebFileHelper.GetUrl(ainfo.coverUrl);

            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = ainfo;
            return res;
        }

        #endregion
    }
}