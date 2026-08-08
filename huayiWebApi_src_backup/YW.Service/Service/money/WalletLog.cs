namespace YW.Service
{
    public partial interface IWalletLogService : IBaseRepository<WalletLog>
    {

        /// <summary>
        /// 后台列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<ResultModel> BackEndList(QueryModel queryModel);

        /// <summary>
        /// 我的钱包
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        Task<ResultModel> MyWallet(QueryModel view, UserInfo user);

        Task<ResultModel> FrontEndList(QueryModel view, UserInfo user);
    }
    public partial class WalletLogService : BaseRepository<WalletLog>, IWalletLogService
    {
        private readonly WalletLogMapper mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public WalletLogService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }
        #region 列表
        public async Task<ResultModel> FrontEndList(QueryModel view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<WalletLog>(a => a.userId == user.Id && a.userType == (int)walletUserTypeEnum.用户);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                int tId = 0;
                int.TryParse(view.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.title.Contains(view.queryName) || a.orderNo.Contains(view.queryName) || a.remark.Contains(view.queryName));
            }
            if (view.queryId.HasValue)
            {
                exWhere.And(a => a.sourceType == view.queryId.Value);
            }
            if (view.queryType.HasValue) exWhere.And(a => a.wType == view.queryType.Value);
            else exWhere.And(a => a.wType == (int)walletTypeEnum.余额);

            if (view.queryYear.HasValue)
            {
                exWhere.And(a => a.createTime.Year == view.queryYear.Value);
            }
            if (view.queryMonth.HasValue)
            {
                exWhere.And(a => a.createTime.Month == view.queryMonth.Value);
            }

            if (view.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= view.startTime.Value);
            }
            if (view.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= view.endTime.Value);
            }

            var data = await base.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = data };
            return res;
        }

        public async Task<ResultModel> BackEndList(QueryModel view)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<WalletLog>(a => a.Id > 0);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                int tId = 0;
                int.TryParse(view.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.title.Contains(view.queryName) || a.orderNo.Contains(view.queryName) || a.remark.Contains(view.queryName));
            }
            if (view.queryId.HasValue) exWhere.And(a => a.userType == view.queryId.Value);
            if (view.parentId.HasValue) exWhere.And(it => it.wType == view.parentId.Value);
            if (view.queryType.HasValue) exWhere.And(a => a.sourceType == view.queryType.Value);
            if (view.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= view.startTime.Value);
            }
            if (view.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= view.endTime.Value);
            }

            var list = new List<WalletLogView>();
            var data = await WalletLogDb.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            if (data != null && data.Count > 0)
            {
                //用户列表
                var uIds = data.Select(it => it.userId).Distinct().ToList();
                var uList = await UserInfoDb.GetListAsync(it => SqlFunc.ContainsArray(uIds, it.Id));


                list = mapper.ToViewList(data);
                foreach (var item in list)
                {
                    //用户信息
                    if (item.userId > 0 && uList.Count(it => it.Id == item.userId) > 0)
                    {
                        var uInfo = uList.Find(it => it.Id == item.userId);
                        item.userName = uInfo.nickName;
                    }
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }

        #endregion

        #region 我的钱包
        public async Task<ResultModel> MyWallet(QueryModel view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            //余额
            var wlist = await WalletLogDb.GetListAsync(a => a.userId == user.Id && a.wType == (int)walletTypeEnum.余额 && a.userType == (int)walletUserTypeEnum.用户);
            //今日收入
            decimal dayTotal = 0;
            if (wlist != null && wlist.Count(a => a.createTime.Date == DateTime.Now.Date) > 0)
            {
                dayTotal = wlist.Where(a => a.createTime.Date == DateTime.Now.Date).Sum(a => a.change);
            }
            //昨日收入
            decimal yesterdayTotal = 0M;
            if (wlist != null && wlist.Count(a => a.createTime.Date == DateTime.Now.AddDays(-1).Date) > 0)
            {
                yesterdayTotal = wlist.Where(a => a.createTime.Date == DateTime.Now.AddDays(-1).Date).Sum(a => a.change);
            }
            //已提现收入
            decimal withdrawTotal = 0;
            if (WithdrawalDb.CountAsync(a => a.userType == 0 && a.status == 1 && a.userId == user.Id).Result > 0)
            {
                withdrawTotal = await db.Queryable<Withdrawal>().Where(a => a.userType == 0 && a.status == 1 && a.userId == user.Id).SumAsync(a => a.amount);
            }
            //冻结收入=待提现金额
            decimal frozenTotal = 0;
            if (WithdrawalDb.CountAsync(a => a.userType == 0 && a.status == 0 && a.userId == user.Id).Result > 0)
            {
                frozenTotal = await db.Queryable<Withdrawal>().Where(a => a.userType == 0 && a.status == 0 && a.userId == user.Id).SumAsync(a => a.amount);
            }
            res.data = new
            {
                yesterdayTotal,
                dayTotal,
                withdrawTotal,
                frozenTotal,
                user.amount,
            };
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            return res;
        }
        #endregion
    }
}