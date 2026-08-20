namespace YW.Service
{
    public partial interface IClassService : IBaseRepository<Class>
    {
        /// <summary>
        /// 后台列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<ResultModel> backEndList(QueryModel queryModel);

        /// <summary>
        /// 前端列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<ResultModel> frontEndList(QueryModel queryModel);

    }
    public partial class ClassService : BaseRepository<Class>, IClassService
    {
        private readonly ClassMapper _mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public ClassService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

        #region 列表
        public async Task<ResultModel> backEndList(QueryModel queryModel)
        {
            var res = new ResultModel();
            var exWhere = PredicateBuilder.New<Class>(it => it.status != 99);
            if (queryModel.queryType.HasValue)
            {
                exWhere.And(a => a.cType == queryModel.queryType.Value);
            }
            else exWhere.And(it => it.cType == 0);

            if (queryModel.parentId.HasValue)
            {
                exWhere.And(a => a.parentId == queryModel.parentId.Value);
            }
            else exWhere.And(it => it.parentId == 0);
            if (queryModel.queryState.HasValue)
            {
                exWhere.And(a => a.status == queryModel.queryState.Value);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                int.TryParse(queryModel.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.title.Contains(queryModel.queryName) || a.intro.Contains(queryModel.queryName) || a.imgUrl.Contains(queryModel.queryName) || a.link.Contains(queryModel.queryName));
            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= queryModel.endTime.Value);
            }
            var adminId = (int)_claimsAccessor.UserId;
            if (sysUserDb.Count(it => it.Id == adminId) == 0)
            {
                res.msg = "登录失效，请重新登录";
                return res;
            }
            var admin = await sysUserDb.GetByIdAsync(adminId);
            if (admin.roleId == 3)
            {
                exWhere.And(it => SqlFunc.SplitIn(admin.classIds, it.Id.ToString()));
            }

            var data = await db.Queryable<Class>().Where(exWhere).OrderByDescending(it => it.sort).ToListAsync();

            var list = _mapper.ToViewList(data);
            if (list.Count > 0)
            {
                var ids = list.Select(a => a.Id).ToList();
                var clist = await ClassDb.GetListAsync(a => a.status != 99 && SqlFunc.ContainsArray(ids, a.parentId));
                foreach (var item in list)
                {
                    if (clist.Count(a => a.parentId == item.Id) > 0) item.children = clist.Where(a => a.parentId == item.Id).OrderByDescending(it => it.sort).ToList();

                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { items = list };//total = list.Count,
            return res;
        }

        public async Task<ResultModel> frontEndList(QueryModel queryModel)
        {
            var res = new ResultModel();
            var exWhere = PredicateBuilder.New<Class>(a => a.status == 0);

            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                int.TryParse(queryModel.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.title.Contains(queryModel.queryName) || a.intro.Contains(queryModel.queryName) || a.imgUrl.Contains(queryModel.queryName) || a.link.Contains(queryModel.queryName));
            }
            if (queryModel.queryType.HasValue)
            {
                exWhere.And(a => a.cType == queryModel.queryType.Value);
            }
            else exWhere.And(it => it.cType == 0);
            if (queryModel.parentId.HasValue)
            {
                exWhere.And(a => a.parentId == queryModel.parentId.Value);
            }
            else exWhere.And(it => it.parentId == 0);
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= queryModel.endTime.Value);
            }

            var data = await db.Queryable<Class>().Where(exWhere).OrderByDescending(it => it.sort).ToListAsync();
            var list = _mapper.ToViewList(data);
            if (list.Count > 0)
            {
                var ids = list.Select(a => a.Id).ToList();
                var glist = await GoodsDb.GetListAsync(a => a.status == 0 && SqlFunc.ContainsArray(ids, a.classId) && a.gType == (int)GoodsTypeEnum.普通商品);
                if (glist != null && glist.Count > 0)
                {
                    foreach (var item in glist)
                    {
                        item.coverPicture = WebFileHelper.GetUrl(item.coverPicture);
                    }
                }

                foreach (var item in list)
                {
                    item.goodsList = glist.FindAll(it => it.classId == item.Id);
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { items = list };
            return res;
        }
        #endregion

    }
}