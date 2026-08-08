using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{

    /// <summary>
    /// 控制器层 SysUserCouponController
    /// </summary>
    public class SysUserCouponController : BaseController
    {

        private readonly IUserCouponService _userCouponService;
        private readonly IUserInfoService _userInfoService;
        private readonly ICouponService _couponService;

        private readonly UserCouponMapper mapper = new();

        public SysUserCouponController(IClaimsAccessor claimsAccessor,
            UserCouponService userCouponService, CouponService couponService, UserInfoService userInfoService)
        {
            _claimsAccessor = claimsAccessor;
            _userCouponService = userCouponService;
            _userInfoService = userInfoService;
            _couponService = couponService;
        }

        #region userCoupon操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<UserCoupon>();
            exWhere.And(a => a.status < 99);
            if (queryModel.queryState.HasValue)
            {
                exWhere.And(a => a.status == queryModel.queryState.Value);
            }
            if (queryModel.queryType.HasValue)
            {
                exWhere.And(a => a.couponType == queryModel.queryType.Value);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                if (int.TryParse(queryModel.queryName, out tId)) exWhere.And(a => a.Id == tId || a.couponId == tId || a.userId == tId);
                else exWhere.And(a => a.couponTitle.Contains(queryModel.queryName) || a.intro.Contains(queryModel.queryName) || a.sourceNo.Contains(queryModel.queryName));

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
            var data = await _userCouponService.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            if (data.Count > 0)
            {
                var uids = data.Select(a => a.userId).Distinct().ToList();
                var ulist = await _userInfoService.GetListAsync(a => SqlFunc.ContainsArray(uids, a.Id));

                list = mapper.ToViewList(data);

                foreach (var item in list)
                {
                    if (ulist.Count(a => a.Id == item.userId) > 0) item.userName = ulist.FirstOrDefault(a => a.Id == item.userId).nickName;
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }



        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> AddOperation(UserCouponView model)
        {
            var res = new ResultModel();

            if (model.CouponIds == null || model.CouponIds.Count <= 0)
            {
                res.msg = "请选择优惠券";
                return res;
            }

            if (model.UserIds == null || model.UserIds.Count <= 0)
            {
                res.msg = "请选择用户";
                return res;
            }

            var clist = await _couponService.GetListAsync(a => a.status == 0 && SqlFunc.ContainsArray(model.CouponIds, a.Id));
            if (clist == null || clist.Count <= 0)
            {
                res.msg = "选择的优惠券不存在";
                return res;
            }
            var ulist = await _userInfoService.GetListAsync(a => a.status == 0 && SqlFunc.ContainsArray(model.UserIds, a.Id));
            if (ulist == null || ulist.Count <= 0)
            {
                res.msg = "选择的用户不存在";
                return res;
            }
            var list = new List<UserCoupon>();
            foreach (var coupon in clist)
            {
                foreach (var user in ulist)
                {
                    list.Add(new UserCoupon
                    {
                        couponId = coupon.Id,
                        couponTitle = coupon.title,
                        couponType = coupon.couponType,
                        createTime = DateTime.Now,
                        discount = coupon.discount,
                        intro = coupon.intro,
                        sourceNo = "",
                        sourceType = 0,
                        startAmount = coupon.startAmount,
                        status = 0,
                        updateTime = DateTime.Now,
                        endTime = DateOnly.FromDateTime(DateTime.Now.AddDays(coupon.dayTime)),
                        userId = user.Id
                    });
                }
            }
            var isok = await _userCouponService.InsertAsync(list);
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");

            return res;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(UserCouponView model)
        {
            var res = new ResultModel();

            var info = mapper.ToModel(model);
            bool isok = false;
            if (info.Id > 0)
            {
                info.updateTime = DateTime.Now;
                isok = await _userCouponService.UpdateAsync(info);
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
        public async Task<ResultModel> DelUserCoupon(DelModel del)
        {
            var res = new ResultModel();

            if (del.ids == null || del.ids.Length <= 0)
            {
                res.msg = "请选择删除数据";
                return res;
            }
            var isok = await _userCouponService.UpdateAsync(it => new UserCoupon { status = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));

            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");


            return res;

        }
        #endregion
    }
}