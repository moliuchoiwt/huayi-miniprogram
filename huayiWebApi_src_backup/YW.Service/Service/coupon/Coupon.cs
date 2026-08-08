namespace YW.Service
{
    public partial interface ICouponService : IBaseRepository<Coupon>
    {
        /// <summary>
        /// 领取优惠券
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        Task<ResultModel> Receive(QueryModel view);
        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ResultModel> Operation(CouponView model);

        /// <summary>
        /// 后端列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<ResultModel> backEndList(couponQuery queryModel);

        /// <summary>
        /// 发放优惠券给用户
        /// </summary>        
        Task<ResultModel> GiveCouponToUser(GiveCouponQuery view);

        Task<ResultModel> frontEndList(QueryModel queryModel);
    }

    public partial class CouponService : BaseRepository<Coupon>, ICouponService
    {
        private readonly CouponMapper _mapper;
        private readonly IClaimsAccessor _claimsAccessor;

        public CouponService(IClaimsAccessor claimsAccessor, CouponMapper mapper)
        {
            _claimsAccessor = claimsAccessor;
            _mapper = mapper;
        }
        #region 列表
        public async Task<ResultModel> frontEndList(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<Coupon>(a => a.status == 0);
            if (queryModel.queryId.HasValue)
            {
                var ids = new List<int>();
                var rlist = await CouponRoleDb.GetListAsync(a => a.State == 0 && a.ShopId == queryModel.queryId.Value);
                if (rlist != null && rlist.Count > 0)
                {
                    ids = rlist.Select(a => a.CouponId).ToList();
                }
                exWhere.And(a => SqlFunc.ContainsArray(ids, a.Id));
            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= queryModel.endTime.Value);
            }

            var data = await base.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = data };
            return res;
        }

        public async Task<ResultModel> backEndList(couponQuery queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<Coupon>();
            exWhere.And(a => a.status < 99);
            if (queryModel.status.HasValue) exWhere.And(a => a.status == queryModel.queryState.Value);

            if (queryModel.queryType.HasValue)
            {
                exWhere.And(a => a.couponType == queryModel.queryType.Value);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                int.TryParse(queryModel.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.title.Contains(queryModel.queryName) || a.intro.Contains(queryModel.queryName));
            }
            if (queryModel.queryId.HasValue)
            {
                var cids = new List<int>();
                var rlist = await CouponRoleDb.GetListAsync(a => a.State == 0 && a.ShopId == queryModel.queryId.Value);
                if (rlist.Count > 0) cids = rlist.Select(a => a.CouponId).ToList();
                exWhere.And(a => SqlFunc.ContainsArray(cids, a.Id));
            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.updateTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.updateTime <= queryModel.endTime.Value);
            }
            var list = new List<CouponView>();
            var data = await CouponDb.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            if (data != null && data.Count > 0)
            {
                list = _mapper.ToViewList(data);
                var cids = data.Select(a => a.Id).ToList();
                var rlist = await CouponRoleDb.GetListAsync(a => a.State == 0 && SqlFunc.ContainsArray(cids, a.CouponId));

                foreach (var item in list)
                {
                    if (rlist.Count(a => a.CouponId == item.Id) > 0)
                    {
                        item.goodsClassIdsList = rlist.Where(a => a.CouponId == item.Id).Select(a => a.GoodsClassId).ToList();
                    }
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }

        #endregion

        #region 领取优惠券
        /// <summary>
        /// 领取优惠券
        /// </summary>
        public async Task<ResultModel> Receive(QueryModel view)
        {
            var res = new ResultModel();
            if (view == null)
            {
                res.msg = "参数不能为空";
                return res;
            }
            if (view.Ids == null || view.Ids.Count <= 0)
            {
                res.msg = "领取的优惠券不能为空";
                return res;
            }
            var couponList = await base.GetListAsync(a => a.status == 0 && SqlSugar.SqlFunc.ContainsArray(view.Ids, a.Id));
            if (couponList == null && couponList.Count <= 0)
            {
                res.msg = "领取的优惠券不存在";
                return res;
            }
            var user = UserInfoDb.GetById(_claimsAccessor.UserId);
            if (user == null && user.Id <= 0)
            {
                res.msg = "用户信息不存在";
                return res;
            }
            db.Ado.BeginTran();

            try
            {

                foreach (var item in couponList)
                {

                    if (UserCouponDb.Count(a => a.userId == user.Id && a.status == 0 && a.couponId == item.Id) > 0)
                    {
                        db.Ado.RollbackTran();
                        res.msg = $"{item.title}已领取,请勿重复领取";
                        return res;
                    }

                    item.distributeNum += 1;
                    item.updateTime = DateTime.Now;
                    await base.UpdateAsync(item);

                    //发放优惠券
                    UserCouponDb.Insert(new UserCoupon
                    {
                        status = 0,
                        userId = user.Id,
                        endTime = DateOnly.FromDateTime(DateTime.Now.AddDays(item.dayTime)),
                        createTime = DateTime.Now,
                        updateTime = DateTime.Now,
                        sourceType = 1,
                        couponTitle = item.title,
                        couponId = item.Id,
                        couponType = item.couponType,
                        discount = item.discount,
                        intro = item.intro,
                        startAmount = item.startAmount,
                        sourceNo = "",
                    });
                }

                res.msg = "SUCCESS";
                res.code = (int)ResultEnum.success;
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                Common.LogHelper.Error("订单回调错误", ex);
                res.msg = ex.Message;
                return res;

            }
            db.Ado.CommitTran();

            return res;
        }

        #endregion

        #region 新增/修改优惠券
        public async Task<ResultModel> Operation(CouponView model)
        {
            var res = new ResultModel();
            if (string.IsNullOrWhiteSpace(model.title))
            {
                res.msg = "优惠券标题不能为空";
                return res;
            }
            if (model.startAmount <= 0)
            {
                res.msg = $"优惠券满多少金额不能小于零";
                return res;
            }
            if (model.discount <= 0)
            {
                res.msg = $"优惠券{(model.couponType == 0 ? "满减" : "折扣")}不能小于零";
                return res;
            }
            var info = _mapper.ToModel(model);
            info.createId = (int)_claimsAccessor.UserId;

            try
            {
                await db.Ado.BeginTranAsync();

                if (info.Id > 0)
                {
                    info.updateTime = DateTime.Now;
                    await CouponDb.UpdateAsync(info);
                }
                else
                {
                    info.createTime = DateTime.Now;
                    info.updateTime = DateTime.Now;
                    info.Id = await CouponDb.InsertReturnIdentityAsync(info);
                }
                var cIds = new List<int>();
                if (model.goodsClassIdsList != null && model.goodsClassIdsList.Count > 0) cIds = model.goodsClassIdsList;

                await CouponRoleDb.DeleteAsync(a => a.CouponId == info.Id);
                var c_rList = new List<CouponRole>();
                foreach (var itemId in cIds)
                {
                    c_rList.Add(new CouponRole { UpdateTime = DateTime.Now, CreateTime = DateTime.Now, CouponId = info.Id, State = 0, GoodsClassId = itemId });
                }
                await CouponRoleDb.InsertRangeAsync(c_rList);
                await db.Ado.CommitTranAsync();
                res.msg = "操作成功";
                res.code = (int)ResultEnum.success;
            }
            catch (Exception ex)
            {
                await db.Ado.RollbackTranAsync();
                LogHelper.Error("新增/修改优惠券错误", ex);
                res.msg = ex.Message;
                return res;
            }
            return res;
        }
        #endregion

        #region 发放优惠券给用户
        public async Task<ResultModel> GiveCouponToUser(GiveCouponQuery view)
        {
            var res = new ResultModel();
            if (view == null)
            {
                res.msg = "参数不能为空";
                return res;
            }
            if (view.couponId == 0 || CouponDb.Count(it => it.Id == view.couponId) == 0)
            {
                res.msg = "优惠券不存在";
                return res;
            }
            if (view.giveType == -1 && (view.userIds == null || view.userIds.Count == 0))
            {
                res.msg = "请指定发放的用户";
                return res;
            }
            var uCouponList = new List<UserCoupon>();
            var coupon = CouponDb.GetById(view.couponId);
            var User_exWhere = PredicateBuilder.New<UserInfo>(it1 => it1.status != 99);
            if (view.userIds != null && view.userIds.Count > 0)//发放给部分用户
            {
                User_exWhere.And(it => SqlFunc.ContainsArray(view.userIds, it.Id));
            }
            var userList = await db.Queryable<UserInfo>().Where(User_exWhere).ToListAsync();
            if (userList != null)
            {
                foreach (var user in userList)
                {
                    coupon.distributeNum += 1;

                    var uCoupon = new UserCoupon
                    {
                        status = 0,
                        userId = user.Id,
                        endTime = DateOnly.FromDateTime(DateTime.Now.AddDays(coupon.dayTime)),
                        createTime = DateTime.Now,
                        updateTime = DateTime.Now,
                        sourceType = 0,
                        couponTitle = coupon.title,
                        couponId = coupon.Id,
                        couponType = coupon.couponType,
                        discount = coupon.discount,
                        intro = coupon.intro,
                        startAmount = coupon.startAmount,
                        sourceNo = ""
                    };
                    uCouponList.Add(uCoupon);
                }
            }
            try
            {



                db.BeginTran();
                coupon.updateTime = DateTime.Now;
                await CouponDb.UpdateAsync(coupon);
                await UserCouponDb.InsertRangeAsync(uCouponList);
                db.CommitTran();
                res.msg = "操作成功";
                res.code = (int)ResultEnum.success;
            }
            catch (Exception ex)
            {
                db.RollbackTran();
                res.msg = ex.Message;
            }
            return res;
        }
        #endregion
    }
}