using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.ApiControllers
{
    /// <summary>
    /// 店铺
    /// </summary>
    public class ShopController : BaseController
    {
        private readonly IShopService _shopService;
        public ShopController(IClaimsAccessor claimsAccessor, ShopService shopService)
        {
            _shopService = shopService;
            _claimsAccessor = claimsAccessor;
        }

        /// <summary>
        /// 通过经纬度获取最近的店铺
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> LngLatGetShop(ShopQuery view) => await _shopService.LngLatGetShop(view);

        /// <summary>
        /// 店铺列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> ShopList(QueryModel view) => await _shopService.frontEndList(view);


        #region 商家入驻
        [Authorize(Roles = "api")]
        public async Task<ResultModel> SubmitShop(ShopView model) => await _shopService.ApplyForMerchantsToSettleIn(model, user);
        #endregion

        [HttpPost]
        public async Task<ResultModel> ShopDetail(QueryModel view) => await _shopService.frontEndDetails(view);

    }
}
