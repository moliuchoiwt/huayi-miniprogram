using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysCouponController
    /// </summary>
    public class SysCouponController : BaseController
    {

        private readonly ICouponService _couponService;
        private readonly ICouponRoleService _couponRoleService;
        public SysCouponController(IClaimsAccessor claimsAccessor,
            CouponService couponService, CouponRoleService couponRoleService)
        {
            _claimsAccessor = claimsAccessor;
            _couponService = couponService;
            _couponRoleService = couponRoleService;
        }

        #region coupon操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(couponQuery queryModel) => await _couponService.backEndList(queryModel);


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(CouponView view) => await _couponService.Operation(view);


        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Delete(DelModel del)
        {
            var res = new ResultModel();

            if (del.ids == null || del.ids.Length <= 0)
            {
                res.msg = "请选择删除数据";
                return res;
            }
            var isok = await _couponService.UpdateAsync(it => new Coupon { status = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));

            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");


            return res;

        }
        #endregion


        #region 发放优惠券
        [HttpPost]
        public async Task<ResultModel> GiveCouponToUser(GiveCouponQuery view) => await _couponService.GiveCouponToUser(view);

        #endregion
    }
}