namespace YW.Service
{
    public partial interface IUserCouponService : IBaseRepository<UserCoupon>
    {
        /// <summary>
        /// 前端列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<ResultModel> frontEndList(QueryModel queryModel);

    }
    public partial class UserCouponService : BaseRepository<UserCoupon>, IUserCouponService
    {
        private readonly UserCouponMapper _mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public UserCouponService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }
        #region 列表
        public async Task<ResultModel> frontEndList(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<UserCoupon>(a => a.status < 99 && a.userId == (int)_claimsAccessor.UserId);
            if (queryModel.queryState.HasValue)
            {
                exWhere.And(a => a.status == queryModel.queryState.Value);
            }
            if (queryModel.queryType.HasValue)
            {
                exWhere.And(a => a.couponType == queryModel.queryType.Value);
            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= queryModel.endTime.Value);
            }
            var list = new List<UserCouponView>();
            var data = await UserCouponDb.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            if (data != null && data.Count > 0)
            {
                var couponIds = data.Select(it => it.couponId).Distinct().ToList();
                var cRList = await CouponRoleDb.GetListAsync(it => SqlFunc.ContainsArray(couponIds, it.CouponId) && it.State == 0);
                var g_cIds = cRList.Select(it => it.GoodsClassId).Distinct().ToList();
                var g_cList = await ClassDb.GetListAsync(it => SqlFunc.ContainsArray(g_cIds, it.Id) && it.status == 0);

                list = _mapper.ToViewList(data);
                foreach (var item in list)
                {
                    if (cRList.Count(it => it.CouponId == item.couponId) > 0)
                    {
                        var i_cIds = cRList.Where(it => it.CouponId == item.couponId).Select(it => it.GoodsClassId).Distinct().ToList();
                        item.goodsClassNamesList = g_cList.Where(it => i_cIds.Contains(it.Id)).Select(it => it.title).ToList();
                    }

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
