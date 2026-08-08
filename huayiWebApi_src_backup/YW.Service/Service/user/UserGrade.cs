namespace YW.Service
{
    public partial interface IUserGradeService : IBaseRepository<UserGrade>
    {
        /// <summary>
        /// 前端列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<ResultModel> frontEndList(QueryModel queryModel, UserInfo user);

        /// <summary>
        /// 后台列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<ResultModel> backEndList(QueryModel queryModel);
    }
    public partial class UserGradeService : BaseRepository<UserGrade>, IUserGradeService
    {
        private readonly UserGradeMapper mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public UserGradeService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

        #region 列表
        /// <summary>
        /// 前端列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<ResultModel> frontEndList(QueryModel queryModel, UserInfo user)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<UserGrade>(it => it.status == 0);
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                exWhere.And(a => a.name.Contains(queryModel.queryName));
            }
            if (queryModel.queryState.HasValue) exWhere.And(it => it.status == queryModel.queryState.Value);
            if (queryModel.startTime.HasValue) { exWhere.And(a => a.createTime >= queryModel.startTime.Value); }
            if (queryModel.endTime.HasValue) { exWhere.And(a => a.createTime <= queryModel.endTime.Value); }
            var list = new List<UserGradeView>();
            var data = await UserGradeDb.GetPageListAsync(exWhere, p, it => new { it.jibie, it.createTime }, OrderByType.Asc);
            if (data != null && data.Count > 0)
            {
                list = mapper.ToViewList(data);

                foreach (var item in list)
                {

                }
            }
            var currentDate = DateOnly.FromDateTime(DateTime.Now);

            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }

        /// <summary>
        /// 后台列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        public async Task<ResultModel> backEndList(QueryModel queryModel)
        {
            var res = new ResultModel();
            if (queryModel == null) { res.msg = "参数错误"; return res; }
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<UserGrade>(it => it.status != 99);
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                exWhere.And(a => a.name.Contains(queryModel.queryName));
            }
            if (queryModel.queryState.HasValue) exWhere.And(it => it.status == queryModel.queryState.Value);
            if (queryModel.startTime.HasValue) { exWhere.And(a => a.createTime >= queryModel.startTime.Value); }
            if (queryModel.endTime.HasValue) { exWhere.And(a => a.createTime <= queryModel.endTime.Value); }
            var list = new List<UserGradeView>();
            var data = await UserGradeDb.GetPageListAsync(exWhere, p, it => new { it.jibie, it.createTime }, OrderByType.Asc);
            if (data != null && data.Count > 0)
            {
                list = mapper.ToViewList(data);

                foreach (var item in list)
                {
                    item.giveAwayCouponIdList = new List<string>();
                    if (!string.IsNullOrWhiteSpace(item.giveAwayCouponIds)) item.giveAwayCouponIdList = item.giveAwayCouponIds.Split(",").ToList();
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }

        #endregion
    }
}