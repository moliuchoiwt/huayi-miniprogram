using System.Threading;
using YW.Service.WeChat;

namespace YW.Service
{

    //商城订单

    public partial interface IGoodsOrderService : IBaseRepository<GoodsOrder>
    {


        /// <summary>
        /// 订单支付
        /// </summary>
        Task<ResultModel> OrderAgainPay(OrderPayView view, UserInfo user);
        /// <summary>
        /// 订单支付回调
        /// </summary>
        Task<ResultModel> OrderCallback(string payNo);
        /// <summary>
        /// 确认订单页面
        /// </summary>
        Task<ResultModel> CreateOrderView(OrderView view, UserInfo user);
        /// <summary>
        /// 详情
        /// </summary>
        Task<ResultModel> OrderDetail(QueryModel view);
        /// <summary>
        /// 发货
        /// </summary>
        Task<ResultModel> ConfirmDeliver(LogisticsView model);
        /// <summary>
        /// 申请退款
        /// </summary>
        Task<ResultModel> ApplyRefund(AfterSaleView view, UserInfo user);
        /// <summary>
        /// 确认退款
        /// </summary>
        Task<ResultModel> ConfirmRefund(AfterSaleView model);
        /// <summary>
        /// 确认收货
        /// </summary>
        Task<ResultModel> OrderReceiving(List<string> orderNoArr);
        /// <summary>
        /// 订单自提
        /// </summary>
        Task<ResultModel> OrderSelfMention(List<string> orderNoArr);
        /// <summary>
        /// 定时任务
        /// </summary>
        Task TimedTaskFun();

        /// <summary>
        /// 后台列表
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        Task<ResultModel> backEndList(GoodsOrderQuery view, SysUser admin);
        /// <summary>
        /// 前端列表
        /// </summary>        
        Task<ResultModel> frontEndList(QueryModel view, UserInfo user);

        /// <summary>
        /// 店铺订单列表        
        /// </summary>        
        Task<ResultModel> FrontEndStoreList(QueryModel view, UserInfo user);


        /// <summary>
        /// 线下支付
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ResultModel> OfflinePayment(GoodsOrderView model);

        /// <summary>
        /// 订单数量
        /// </summary>
        /// <returns></returns>
        Task<ResultModel> frontEndOrderCount(UserInfo user);



    }

    public partial class GoodsOrderService : BaseRepository<GoodsOrder>, IGoodsOrderService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly GoodsOrderMapper mapper = new();
        private readonly GoodsOrderDetailMapper goodsOrderDetailMapper = new();
        private readonly AfterSaleMapper afterSaleMapper = new();
        private readonly LogisticsMapper logisticsMapper = new();
        private readonly List<int> goodsTypeArr = new() { (int)GoodsTypeEnum.普通商品 };

