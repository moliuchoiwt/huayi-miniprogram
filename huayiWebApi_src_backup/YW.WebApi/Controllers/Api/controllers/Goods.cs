using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.ApiControllers
{
    /// <summary>
    /// 商品
    /// </summary>
    public class GoodsController : BaseController
    {
        private readonly IGoodsService _goodsService;
        private readonly ICartService _cartService;
        private readonly IBrowsesService _browsesService;
        private readonly ICollectionRecordService _collectionRecordService;

        public GoodsController(IClaimsAccessor claimsAccessor,
            GoodsService goodsService,
            CartService cartService,
            BrowsesService browsesService,
            CollectionRecordService collectionRecordService)
        {
            _claimsAccessor = claimsAccessor;
            _goodsService = goodsService;
            _cartService = cartService;
            _browsesService = browsesService;
            _collectionRecordService = collectionRecordService;
        }

        #region 商品

        /// <summary>
        /// 店铺商品列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> GoodsList(GoodsQuery queryModel) => await _goodsService.frontEndList(queryModel, user);

        /// <summary>
        /// 商品详情
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> GoodsInfo(QueryModel model) => await _goodsService.frontEndInfo(model, user);
        #endregion

        #region 购物车

        /// <summary>
        /// 购物车列表
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "api")]
        [HttpPost]
        public async Task<ResultModel> CartList() => await _cartService.userCart(user);

        /// <summary>
        /// 加入购物车操作
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize(Roles = "api")]
        [HttpPost]
        public async Task<ResultModel> CartOperation(CartView view) => await _cartService.frontEndOperation(view, user);


        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "api")]
        [HttpPost]
        public async Task<ResultModel> DelCart(DelModel del)
        {
            var res = new ResultModel();

            if (del.ids == null || del.ids.Length <= 0)
            {
                res.msg = "购物车ID不能为空";
                return res;
            }
            var isok = await _cartService.UpdateAsync(it => new Cart { status = 99 }, it => it.userId == _claimsAccessor.UserId && SqlFunc.ContainsArray(del.ids, it.Id));
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");

            return res;

        }
        #endregion

        #region 浏览记录     

        /// <summary>
        /// 操作浏览记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> OperationBrowses(BrowsesView view) => await _browsesService.frontEndOperation(view, user);

        #endregion

        #region 商品收藏        
        [HttpPost]
        public async Task<ResultModel> OperationCollection(CollectionRecordView model) => await _collectionRecordService.frontEndOperation(model, user);
        #endregion      
    }
}
