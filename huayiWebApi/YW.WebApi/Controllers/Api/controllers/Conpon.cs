using LinqKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.ApiControllers
{
    /// <summary>
    /// 优惠券
    /// </summary>
    [Authorize(Roles = "api")]
    public class CouponController : BaseController
    {
        private readonly ICouponService _couponService;
        private readonly ICouponRoleService _couponRoleService;
        private readonly IUserCouponService _userCouponService;
        private readonly IRewardSetService _rewardSetService;
        private readonly IRewardRelationService _rewardRelationService;
        private readonly IRewardReceiveService _rewardReceiveService;
        private readonly RewardSetMapper rewardSetMapper = new();
        private readonly RewardRelationMapper rewardRelationMapper = new();

        public CouponController(CouponService couponService, IClaimsAccessor claimsAccessor,
          CouponRoleService couponRoleService, UserCouponService userCouponService,
           RewardSetService rewardSetService, RewardRelationService rewardRelationService, RewardReceiveService rewardReceiveService)
        {
            _claimsAccessor = claimsAccessor;
            _couponService = couponService;
            _couponRoleService = couponRoleService;
            _userCouponService = userCouponService;
            _rewardSetService = rewardSetService;
            _rewardRelationService = rewardRelationService;
            _rewardReceiveService = rewardReceiveService;
        }

        #region 优惠券

        /// <summary>
        /// 我的优惠券
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> MyCouponList(QueryModel queryModel) => await _userCouponService.frontEndList(queryModel);

        //店铺优惠券                
        [HttpPost]
        [AllowAnonymous]
        public async Task<ResultModel> CouponList(QueryModel view) => await _couponService.frontEndList(view);

        /// <summary>
        /// 领取优惠券
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Receive(QueryModel view) => await _couponService.Receive(view);

        #endregion

        #region 奖励礼包

        /// <summary>
        /// 礼包信息
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> RewardInfo(QueryModel queryModel)
        {
            var res = new ResultModel();
            var exWhere = PredicateBuilder.New<RewardSet>(a => a.State == 0);
            if (queryModel.queryType.HasValue)
            {
                exWhere.And(a => a.RewardType == queryModel.queryType.Value);
            }
            var reward = new RewardSetView();
            var data = await _rewardSetService.GetListAsync(exWhere);
            if (data.Count > 0)
            {
                reward = rewardSetMapper.ToView(data[0]);
                var rdata = await _rewardRelationService.GetListAsync(a => a.State == 0 && a.RewardId == reward.Id);
                var rlist = rewardRelationMapper.ToViewList(rdata);

                reward.ImgUrl = GetFileUrl(reward.ImgUrl);
                reward.RelationList = rlist.Where(a => a.RewardId == reward.Id).ToList();
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = reward;
            return res;
        }

        /// <summary>
        /// 是否领取礼包信息
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> IsReceive(QueryModel queryModel)
        {
            var res = new ResultModel();
            var exWhere = PredicateBuilder.New<RewardReceive>(a => a.UserId == _claimsAccessor.UserId);
            if (queryModel.queryType.HasValue)
            {
                exWhere.And(a => a.RewardType == queryModel.queryType.Value);
            }
            var rcount = await _rewardReceiveService.CountAsync(exWhere);
            res.data = rcount > 0;
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            return res;
        }




        #endregion
    }
}
