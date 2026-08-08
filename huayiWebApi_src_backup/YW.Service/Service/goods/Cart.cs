namespace YW.Service
{
    public partial interface ICartService : IBaseRepository<Cart>
    {

        Task<ResultModel> userCart(UserInfo user);

        /// <summary>
        /// 前端加入购物车
        /// </summary>
        /// <param name="view"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<ResultModel> frontEndOperation(CartView view, UserInfo user);

    }
    public partial class CartService : BaseRepository<Cart>, ICartService
    {
        private readonly CartMapper mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public CartService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

        public async Task<ResultModel> userCart(UserInfo user)
        {
            var res = new ResultModel();

            var p = new PageModel() { PageIndex = 1, PageSize = 9999 };
            var exWhere = PredicateBuilder.New<Cart>(a => a.status == 0 && a.userId == _claimsAccessor.UserId);

            var data = await base.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            var list = new List<CartView>();
            if (data.Count > 0)
            {
                list = mapper.ToViewList(data);
                //商品
                var gids = list.Select(a => a.goodsId).ToList();
                var glist = await GoodsDb.GetListAsync(a => a.status == 0 && SqlFunc.ContainsArray(gids, a.Id));
                //规格
                var skuids = list.Where(a => a.skuId > 0).Select(a => a.skuId).ToList();
                var skulist = await GoodSkuDb.GetListAsync(a => a.status == 0 && SqlFunc.ContainsArray(skuids, a.Id));

                foreach (var item in list)
                {
                    //商品
                    if (glist.Count(it => it.Id == item.goodsId) > 0)
                    {
                        var gInfo = glist.Find(it => it.Id == item.goodsId);
                        item.goodsUrl = WebFileHelper.GetUrl(gInfo.coverPicture);
                        item.goodsName = gInfo.name;
                        item.price = gInfo.price;
                        item.stock = gInfo.stock;
                    }
                    //规格
                    if (skulist.Count(it => it.Id == item.skuId) > 0)
                    {
                        var skuInfo = skulist.Find(it => it.Id == item.skuId);
                        item.skuName = skuInfo.name;
                        item.price = skuInfo.price;
                        item.stock = skuInfo.stock;
                        item.goodsUrl = WebFileHelper.GetUrl(skuInfo.url);
                    }
                    item.status = item.stock > 0 ? 0 : 1;
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "OK";
            res.data = new { total = list.Count, items = list };
            return res;
        }

        #region 编辑
        public async Task<ResultModel> frontEndOperation(CartView view, UserInfo user)
        {
            var res = new ResultModel();
            var isok = false;
            if (view.Id > 0)
            {
                //购物车操作
                var cart = await base.GetByIdAsync(view.Id);
                if (cart.status != 0 && cart.userId != _claimsAccessor.UserId)
                {
                    res.msg = "操作失败";
                    return res;
                }
                cart.num = view.num;
                cart.updateTime = DateTime.Now;
                isok = await base.UpdateAsync(cart);

            }
            else
            {
                var stock = 0;
                var model = mapper.ToModel(view);
                //商品加购操作
                var good = await GoodsDb.GetByIdAsync(model.goodsId);
                if (good.status != 0)
                {
                    res.msg = "商品已下架";
                    return res;
                }

                stock = good.stock;

                if (model.skuId == 0 && await GoodSkuDb.CountAsync(a => a.status == 0 && a.goodsId == good.Id) > 0)
                {
                    res.msg = "请选择商品规格";
                    return res;
                }
                if (model.skuId > 0)
                {
                    var sku = await GoodSkuDb.GetByIdAsync(model.skuId);
                    if (sku.status != 0)
                    {
                        res.msg = "此规格已下架";
                        return res;
                    }
                    if (sku.goodsId != good.Id)
                    {
                        res.msg = "此规格非商品规格";
                        return res;
                    }
                    stock = sku.stock;
                }
                model.userId = (int)_claimsAccessor.UserId;
                model.updateTime = DateTime.Now;
                if (await base.CountAsync(a => a.status == 0 && a.userId == model.userId && a.goodsId == model.goodsId && a.skuId == model.skuId) == 0)
                {
                    model.createTime = DateTime.Now;
                    if (model.num > stock)
                    {
                        res.msg = "商品加购数量大于库存数";
                        return res;
                    }

                    isok = await base.InsertAsync(model);

                }
                else
                {
                    //购物车中已存在
                    var cart = await base.GetSingleAsync(a => a.status == 0 && a.userId == model.userId && a.goodsId == model.goodsId && a.skuId == model.skuId);
                    if (cart.num + model.num > stock)
                    {
                        res.msg = "商品加购数量大于库存数";
                        return res;
                    }
                    model.Id = cart.Id;
                    model.num = cart.num + model.num;
                    model.createTime = cart.createTime;
                    isok = await base.UpdateAsync(model);
                }
            }

            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = isok ? "OK" : "NO";
            return res;
        }

        #endregion
    }
}
