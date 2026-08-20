namespace YW.Service
{
    public partial interface ISysMenuService : IBaseRepository<SysMenu>
    {
        Task<ResultModel> TreeList(QueryModel queryModel);

    }
    public partial class SysMenuService : BaseRepository<SysMenu>, ISysMenuService
    {
        private readonly SysMenuMapper _mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public SysMenuService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

        private readonly JwtService _jwtService;
        public SysMenuService(JwtService jwtService, IClaimsAccessor claimsAccessor)
        {
            _jwtService = jwtService;
            _claimsAccessor = claimsAccessor;
        }


        public async Task<ResultModel> TreeList(QueryModel queryModel)
        {
            var res = new ResultModel();
            var exWhere = PredicateBuilder.New<SysMenu>();
            exWhere.And(a => !a.delFlag);
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                exWhere.And(a => a.name.Contains(queryModel.queryName));
            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= queryModel.endTime.Value);
            }
            if (queryModel.Ids != null && queryModel.Ids.Count > 0)
            {
                exWhere.And(it => SqlFunc.ContainsArray(queryModel.Ids, it.Id));
            }
            if (queryModel.queryTypeArr != null && queryModel.queryTypeArr.Count > 0)
            {
                exWhere.And(it => SqlFunc.ContainsArray(queryModel.queryTypeArr, it.menuType));
            }

            var list = new List<SysUserMenu>();
            var data = await db.Queryable<SysMenu>().Where(exWhere).OrderByDescending(it => it.sort)
                .Select(it => new SysUserMenu
                {
                    name = it.name,
                    component = it.component,
                    redirect = it.redirect,
                    path = it.path,
                    meta = new SysUserMenuMeta
                    {
                        icon = it.icon,
                        activeMenu = it.activeMenu,
                        isAffix = it.isAffix,
                        isFull = it.isFull,
                        isHide = it.isHide,
                        isKeepAlive = it.isKeepAlive,
                        isLink = it.isLink,
                        title = it.title
                    },
                    pid = it.pid,
                    Id = it.Id,
                    menuType = it.menuType,
                    sort = it.sort
                }).ToTreeAsync(it => it.children, it => it.pid, 0);

            res.data = data;
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            return res;
        }

    }
}