        public GoodsOrderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        #region 列表
        public async Task<ResultModel> backEndList(GoodsOrderQuery view, SysUser admin)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<GoodsOrder>(a => a.status != 99);
            if (view.status.HasValue) exWhere.And(a => a.status == view.status.Value);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                int tId = 0;
                int.TryParse(view.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.orderNo.Contains(view.queryName)
               || a.payNo.Contains(view.queryName)
               || a.payMent.Contains(view.queryName) || a.remarks.Contains(view.queryName));

            }
            if (view.orderType.HasValue) exWhere.And(a => a.orderType == view.orderType.Value);
            if (view.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= view.startTime.Value);
            }
            if (view.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= view.endTime.Value);
            }
            if (admin == null)
            {
                res.msg = "登录失效，请重新登录";
                return res;
            }

            var data = await GoodsOrderDb.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            var list = new List<GoodsOrderView>();
            if (data != null && data.Count > 0)
            {
                list = mapper.ToViewList(data);
                //用户列表
                var uids = new List<int>();
                var userlist = new List<UserInfo>();
                if (data.Count(a => a.shareUserId > 0) > 0) uids.AddRange(data.Where(a => a.shareUserId > 0).Select(a => a.shareUserId).Distinct().ToList());
                if (uids.Count > 0)
                {
                    uids = uids.Distinct().ToList();
                    userlist = await UserInfoDb.GetListAsync(a => SqlFunc.ContainsArray(uids, a.Id));
                }

                var orNoArr = data.Select(a => a.orderNo).ToList();
                //订单详情
                var dList = new List<GoodsOrderDetailView>();
                var dData = await GoodsOrderDetailDb.GetListAsync(a => SqlFunc.ContainsArray(orNoArr, a.orderNo));
                if (dData != null && dData.Count > 0)
                {
                    //商品
                    var gIds = dData.Select(it => it.goodsId).Distinct().ToList();
                    var gList = await GoodsDb.GetListAsync(it => SqlFunc.ContainsArray(gIds, it.Id));
                    //规格
                    var sIds = dData.Select(it => it.skuId).Distinct().ToList();
                    var sList = await GoodSkuDb.GetListAsync(it => SqlFunc.ContainsArray(sIds, it.Id));

                    dList = goodsOrderDetailMapper.ToViewList(dData);
                    foreach (var item in dList)
                    {
                        //商品
                        if (gList.Count(it => it.Id == item.goodsId) > 0)
                        {
                            var gInfo = gList.Find(it => it.Id == item.goodsId);
                            item.goodsImg = WebFileHelper.GetUrl(gInfo.coverPicture);
                            item.goodsName = gInfo.name;
                        }
                        //规格
                        if (sList.Count(it => it.Id == item.skuId) > 0)
                        {
                            var sInfo = sList.Find(it => it.Id == item.skuId);
                            item.skuName = sInfo.name;
                        }
                    }
                }
                //发货列表
                var lList = await LogisticsDb.GetListAsync(a => SqlFunc.ContainsArray(orNoArr, a.orderNo));
                //退款列表
                var rList = new List<AfterSaleView>();
                var refundData = await AfterSaleDb.GetListAsync(a => a.status != 99 && a.type != 13 && SqlFunc.ContainsArray(orNoArr, a.orderNo));
                if (refundData != null && refundData.Count > 0)
                {
                    rList = afterSaleMapper.ToViewList(refundData);
                    foreach (var item in rList)
                    {
                        if (!string.IsNullOrWhiteSpace(item.url)) item.imgList = item.url.Split(",").ToList();
                    }
                }
                foreach (var item in list)
                {
                    item.loglist = lList.FindAll(a => a.orderNo == item.orderNo);
                    item.dlist = dList.FindAll(a => a.orderNo == item.orderNo);
                    item.rlist = rList.FindAll(a => a.orderNo == item.orderNo);
                }
            }

            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }

        public async Task<ResultModel> frontEndList(QueryModel view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<GoodsOrder>(a => a.status != 99 && a.userId == user.Id);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                int tId = 0;
                int.TryParse(view.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.orderNo.Contains(view.queryName)
               || a.payNo.Contains(view.queryName)
               || a.payMent.Contains(view.queryName) || a.remarks.Contains(view.queryName));

            }
            if (view.queryState.HasValue) exWhere.And(a => a.status == view.queryState.Value);
            if (view.queryStateArr != null && view.queryStateArr.Count > 0) exWhere.And(a => SqlFunc.ContainsArray(view.queryStateArr, a.status));
            if (view.queryType.HasValue) exWhere.And(a => a.orderType == view.queryType.Value);
            if (view.queryTypeArr != null && view.queryTypeArr.Count > 0) exWhere.And(a => SqlFunc.ContainsArray(view.queryTypeArr, a.orderType));
            if (view.startTime.HasValue) exWhere.And(a => a.createTime >= view.startTime.Value);
            if (view.endTime.HasValue) exWhere.And(a => a.createTime <= view.endTime.Value);

            var data = await GoodsOrderDb.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            var list = new List<GoodsOrderView>();
            if (data != null && data.Count > 0)
            {
                list = mapper.ToViewList(data);


                var orNoArr = data.Select(a => a.orderNo).ToList();
                //订单详情
                var dList = new List<GoodsOrderDetailView>();
                var dData = await GoodsOrderDetailDb.GetListAsync(a => SqlFunc.ContainsArray(orNoArr, a.orderNo));
                if (dData != null && dData.Count > 0)
                {
                    //商品
                    var gIds = dData.Select(it => it.goodsId).Distinct().ToList();
                    var gList = await GoodsDb.GetListAsync(it => SqlFunc.ContainsArray(gIds, it.Id));
                    //规格
                    var sIds = dData.Select(it => it.skuId).Distinct().ToList();
                    var sList = await GoodSkuDb.GetListAsync(it => SqlFunc.ContainsArray(sIds, it.Id));

                    dList = goodsOrderDetailMapper.ToViewList(dData);
                    foreach (var item in dList)
                    {
                        //商品
                        if (gList.Count(it => it.Id == item.goodsId) > 0)
                        {
                            var gInfo = gList.Find(it => it.Id == item.goodsId);
                            item.goodsImg = WebFileHelper.GetUrl(gInfo.coverPicture);
                            item.goodsName = gInfo.name;
                        }
                        //规格
                        if (sList.Count(it => it.Id == item.skuId) > 0)
                        {
                            var sInfo = sList.Find(it => it.Id == item.skuId);
                            item.skuName = sInfo.name;
                        }
                    }
                }

                //发货列表
                var lList = await LogisticsDb.GetListAsync(a => SqlFunc.ContainsArray(orNoArr, a.orderNo));
                //退款列表
                var rList = new List<AfterSaleView>();
                var refundData = await AfterSaleDb.GetListAsync(a => a.status != 99 && a.type != 13 && SqlFunc.ContainsArray(orNoArr, a.orderNo));
                if (refundData != null && refundData.Count > 0)
                {
                    rList = afterSaleMapper.ToViewList(refundData);
                    foreach (var item in rList)
                    {
                        if (!string.IsNullOrWhiteSpace(item.url)) item.imgList = WebFileHelper.GetListUrl(item.url);
                    }
                }
                foreach (var item in list)
                {
                    item.loglist = lList.FindAll(a => a.orderNo == item.orderNo);
                    item.dlist = dList.FindAll(a => a.orderNo == item.orderNo);
                    if (item.dlist != null) item.buyTotal = item.dlist.Sum(a => a.num);
                    item.rlist = rList.FindAll(a => a.orderNo == item.orderNo);
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list, mch_id = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_MchId };
            return res;
        }

        public async Task<ResultModel> FrontEndStoreList(QueryModel view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            if (ShopDb.Count(it => it.status == 0 && it.userId == user.Id) == 0)
            {
                res.msg = "无权限";
                return res;
            }
            var shop = await ShopDb.GetFirstAsync(it => it.userId == user.Id && it.status == 0);

            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<GoodsOrder>(a => a.status != 99 && a.shopId == shop.Id);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                int tId = 0;
                int.TryParse(view.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.orderNo.Contains(view.queryName)
               || a.payNo.Contains(view.queryName)
               || a.payMent.Contains(view.queryName) || a.remarks.Contains(view.queryName));

            }
            if (view.queryState.HasValue) exWhere.And(a => a.status == view.queryState.Value);
            if (view.queryStateArr != null && view.queryStateArr.Count > 0) exWhere.And(a => SqlFunc.ContainsArray(view.queryStateArr, a.status));
            if (view.queryType.HasValue) exWhere.And(a => a.orderType == view.queryType.Value);
            if (view.queryTypeArr != null && view.queryTypeArr.Count > 0) exWhere.And(a => SqlFunc.ContainsArray(view.queryTypeArr, a.orderType));
            if (view.startTime.HasValue) exWhere.And(a => a.createTime >= view.startTime.Value);
            if (view.endTime.HasValue) exWhere.And(a => a.createTime <= view.endTime.Value);

            var data = await GoodsOrderDb.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            var list = new List<GoodsOrderView>();
            if (data != null && data.Count > 0)
            {
                list = mapper.ToViewList(data);
                var orNoArr = data.Select(a => a.orderNo).ToList();

                //发货列表
                var lList = await LogisticsDb.GetListAsync(a => SqlFunc.ContainsArray(orNoArr, a.orderNo));
                //退款列表
                var rList = new List<AfterSaleView>();
                var refundData = await AfterSaleDb.GetListAsync(a => a.status != 99 && a.type != 13 && SqlFunc.ContainsArray(orNoArr, a.orderNo));
                if (refundData != null && refundData.Count > 0)
                {
                    rList = afterSaleMapper.ToViewList(refundData);
                    foreach (var item in rList)
                    {
                        if (!string.IsNullOrWhiteSpace(item.url)) item.imgList = WebFileHelper.GetListUrl(item.url);
                    }
                }
                foreach (var item in list)
                {
                    item.loglist = lList.FindAll(a => a.orderNo == item.orderNo);
                    item.rlist = rList.FindAll(a => a.orderNo == item.orderNo);
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list, mch_id = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_MchId };
            return res;
        }

        #endregion

        #region 商城确认订单页面

        public async Task<ResultModel> CreateOrderView(OrderView view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            try
            {

                var goodsTotal = 0;//商品数
                var glist = new List<OrderPageDto>();
                var addressModel = new UserAddress();
                var orderType = (int)OrderEnum.购物订单;


                if (view.cartIds != null && view.cartIds.Count > 0)
                {
                    var cartlist = CartDb.GetList(a => a.status == 0 && SqlFunc.ContainsArray(view.cartIds, a.Id));
                    var gids = cartlist.Select(a => a.goodsId).Distinct().ToList();
                    var goodslist = GoodsDb.GetList(a => SqlFunc.ContainsArray(gids, a.Id) && a.gType == (int)GoodsTypeEnum.普通商品);
                    var gClassList = new List<Class>();
                    if (goodslist != null && goodslist.Count > 0)
                    {
                        var cIds = goodslist.Select(it => it.classId).Distinct().ToList();
                        gClassList = await ClassDb.GetListAsync(it => SqlFunc.ContainsArray(cIds, it.Id));

                    }

                    var kids = cartlist.Where(a => a.skuId > 0).Select(a => a.skuId).Distinct().ToList();
                    var skulist = GoodSkuDb.GetList(a => SqlFunc.ContainsArray(kids, a.Id));
                    foreach (var item in cartlist)
                    {
                        var goods = goodslist.FirstOrDefault(a => a.Id == item.goodsId);
                        if (goods == null || goods.Id <= 0 || goods.status != 0)
                        {
                            res.msg = $"[{goods.name}]已下架";
                            return res;
                        }
                        var stock = goods.stock;

                        var gview = new OrderPageDto()
                        {
                            goodsId = goods.Id,
                            goodsName = goods.name,
                            goodsType = goods.gType,
                            goodsClassId = goods.classId,
                            num = item.num
                        };
                        gview.price = goods.price;
                        gview.goodsUrl = WebFileHelper.GetUrl(goods.pictureList);
                        if (item.skuId > 0)
                        {
                            var sku = skulist.FirstOrDefault(a => a.Id == item.skuId);
                            if (sku == null || sku.Id <= 0 || sku.status != 0 || sku.goodsId != goods.Id)
                            {
                                res.msg = $"[{goods.name}-{sku.name}]已下架";
                                return res;
                            }
                            stock = sku.stock;
                            gview.skuId = sku.Id;
                            gview.skuName = sku.name;

                            gview.price = sku.price;
                            if (!string.IsNullOrWhiteSpace(sku.url)) gview.goodsUrl = WebFileHelper.GetUrl(sku.url);
                        }
                        else
                        {
                            if (skulist.Count(a => a.goodsId == goods.Id && a.status == 0) > 0)
                            {
                                res.msg = $"请选择[{goods.name}]商品规格";
                                return res;
                            }
                        }
                        if (item.num > stock)
                        {
                            res.msg = $"{goods.name}购买数量超过库存数量";
                            return res;
                        }
                        gview.total = (gview.num * gview.price);
                        goodsTotal += gview.num;
                        glist.Add(gview);
                    }
                }
                else
                {
                    if (view.num <= 0)
                    {
                        res.msg = "购买数量不能小于1";
                        return res;
                    }
                    var ginfo = await GoodsDb.GetByIdAsync(view.goodsId);
                    if (ginfo == null || ginfo.Id <= 0 || ginfo.status != 0)
                    {
                        res.msg = $"商品已下架";
                        return res;
                    }

                    var stock = ginfo.stock;
                    var gview = new OrderPageDto()
                    {
                        goodsId = ginfo.Id,
                        goodsName = ginfo.name,
                        goodsType = ginfo.gType,
                        goodsClassId = ginfo.classId,
                        num = view.num
                    };
                    gview.price = ginfo.price;
                    gview.goodsUrl = WebFileHelper.GetUrl(ginfo.coverPicture);
                    if (view.skuId > 0)
                    {
                        var sku = GoodSkuDb.GetById(view.skuId);
                        if (sku == null || sku.Id <= 0 || sku.status != 0 || sku.goodsId != ginfo.Id)
                        {
                            res.msg = $"[{ginfo.name}-{sku.name}]已下架";
                            return res;
                        }
                        stock = sku.stock;
                        gview.skuId = sku.Id;
                        gview.skuName = sku.name;

                        gview.price = sku.price;
                        if (!string.IsNullOrWhiteSpace(sku.url)) gview.goodsUrl = WebFileHelper.GetUrl(sku.url);
                    }
                    else
                    {
                        if (GoodSkuDb.Count(a => a.goodsId == ginfo.Id && a.status == 0) > 0)
                        {
                            res.msg = $"请选择[{ginfo.name}]商品规格";
                            return res;

                        }
                    }
                    if (view.num > stock)
                    {
                        res.msg = $"{ginfo.name}购买数量超过库存数量";
                        return res;
                    }
                    gview.total = (gview.num * gview.price);
                    goodsTotal += gview.num;
                    glist.Add(gview);
                }

                var alist = UserAddressDb.GetList(a => a.status == 0 && a.userId == user.Id);
                if (alist != null && alist.Count > 0)
                {
                    addressModel = alist.OrderByDescending(a => a.isDefault).ThenByDescending(a => a.updateTime).FirstOrDefault();
                }



                //会员折扣            
                var orderPayAmount = glist.Sum(it => it.total);

                //运费
                var freight = 0;
                //使用积分
                var useIntegralNum = 0M;
                switch (orderType)
                {
                    default://默认生活区商品
                        {
                            //积分抵扣
                            if (user.integral > 0)
                            {
                                if (orderPayAmount > user.integral) useIntegralNum = user.integral;
                                else
                                {
                                    useIntegralNum = orderPayAmount - 0.01M;
                                }
                            }
                            //抵扣最低支付0.01                            
                            orderPayAmount -= useIntegralNum;

                        }
                        break;
                }


                res.msg = "";
                res.code = (int)ResultEnum.success;
                res.data = new
                {
                    addressModel,
                    user.integral,
                    user.amount,
                    goodsList = glist,
                    useIntegralNum,
                    goodsTotal,
                    orderPayAmount,
                    freight
                };
            }
            catch (Exception ex)
            {
                res.msg = ex.Message;
            }
            return res;
        }
        #endregion

        #region 创建订单        

        #region 购物订单
        private async Task<ResultModel> CreateGoodsOrder(OrderView view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }

            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var username = user.mobile;
            var orderlist = new List<GoodsOrder>();
            var dlist = new List<GoodsOrderDetailView>();
            var goodslist = new List<Goods>();
            var skuslist = new List<GoodSku>();
            var cartlist = new List<Cart>();
            var lList = new List<Logistics>();
            var orderType = (int)OrderEnum.购物订单;

            //购物车购买
            if (view.cartIds != null && view.cartIds.Count > 0)
            {
                cartlist = CartDb.GetList(a => a.status == 0 && SqlFunc.ContainsArray(view.cartIds, a.Id));
                var gIds = cartlist.Select(a => a.goodsId).Distinct().ToList();
                goodslist = GoodsDb.GetList(a => SqlFunc.ContainsArray(gIds, a.Id) && a.gType == (int)GoodsTypeEnum.普通商品);
                var skusIds = cartlist.Where(a => a.skuId > 0).Select(a => a.skuId).Distinct().ToList();
                skuslist = GoodSkuDb.GetList(a => SqlFunc.ContainsArray(skusIds, a.Id));
                string orderno = string.Empty;
                foreach (var item in cartlist)
                {
                    if (goodslist.Count(it => it.Id == item.goodsId) == 0)
                    {
                        res.msg = "商品不存在";
                        return res;
                    }
                    var goods = goodslist.FirstOrDefault(a => a.Id == item.goodsId);
                    if (goods.status != 0)
                    {
                        res.msg = $"商品[{goods.name}]已下架";
                        return res;
                    }
                    orderno = CommonHelper.GenerateUniqueText("g");
                    orderlist.Add(new GoodsOrder()
                    {
                        orderNo = orderno,
                        status = (int)OrderStateEnum.待支付,
                        orderType = orderType,
                        userId = user.Id,
                        remarks = view.remarks ?? ""
                    });


                    var stock = goods.stock;
                    var img = goods.coverPicture;
                    var detail = new GoodsOrderDetailView()
                    {
                        detailNo = CommonHelper.GenerateUniqueText("g"),
                        orderNo = orderno,
                        status = (int)OrderStateEnum.待支付,
                        orderType = orderType,
                        userId = user.Id,
                        goodsId = goods.Id,
                        goodsClassId = goods.classId,
                        price = goods.price,
                        num = item.num
                    };

                    if (item.skuId > 0 && skuslist.Count(it => it.goodsId == goods.Id && it.Id == item.skuId) > 0)
                    {
                        var sku = skuslist.FirstOrDefault(a => a.goodsId == goods.Id && a.Id == item.skuId);
                        if (sku.status != 0)
                        {
                            res.msg = $"[{goods.name}-{sku.name}]已下架";
                            return res;
                        }
                        stock = sku.stock;
                        detail.skuId = sku.Id;
                        detail.price = sku.price;
                    }
                    else
                    {
                        if (GoodSkuDb.Count(a => a.goodsId == goods.Id && a.status == 0) > 0)
                        {
                            res.msg = $"请选择[{goods.name}]商品规格";
                            return res;
                        }
                        detail.price = goods.price;
                    }
                    if (item.num > stock)
                    {
                        res.msg = $"{goods.name}购买数量超过库存数量";
                        return res;
                    }

                    detail.total = detail.price * detail.num;
                    detail.amount = detail.total;
                    dlist.Add(detail);

                    //购物车删除
                    item.status = 99;
                    item.updateTime = DateTime.Now;
                }
            }
            else
            {

                var goods = GoodsDb.GetById(view.goodsId);
                if (goods.Id <= 0 || goods.status != 0)
                {
                    res.msg = $"[{goods.name}]已下架";
                    return res;
                }
                goodslist.Add(goods);
                if (!goodsTypeArr.Contains(goods.gType))
                {
                    res.msg = $"商品类型有误";
                    return res;
                }
                var stock = goods.stock;
                var orderno = CommonHelper.GenerateUniqueText("g");
                orderlist.Add(new GoodsOrder()
                {
                    orderNo = orderno,
                    orderType = orderType,
                    status = (int)OrderStateEnum.待支付,
                    userId = user.Id,
                    remarks = view.remarks ?? ""
                });
                var img = goods.coverPicture;
                var detail = new GoodsOrderDetailView()
                {
                    detailNo = Common.CommonHelper.GenerateUniqueText("g"),
                    orderNo = orderno,
                    status = (int)OrderStateEnum.待支付,
                    orderType = orderType,
                    userId = user.Id,
                    goodsId = goods.Id,
                    goodsClassId = goods.classId,
                    price = goods.price,
                    num = view.num
                };

                if (view.skuId > 0 && GoodSkuDb.Count(it => it.Id == view.skuId && it.goodsId == goods.Id) > 0)
                {
                    var sku = GoodSkuDb.GetById(view.skuId);
                    if (sku.status != 0)
                    {
                        res.msg = $"[{goods.name}-{sku.name}]已下架";
                        return res;
                    }
                    skuslist.Add(sku);
                    stock = sku.stock;
                    detail.skuId = sku.Id;
                    detail.price = sku.price;
                }
                else
                {
                    if (GoodSkuDb.Count(a => a.goodsId == goods.Id && a.status == 0) > 0)
                    {
                        res.msg = $"请选择[{goods.name}]商品规格";
                        return res;
                    }
                    detail.price = goods.price;
                }
                if (view.num > stock)
                {
                    res.msg = $"{goods.name}购买数量超过库存数量";
                    return res;
                }
                detail.total = detail.price * detail.num;
                detail.amount = detail.total;
                dlist.Add(detail);
            }

            //校验发货地址           
            if (!view.addressId.HasValue || UserAddressDb.Count(it => it.Id == view.addressId.Value && it.status == 0 && it.userId == user.Id) == 0)
            {
                res.msg = $"用户地址信息错误";
                return res;
            }

            var address = await UserAddressDb.GetByIdAsync(view.addressId.Value);

            foreach (var item in orderlist)
            {
                var orderno = item.orderNo;
                var ilist = dlist.FindAll(a => a.orderNo == orderno);
                //商品
                var gIds = ilist.Select(it => it.goodsId).Distinct().ToList();
                var gList = goodslist.FindAll(it => gIds.Contains(it.Id));
                //规格
                var skus_Ids = ilist.Select(it => it.skuId).Distinct().ToList();
                var skus_List = skuslist.FindAll(it => skus_Ids.Contains(it.Id));

                string ginfo = string.Empty;
                foreach (var detail in ilist)
                {
                    if (gList.Count(it => it.Id == detail.goodsId) > 0)
                    {
                        var goods = gList.Find(it => it.Id == detail.goodsId);
                        var skusName = "";
                        if (skus_List.Count(it => it.Id == detail.skuId && it.goodsId == goods.Id) > 0)
                        {
                            var skusInfo = skus_List.Find(it => it.Id == detail.skuId && it.goodsId == goods.Id);
                            skusName = skusInfo.name;
                        }
                        ginfo += $"商品:{goods.name}{(detail.skuId > 0 ? $"[{skusName}]" : "")}*数量:{detail.num};*单价:{detail.price}￥;";
                    }
                }
                var dNoArr = string.Join(',', ilist.Select(a => a.detailNo).ToList());
                var lo = new Logistics()
                {

                    logisticsType = 0,
                    status = -1,
                    createTime = DateTime.Now,
                    updateTime = DateTime.Now,
                    orderNo = orderno,
                    detailNo = dNoArr,
                    goodsInfo = ginfo,
                    mobile = address.mobile,
                    userId = user.Id,
                    consignee = address.consignee,
                    address = $"{address.province ?? ""}{address.city ?? ""}{address.area ?? ""}{address.address}"
                };
                lList.Add(lo);
            }


            #region 合计金额          

            foreach (var order in orderlist)
            {
                var detaillist = dlist.FindAll(a => a.orderNo == order.orderNo);
                order.total = detaillist.Sum(a => a.total); //订单总金额                                                                                                                                
                order.amount = detaillist.Sum(a => a.amount);//应该要支付的金额                
                switch (order.orderType)
                {
                    default://生活区商品
                        {
                            //积分抵扣
                            if (user.integral > 0)
                            {
                                if (order.amount > user.integral) order.useIntegral = user.integral;
                                else
                                {
                                    order.useIntegral = order.amount - 0.01M;
                                }
                            }
                            order.integralPay = order.useIntegral;

                            //抵扣最低支付0.01                            
                            order.amount -= order.integralPay;
                        }
                        break;
                }
            }
            #endregion



            try
            {
                await db.BeginTranAsync();
                //处理购物车  清空购物车
                if (cartlist != null && cartlist.Count > 0) CartDb.UpdateRange(cartlist);
                //添加订单物流信息
                if (lList != null && lList.Count > 0) await LogisticsDb.InsertRangeAsync(lList);
                //处理订单列表
                await base.InsertAsync(orderlist);
                //处理订单详情列表                
                var order_dlist = goodsOrderDetailMapper.ToModelList(dlist);
                await GoodsOrderDetailDb.InsertRangeAsync(order_dlist);
                await db.CommitTranAsync();
                res.code = (int)ResultEnum.success;
                res.data = string.Join(",", orderlist.Select(a => a.orderNo).ToList());
                res.msg = $"创建订单成功";
            }
            catch (Exception ex)
            {
                await db.RollbackTranAsync();
                res.msg = ex.Message;
            }
            return res;
        }
        #endregion


        #endregion

        #region 订单详情

        public async Task<ResultModel> OrderDetail(QueryModel view)
        {

            var res = new ResultModel();
            if (!view.queryId.HasValue)
            {
                res.msg = "订单ID不能为空";
                return res;
            }
            var order = await GoodsOrderDb.GetByIdAsync(view.queryId.Value);
            if (order == null || order.status == 99)
            {
                res.msg = "订单不存在";
                return res;
            }
            var dlist = new List<GoodsOrderDetailView>();
            var dData = await GoodsOrderDetailDb.GetListAsync(a => a.orderNo == order.orderNo);
            if (dData != null && dData.Count > 0)
            {
                //商品
                var gIds = dData.Select(it => it.goodsId).Distinct().ToList();
                var gList = await GoodsDb.GetListAsync(it => SqlFunc.ContainsArray(gIds, it.Id));
                //规格
                var sIds = dData.Select(it => it.skuId).Distinct().ToList();
                var sList = await GoodSkuDb.GetListAsync(it => SqlFunc.ContainsArray(sIds, it.Id));

                dlist = goodsOrderDetailMapper.ToViewList(dData);
                foreach (var item in dlist)
                {
                    //商品
                    if (gList.Count(it => it.Id == item.goodsId) > 0)
                    {
                        var gInfo = gList.Find(it => it.Id == item.goodsId);
                        item.goodsImg = WebFileHelper.GetUrl(gInfo.coverPicture);
                        item.goodsName = gInfo.name;
                    }
                    //规格
                    if (sList.Count(it => it.Id == item.skuId) > 0)
                    {
                        var sInfo = sList.Find(it => it.Id == item.skuId);
                        item.skuName = sInfo.name;
                    }
                }
            }
            var llist = await LogisticsDb.GetListAsync(a => a.orderNo == order.orderNo);
            var rlist = await AfterSaleDb.GetListAsync(a => a.status != 99 && a.type != 13 && a.orderNo == order.orderNo);
            var goodsTotal = dlist.Sum(a => a.num);
            res.msg = "";
            res.code = (int)ResultEnum.success;
            res.data = new
            {
                order,
                dlist,
                llist,
                rlist,
                goodsTotal
            };
            return res;
        }
        #endregion

        #region 订单发起支付

        /// <summary>
        /// 订单发起支付
        /// </summary>
        public async Task<ResultModel> OrderAgainPay(OrderPayView view, UserInfo user)
        {
            var res = new ResultModel();

            var orderNoarr = view.orderNo.Split(',').ToList();
            var ordeList = await base.GetListAsync(a => a.status != 99 && a.userId == user.Id && a.status == (int)OrderStateEnum.待支付 && SqlFunc.ContainsArray(orderNoarr, a.orderNo));
            if (ordeList == null || ordeList.Count <= 0) { res.msg = "参数错误,订单不存在"; return res; }
            var payOrderNo = Common.CommonHelper.Timestamp();
            //实际支付金额 余额支付,积分数
            decimal sumAmount = ordeList.Sum(a => a.amount);
            //, sumBalance = ordeList.Sum(a => a.BalancePay), sumUseIntegral = ordeList.Sum(a => a.UseIntegral)
            //if (sumBalance > user.amount)
            //{
            //    res.msg = "用户余额不足,请先充值";
            //    return res;
            //}
            //if (sumUseIntegral > user.integral)
            //{
            //    res.msg = "用户积分不足";
            //    return res;
            //}

            ordeList.ForEach(item => item.payNo = payOrderNo);
            var orderIds = ordeList.Select(a => a.Id).ToList();
            var payMent = "微信支付";
            if (sumAmount <= 0)
            {
                view.payType = (int)PayEnum.无需支付;
                payMent = "无需支付";
            }
            if (!await base.UpdateAsync(a => new GoodsOrder { payNo = payOrderNo, payType = view.payType, payMent = payMent, updateTime = DateTime.Now }, a => SqlFunc.ContainsArray(orderIds, a.Id)))
            {
                res.msg = "支付失败";
                return res;
            }
            if (sumAmount <= 0)
            {
                res = await OrderCallback(payOrderNo);
                res.data = new { IsPay = false };
            }
            else
            {
                //if (view.payType != (int)PayEnum.支付宝APP)
                //{                
                var openId = user.wxAppletsOpenId;
                //生成微信支付
                string notifyUrl = PubConstant.Config.DomianName + "/api/Order/WxOrderNotifyUrl";
                int totalint = (int)(sumAmount * 100);
                var payObj = await WeChat.TenPayClient.TenPayByJsapi(openId, payOrderNo, payOrderNo, totalint, "", notifyUrl, 1);
                if (payObj != null)
                {
                    var payModel = payObj as WxJSAPIPay;
                    res.code = (int)ResultEnum.success;
                    res.msg = "微信生成支付订单成功";
                    res.data = new { IsPay = true, partnerid = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_MchId, wxData = payModel };
                }
                else
                {
                    res.msg = "微信生成支付订单失败";
                    res.data = new { IsPay = false, PayOrderNo = payOrderNo };
                }
                //}
                //else
                //{
                //    string notifyUrl = PubConstant.Config.DomianName + "/api/Order/AliPayNotifyUrl";
                //    var tradeobj = AliPay.AliPayClient.AlipayTrade(payOrderNo, sumAmount, notifyUrl);
                //    if (tradeobj != null)
                //    {
                //        res.code = (int)ResultEnum.success;
                //        res.msg = "支付宝生成支付订单成功";
                //        res.data = new { IsPay = true, partnerid = "", aliData = tradeobj };
                //    }
                //    else
                //    {
                //        res.msg = "支付宝生成支付订单失败";

                //        res.data = new { IsPay = false, PayOrderNo = payOrderNo };
                //    }
                //}
            }
            return res;
        }

        #endregion

        #region 订单回调

        public async Task<ResultModel> OrderCallback(string payNo)
        {
            var res = new ResultModel();
            if (string.IsNullOrWhiteSpace(payNo)) { res.msg = "支付单号参数不能为空"; return res; }
            var orderList = await base.GetListAsync(a => !a.isPay && a.status == (int)OrderStateEnum.待支付 && a.payNo == payNo);

            try
            {
                foreach (var item in orderList)
                {
                    switch (item.orderType)
                    {
                        case (int)OrderEnum.购物订单:
                            await GoodsOrderCallback(item);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("订单回调查询订单错误", ex);
            }
            res.msg = "SUCCESS";
            res.code = (int)ResultEnum.success;
            return res;

        }

        #region 商品订单回调
        private async Task GoodsOrderCallback(GoodsOrder order)
        {
            var dList = await GoodsOrderDetailDb.GetListAsync(a => a.orderNo == order.orderNo && a.status == (int)OrderStateEnum.待支付);
            var divideList = await GoodsOrderDivideDb.GetListAsync(it => it.orderNo == order.orderNo && it.status == 0);

            var gIds = dList.Select(a => a.goodsId).Distinct().ToList();
            var goodslist = await GoodsDb.GetListAsync(a => SqlFunc.ContainsArray(gIds, a.Id));
            var skuIds = dList.Select(a => a.skuId).Distinct().ToList();
            var skulist = await GoodSkuDb.GetListAsync(a => SqlFunc.ContainsArray(skuIds, a.Id));
            try
            {
                await db.BeginTranAsync();

                #region 变更订单状态
                order.isPay = true;
                order.updateTime = DateTime.Now;
                order.status = (int)OrderStateEnum.待发货;

                await base.UpdateAsync(order);
                //修改订单明细状态
                var detailList = dList.FindAll(it => it.orderNo == order.orderNo);
                await GoodsOrderDetailDb.UpdateAsync(it => new GoodsOrderDetail { updateTime = DateTime.Now, status = order.status }, it => it.orderNo == order.orderNo);
                //修改优惠券已使用状态
                if (order.couponId > 0)
                {
                    await UserCouponDb.UpdateAsync(it => new UserCoupon { status = 1, updateTime = DateTime.Now }, it => it.Id == order.couponId);
                    //查找其它未支付也用了这个优惠券的订单
                    var noPayOrderList = await GoodsOrderDb.GetListAsync(it => it.status == (int)OrderStateEnum.待支付 && it.userId == order.userId && it.Id != order.Id && it.couponId == order.couponId);
                    if (noPayOrderList != null && noPayOrderList.Count > 0)
                    {
                        foreach (var item in noPayOrderList)
                        {
                            item.couponId = 0;
                            item.amount += item.couponPay;
                            item.couponPay = 0;
                            item.updateTime = DateTime.Now;
                        }
                        await base.UpdateAsync(noPayOrderList);
                    }
                }
                #endregion

                //订单发货，变更订单物流记录
                await LogisticsDb.UpdateAsync(a => new Logistics { status = 0, updateTime = DateTime.Now }, a => a.status == -1 && a.orderNo == order.orderNo);

                #region 商品库存处理
                foreach (var ditem in detailList)
                {
                    if (goodslist.Count(it => it.Id == ditem.goodsId) > 0)
                    {
                        //商品的库存处理
                        var goods = goodslist.Find(a => a.Id == ditem.goodsId);
                        goods.sale += ditem.num;
                        goods.stock -= ditem.num;
                        if (goods.stock <= 0) goods.status = 1;
                        goods.updateTime = DateTime.Now;
                        await GoodsDb.UpdateAsync(a => new Goods { stock = goods.stock, sale = goods.sale, updateTime = goods.updateTime, status = goods.status }, a => a.Id == goods.Id);

                        //规格的库存处理
                        if (ditem.skuId > 0 && skulist.Count(it => it.Id == ditem.skuId) > 0)
                        {
                            var sku = skulist.Find(a => a.Id == ditem.skuId);
                            sku.sale += ditem.num;
                            sku.stock -= ditem.num;
                            if (sku.stock <= 0) sku.status = 1;
                            sku.updateTime = DateTime.Now;
                            await GoodSkuDb.UpdateAsync(a => new GoodSku { stock = sku.stock, sale = sku.sale, updateTime = goods.updateTime, status = sku.status }, a => a.Id == sku.Id);
                        }
                    }
                }
                #endregion
                //查询用户信息
                var user = await UserInfoDb.GetByIdAsync(order.userId);

                #region 根据分钱记录 添加冻结收入
                var order_divideList = divideList.FindAll(it => it.orderNo == order.orderNo);
                if (order_divideList != null && order_divideList.Count > 0)
                {
                    foreach (var divide in order_divideList)
                    {
                        var title = "直推分佣";
                        var remarks = $"直推分佣{Math.Round(divide.dRatio * 100, 2)}%,{divide.dAmount}元";
                        var sType = sourceTypeEnum.商品直推;
                        switch (divide.dType)
                        {
                            default:
                                break;
                        }
                        var wlog = GetWalletLog(walletUserTypeEnum.用户, walletTypeEnum.佣金, sType, divide.userId, title, divide.dAmount, order.orderNo, remarks);
                        divide.wLogId = await WalletLogDb.InsertReturnIdentityAsync(wlog);
                        divide.status = 1;//未分红
                    }
                    await GoodsOrderDivideDb.UpdateRangeAsync(order_divideList);
                }
                #endregion

                await db.CommitTranAsync();
            }
            catch (Exception ex)
            {
                await db.RollbackTranAsync();
                LogHelper.Error("订单回调错误", ex);
            }
        }
        #endregion



        #endregion

        #region 订单确认收货
        /// <summary>
        /// 订单确认收货
        /// </summary>
        public async Task<ResultModel> OrderReceiving(List<string> orderNoArr) => await OrderOver(orderNoArr);

        #endregion

        #region 订单用户自提
        /// <summary>
        /// 订单用户自提
        /// </summary>
        public async Task<ResultModel> OrderSelfMention(List<string> orderNoArr)
        {
            var res = await OrderOver(orderNoArr, (int)OrderStateEnum.待发货);
            return res;
        }

        #endregion

        #region 订单完成
        /// <summary>
        /// 订单完成
        /// </summary>
        private async Task<ResultModel> OrderOver(List<string> orderNoArr, int orderstate = (int)OrderStateEnum.待收货)
        {

            var res = new ResultModel();
            if (orderNoArr == null || orderNoArr.Count <= 0) { res.msg = "订单单号参数不能为空"; return res; }
            var orderList = await base.GetListAsync(a => a.isPay && SqlFunc.ContainsArray(orderNoArr, a.orderNo) && a.status == orderstate);
            var dList = new List<GoodsOrderDetail>();
            var goodslist = new List<Goods>();
            var skulist = new List<GoodSku>();
            try
            {

                await db.BeginTranAsync();
                if (orderList != null && orderList.Count > 0)
                {
                    var noArr = orderList.Select(a => a.orderNo).Distinct().ToArray();
                    dList = GoodsOrderDetailDb.GetList(a => SqlFunc.ContainsArray(noArr, a.orderNo) && a.status == orderstate);

                    var gIds = dList.Select(a => a.goodsId).Distinct().ToList();
                    goodslist = GoodsDb.GetList(a => SqlFunc.ContainsArray(gIds, a.Id));
                    var skuIds = dList.Select(a => a.skuId).Distinct().ToList();
                    skulist = GoodSkuDb.GetList(a => SqlFunc.ContainsArray(skuIds, a.Id));


                    if (orderstate == (int)OrderStateEnum.待发货)
                    {
                        //变更发货记录为已收货
                        LogisticsDb.Update(a => new Logistics { status = 2, updateTime = DateTime.Now, isUp = true }, a => a.status == 0 && SqlFunc.ContainsArray(noArr, a.orderNo));
                    }
                }


                foreach (var item in orderList)
                {
                    #region 调整订单状态
                    item.updateTime = DateTime.Now;
                    item.status = (int)OrderStateEnum.待评论;
                    await base.UpdateAsync(item);

                    //变更发货记录为已收货
                    LogisticsDb.Update(a => new Logistics { status = 2, updateTime = DateTime.Now }, a => a.orderNo == item.orderNo);
                    //变更订单详情状态
                    await GoodsOrderDetailDb.UpdateAsync(it => new GoodsOrderDetail { updateTime = DateTime.Now, status = (int)OrderStateEnum.待评论 }, it => it.orderNo == item.orderNo);
                    #endregion

                    //var user = UserInfoDb.GetById(item.userId);                                      
                }
                await db.CommitTranAsync();
                res.msg = "SUCCESS";
                res.code = (int)ResultEnum.success;
            }
            catch (Exception ex)
            {
                await db.RollbackTranAsync();
                LogHelper.Error("订单完成错误", ex);
                res.msg = ex.Message;
            }

            return res;
        }

        /// <summary>
        /// 往上查找第几级的父级
        /// </summary>
        /// <param name="allUser"></param>
        /// <param name="userId"></param>
        /// <param name="maxLevel"></param>
        /// <returns></returns>
        private UserInfo getParentUser(List<UserInfo> allUser, int userId, int maxLevel = 10)
        {
            var result = new UserInfo();
            int currentLevel = 0;
            int currentId = userId;
            while (currentLevel < maxLevel && currentId > 0)
            {
                result = allUser.FirstOrDefault(it => it.Id == currentId);

                if (result == null) break;

                currentId = result.parentId;
                currentLevel++;
            }
            return result;
        }

        /// <summary>
        /// 往上查找父级列表
        /// </summary>
        /// <param name="allUser"></param>
        /// <param name="userId"></param>
        /// <param name="maxLevel"></param>
        /// <returns></returns>
        private List<UserInfo> getParentUserList(List<UserInfo> allUser, int userId, int maxLevel = 10)
        {
            var result = new List<UserInfo>();
            int currentLevel = 0;
            int currentId = userId;

            while (currentLevel < maxLevel && currentId > 0)
            {
                var parent = allUser.FirstOrDefault(it => it.Id == currentId);

                if (parent == null) break;

                result.Add(parent);
                currentId = parent.parentId;
                currentLevel++;
            }
            return result;
        }

        #endregion


        #region 确认发货
        public async Task<ResultModel> ConfirmDeliver(LogisticsView model)
        {
            var res = new ResultModel();
            if (LogisticsDb.Count(it => it.Id == model.Id && it.status == 0) == 0)
            {
                res.msg = "订单不能发货";
                return res;
            }
            var lmodel = await LogisticsDb.GetByIdAsync(model.Id);
            if (model.logisticsType == 0)
            {
                if (ExpressDb.Count(it => it.Id == model.expressId && it.status == 0) == 0)
                {
                    res.msg = "快递信息不能为空";
                    return res;
                }
                var emodel = await ExpressDb.GetByIdAsync(model.expressId);
                model.expressName = emodel.name;
                model.expressCode = emodel.code;
                model.expressUrl = emodel.url;
            }
            if (GoodsOrderDb.Count(it => it.orderNo == lmodel.orderNo) == 0)
            {
                res.msg = "订单信息不存在";
                return res;
            }
            var order = await base.GetSingleAsync(it => it.orderNo == model.orderNo);
            if (UserInfoDb.Count(it => it.Id == order.userId) == 0)
            {
                res.msg = "用户信息不存在";
                return res;
            }
            var user = await UserInfoDb.GetByIdAsync(order.userId);
            var info = logisticsMapper.ToModel(model);
            info.orderNo = lmodel.orderNo;
            info.detailNo = lmodel.detailNo;
            info.goodsInfo = lmodel.goodsInfo;
            info.status = 1;
            info.updateTime = DateTime.Now;

            if (order.payType == (int)PayEnum.微信JSAPI)
            {
                if (model.logisticsType == 0)//物流发货
                {
                    var fahuo = OpenClient.OrderUploadShippingInfo(1, order.payNo, new Senparc.Weixin.WxOpen.AdvancedAPIs.Sec.ShippingListModel { item_desc = lmodel.goodsInfo, express_company = model.expressCode, tracking_no = model.logisticsNo }, user.wxAppletsOpenId);
                    if (fahuo.errcode != 0)
                    {
                        res.msg = fahuo.errmsg;
                        return res;
                    }
                }
                else if (model.logisticsType == 1)//线下自提
                {
                    var fahuo = OpenClient.OrderUploadShippingInfo(4, order.payNo, new Senparc.Weixin.WxOpen.AdvancedAPIs.Sec.ShippingListModel { item_desc = lmodel.goodsInfo }, user.wxAppletsOpenId);
                    if (fahuo.errcode != 0)
                    {
                        res.msg = fahuo.errmsg;
                        return res;
                    }
                }
            }
            try
            {
                await db.Ado.BeginTranAsync();

                await LogisticsDb.UpdateAsync(info);
                //更新订单状态 
                if (LogisticsDb.Count(it => it.orderNo == info.orderNo && it.status == 0) == 0)
                {
                    await GoodsOrderDb.UpdateAsync(a => new GoodsOrder { status = (int)OrderStateEnum.待收货, updateTime = DateTime.Now }, it => it.status == (int)OrderStateEnum.待发货 && it.orderNo == lmodel.orderNo);
                    await GoodsOrderDetailDb.UpdateAsync(a => new GoodsOrderDetail { status = (int)OrderStateEnum.待收货, updateTime = DateTime.Now }, it => it.status == (int)OrderStateEnum.待发货 && it.orderNo == lmodel.orderNo);
                }
                await db.Ado.CommitTranAsync();
                res.code = (int)ResultEnum.success;
                res.msg = "发货成功";

            }
            catch (Exception ex)
            {
                await db.Ado.RollbackTranAsync();
                res.msg = ex.Message;
            }
            return res;
        }
        #endregion

        #region 申请退款
        public async Task<ResultModel> ApplyRefund(AfterSaleView view, UserInfo user)
        {
            var res = new ResultModel();
            //var detail = await GoodsOrderDetailDb.GetFirstAsync(a => view.detailNo == a.detailNo && a.orderType == orderType && a.userId == user.Id);
            //if (detail == null || detail.Id <= 0)
            //{
            //    res.msg = "订单不存在,请稍后再试";
            //    return res;
            //}
            //if ((detail.status != (int)OrderStateEnum.待发货 && detail.status != (int)OrderStateEnum.待收货 && detail.status != (int)OrderStateEnum.待核销))
            //{
            //    res.msg = "订单状态不支持申请退款,请稍后再试";
            //    return res;
            //}
            if (view == null) { res.msg = "参数错误"; return res; }
            if (!view.orderId.HasValue || view.orderId.Value <= 0) { res.msg = "订单id 需要大于0"; return res; }
            if (GoodsOrderDb.Count(it => it.Id == view.orderId && it.userId == user.Id) == 0)
            {
                res.msg = "订单不存在";
                return res;
            }
            var order = await GoodsOrderDb.GetByIdAsync(view.orderId);
            if (order == null || (order.status != (int)OrderStateEnum.待发货 && order.status != (int)OrderStateEnum.待核销 && order.status != (int)OrderStateEnum.待售后))
            {
                res.msg = "订单状态不支持申请退款,请稍后再试";
                return res;
            }
            try
            {
                var rModel = new AfterSale();
                rModel.type = 0;
                rModel.orderNo = order.orderNo;
                rModel.payNo = order.payNo;
                rModel.url = view.url;
                rModel.note = view.note;
                rModel.remark = view.remark;

                rModel.userId = user.Id;
                rModel.total = order.total;
                rModel.useIntegral = order.useIntegral;
                rModel.balancePay = order.balancePay;
                rModel.cashPay = order.cashPay;
                rModel.amount = order.amount;
                rModel.status = 0;
                rModel.createTime = DateTime.Now;
                rModel.updateTime = DateTime.Now;

                db.Ado.BeginTran();
                await AfterSaleDb.InsertAsync(rModel);
                //更新订单状态 
                GoodsOrderDb.Update(a => new GoodsOrder { status = (int)OrderStateEnum.待售后, updateTime = DateTime.Now }, it => it.status != 99 && it.orderNo == order.orderNo);
                GoodsOrderDetailDb.Update(a => new GoodsOrderDetail { status = (int)OrderStateEnum.待售后, updateTime = DateTime.Now }, it => it.status != 99 && it.orderNo == order.orderNo);

                db.Ado.CommitTran();
                res.code = (int)ResultEnum.success;
                res.msg = "申请退款成功";

            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                res.msg = ex.Message;
            }
            return res;
        }
        #endregion

        #region 确认退款
        public async Task<ResultModel> ConfirmRefund(AfterSaleView view)
        {
            var res = new ResultModel();
            if (view.status != 1 && view.status != 2)
            {
                res.msg = "订单确认状态错误,请稍后再试";
                return res;
            }
            var lmodel = await AfterSaleDb.GetByIdAsync(view.Id);
            if (lmodel == null || lmodel.status != 0)
            {
                res.msg = "订单不能确认退款,请稍后再试";
                return res;
            }
            var order = await GoodsOrderDb.GetFirstAsync(a => a.orderNo == lmodel.orderNo);
            if (order == null || order.status != (int)OrderStateEnum.待售后)
            {
                res.msg = "订单非售后订单,请稍后再试";
                return res;
            }
            //退款商品
            var goods = new Goods();
            if (lmodel.goodsId > 0 && GoodsDb.Count(it => it.Id == lmodel.goodsId) == 0)
            {
                res.msg = "退款商品不存在";
                return res;
            }
            goods = await GoodsDb.GetByIdAsync(lmodel.goodsId);

            //退款规格
            var skus = new GoodSku();
            if (lmodel.skuId > 0 && GoodSkuDb.Count(it => it.Id == lmodel.skuId) == 0)
            {
                res.msg = "退款商品规格不存在";
                return res;
            }
            skus = await GoodSkuDb.GetByIdAsync(lmodel.skuId);


            try
            {
                db.Ado.BeginTran();

                lmodel.auditIntro = view.auditIntro ?? "";
                lmodel.status = view.status;
                lmodel.updateTime = DateTime.Now;
                await AfterSaleDb.UpdateAsync(lmodel);

                //原订单状态
                int state = (int)OrderStateEnum.待发货;
                if (lmodel.status == 1)
                {
                    state = (int)OrderStateEnum.已售后;
                }
                else
                {
                    state = (int)OrderStateEnum.待收货;
                }
                //更新总订单状态 
                GoodsOrderDb.Update(a => new GoodsOrder { status = state, updateTime = DateTime.Now }, it => it.status == (int)OrderStateEnum.待售后 && it.orderNo == lmodel.orderNo);
                if (lmodel.status == 1)
                {
                    string payNo = order.payNo;
                    GoodsOrderDetailDb.Update(a => new GoodsOrderDetail { status = (int)OrderStateEnum.已售后, updateTime = DateTime.Now }, it => it.status == (int)OrderStateEnum.待售后 && it.detailNo == lmodel.detailNo);

                    if (order.payType != (int)PayEnum.线下支付)
                    {
                        #region 用户退款流水记录
                        //var user = UserInfoDb.GetById(order.userId);
                        //if (lmodel.balancePay > 0)
                        //{
                        //    var wtitle = $"订单退款余额:{lmodel.balancePay}￥;商品:{goods.name};订单号:{lmodel.orderNo};";
                        //    var wlog = GetWalletLog(walletUserTypeEnum.用户, walletTypeEnum.余额, sourceTypeEnum.订单退款 , lmodel.userId, order.shopId, lmodel.orderNo,  "", "商品订单退款余额", user.amount, lmodel.balancePay, (user.amount + lmodel.balancePay),  wtitle);
                        //    WalletLogDb.Insert(wlog);
                        //    user.amount = user.amount + lmodel.balancePay;
                        //    UserInfoDb.Update(a => new UserInfo { amount = user.amount }, a => a.Id == user.Id);
                        //}


                        //if (lmodel.useIntegral > 0)
                        //{
                        //    var wtitle = $"订单退款积分:{lmodel.useIntegral}￥;商品:{goods.name};订单号:{lmodel.orderNo};";
                        //    var wlog = GetWalletLog(order.shopId, lmodel.orderNo, lmodel.userId, "", "商品订单退款积分", user.integral, lmodel.useIntegral, (user.integral + lmodel.useIntegral), walletTypeEnum.积分, sourceTypeEnum.订单退款, walletUserTypeEnum.用户, wtitle);
                        //    WalletLogDb.Insert(wlog);
                        //    user.integral = user.integral + lmodel.useIntegral;
                        //    UserInfoDb.Update(a => new UserInfo { integral = user.integral }, a => a.Id == user.Id);
                        //}

                        #endregion

                        #region 微信退款

                        if (lmodel.amount > 0)
                        {

                            int total = (int)(lmodel.amount * 100);
                            int ordertotal = (int)(order.amount * 100);
                            string notifyUrl = PubConstant.Config.DomianName + "/api/Order/WxRefundNotifyUrl";
                            var wxResult = WeChat.TenPayClient.Refund(_serviceProvider, payNo, total, ordertotal, 1, notifyUrl);
                            if (wxResult != "成功")
                            {
                                db.Ado.RollbackTran();
                                res.msg = $"微信退款失败:{wxResult}";
                                return res;
                            }
                        }
                        #endregion
                    }
                }
                else
                {
                    GoodsOrderDetailDb.Update(a => new GoodsOrderDetail { status = state, updateTime = DateTime.Now }, it => it.status == (int)OrderStateEnum.待售后 && it.detailNo == lmodel.detailNo);
                }
                db.Ado.CommitTran();
                res.code = (int)ResultEnum.success;
                res.msg = "退款成功";

            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                res.msg = ex.Message;
            }
            return res;
        }
        #endregion

        #region 线下支付
        public async Task<ResultModel> OfflinePayment(GoodsOrderView model)
        {
            var res = new ResultModel();
            if (GoodsOrderDb.Count(it => it.Id == model.Id && !it.isPay) == 0)
            {
                res.msg = "订单不存在或已支付";
                return res;
            }

            var order = await GoodsOrderDb.GetByIdAsync(model.Id);
            var payOrderNo = Common.CommonHelper.Timestamp();

            if (await GoodsOrderDb.UpdateAsync(a => new GoodsOrder { payNo = payOrderNo, payType = (int)PayEnum.线下支付, payMent = "线下支付", updateTime = DateTime.Now }, a => a.Id == order.Id))
            {
                res = await OrderCallback(payOrderNo);
            }
            else
            {
                res.msg = "支付失败";
                return res;
            }
            return res;
        }

        #endregion

        #region 订单数量
        public async Task<ResultModel> frontEndOrderCount(UserInfo user)
        {
            var res = new ResultModel();
            var exWhere = PredicateBuilder.New<GoodsOrder>(a => a.status != 99 && a.userId == user.Id);
            var num1 = await db.Queryable<GoodsOrder>().Where(exWhere).CountAsync(a => a.status == (int)OrderStateEnum.待支付);
            var num2 = await db.Queryable<GoodsOrder>().Where(exWhere).CountAsync(a => a.status == (int)OrderStateEnum.待发货);
            var num3 = await db.Queryable<GoodsOrder>().Where(exWhere).CountAsync(a => a.status == (int)OrderStateEnum.待收货);
            var stateArr4 = new List<int> { (int)OrderStateEnum.待评论 };
            var num4 = await db.Queryable<GoodsOrder>().Where(exWhere).CountAsync(a => SqlFunc.ContainsArray(stateArr4, a.status));
            var stateArr5 = new List<int> { (int)OrderStateEnum.已完成, (int)OrderStateEnum.待售后, (int)OrderStateEnum.已售后 };
            var num5 = await db.Queryable<GoodsOrder>().Where(exWhere).CountAsync(a => SqlFunc.ContainsArray(stateArr5, a.status));
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { num1, num2, num3, num4, num5 };
            return res;
        }
        #endregion


    }

    public partial class GoodsOrderService
    {
        #region 定时任务
        private readonly SemaphoreSlim _asyncLock = new(1, 1);
        public async Task TimedTaskFun()
        {
            await _asyncLock.WaitAsync(); // 异步等待锁

            try
            {

                var date_30min = DateTime.Now.AddMinutes(-30);
                //商城订单30分钟未支付自动取消
                if (GoodsOrderDb.Count(a => !a.isPay && a.createTime < date_30min) > 0)
                {
                    await GoodsOrderDetailDb.UpdateAsync(a => new GoodsOrderDetail { status = 99, updateTime = DateTime.Now }, a => a.status == 0 && a.createTime < date_30min);
                    await GoodsOrderDb.UpdateAsync(a => new GoodsOrder { status = 99, updateTime = DateTime.Now }, a => !a.isPay && a.createTime < date_30min);
                }


                #region  商城订单10天未收货，自动收货

                var date_10day = DateTime.Now.AddDays(-10);
                if (GoodsOrderDb.Count(a => a.status == (int)OrderStateEnum.待收货 && a.updateTime < date_10day) > 0)
                {
                    try
                    {

                        var list = GoodsOrderDb.GetList(a => a.status == (int)OrderStateEnum.待收货 && a.updateTime < date_10day);
                        var noArr = list.Select(s => s.orderNo).ToList();
                        var res = OrderReceiving(noArr).Result;
                        if (res.code != 200)
                        {
                            Common.LogHelper.Error("商城订单10天未收货，自动收货处理错误:" + res.msg);
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.LogHelper.Error("商城订单10天未收货，自动收货处理错误", ex);
                    }
                }
                #endregion


            }
            catch (Exception ex)
            {
                LogHelper.Error("订单定时任务出错：", ex);
            }
            finally
            {
                _asyncLock.Release(); // 释放锁
            }
        }


        #endregion

    }
}