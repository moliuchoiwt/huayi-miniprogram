namespace YW.Service
{
    public interface IGoodsService : IBaseRepository<Goods>
    {
        /// <summary>
        /// 新增商品入库
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ResultModel> Operation(GoodsView model);

        /// <summary>
        /// 后台列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<ResultModel> backEndList(GoodsQuery queryModel);

        /// <summary>
        /// 前端列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<ResultModel> frontEndList(GoodsQuery queryModel, UserInfo user);

        /// <summary>
        /// 前端详情
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ResultModel> frontEndInfo(QueryModel model, UserInfo user);
    }

    //商城商品

    public class GoodsService : BaseRepository<Goods>, IGoodsService
    {
        private readonly GoodsMapper mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public GoodsService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }


        #region 列表
        //后台列表
        public async Task<ResultModel> backEndList(GoodsQuery queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<Goods>(a => a.status != 99);

            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                int.TryParse(queryModel.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.name.Contains(queryModel.queryName)
               || a.intro.Contains(queryModel.queryName)
               || a.contents.Contains(queryModel.queryName)
               || a.parameter.Contains(queryModel.queryName));
            }
            if (queryModel.status.HasValue)
            {
                exWhere.And(a => a.status == queryModel.status.Value);
            }
            else exWhere.And(it => it.status == 0);
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.updateTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.updateTime <= queryModel.endTime.Value);
            }
            if (queryModel.gType.HasValue) exWhere.And(it => it.gType == queryModel.gType.Value);
            else exWhere.And(it => it.gType == 0);
            if (queryModel.isIndex.HasValue) exWhere.And(it => it.isIndex == queryModel.isIndex.Value);

            if (queryModel.classId.HasValue) exWhere.And(it => it.classId == queryModel.classId.Value);
            System.Linq.Expressions.Expression<Func<Goods, object>> order = it => new { it.sort, it.createTime, it.Id };

            if (queryModel.orderBy.HasValue)
            {
                switch (queryModel.orderBy.Value)
                {
                    case 0:
                        order = it => new { it.sale, it.sort };
                        break;
                    default:
                        break;
                }
            }

            var data = await GoodsDb.GetPageListAsync(exWhere, p, order, OrderByType.Desc);
            var list = new List<GoodsView>();
            if (data.Count > 0)
            {
                list = mapper.ToViewList(data);
                var ids = data.Select(a => a.Id).ToList();
                var skulist = await GoodSkuDb.GetListAsync(a => SqlFunc.ContainsArray(ids, a.goodsId) && a.status != 99);
                skulist = skulist.OrderByDescending(a => a.sort).ThenByDescending(a => a.stock).ToList();
                if (skulist != null && skulist.Count > 0)
                {
                    foreach (var item in skulist)
                    {
                        item.url = WebFileHelper.GetUrl(item.url);
                    }
                }


                foreach (var item in list)
                {
                    item.coverPicture = WebFileHelper.GetUrl(item.coverPicture);
                    if (!string.IsNullOrWhiteSpace(item.pictureList)) item.imgList = WebFileHelper.GetListUrl(item.pictureList);
                    item.contents = WebFileHelper.getContent(item.contents);
                    item.skuList = skulist.FindAll(a => a.goodsId == item.Id);
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }

        //前端列表
        public async Task<ResultModel> frontEndList(GoodsQuery queryModel, UserInfo user)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<Goods>(a => a.status != 99);

            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                int.TryParse(queryModel.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.name.Contains(queryModel.queryName)
               || a.intro.Contains(queryModel.queryName)
               || a.contents.Contains(queryModel.queryName)
               || a.parameter.Contains(queryModel.queryName));
            }
            if (queryModel.queryState.HasValue)
            {
                exWhere.And(a => a.status == queryModel.queryState.Value);
            }
            else exWhere.And(it => it.status == 0);
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.updateTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.updateTime <= queryModel.endTime.Value);
            }
            if (queryModel.gType.HasValue) exWhere.And(it => it.gType == queryModel.gType.Value);
            else exWhere.And(it => it.gType == 0);
            if (queryModel.isIndex.HasValue) exWhere.And(it => it.isIndex == queryModel.isIndex.Value);

            if (queryModel.classId.HasValue) exWhere.And(it => it.classId == queryModel.classId.Value);
            System.Linq.Expressions.Expression<Func<Goods, object>> order = it => new { it.sort, it.createTime, it.Id };

            if (queryModel.isCollection.HasValue && queryModel.isCollection.Value)//用户的收藏
            {
                if (user == null)
                {
                    res.msg = "登录过期，请重新登录";
                    return res;
                }
                var cgIds = db.Queryable<CollectionRecord>().Where(it => it.userId == user.Id && it.cType == 0 && it.status == 1).Select(it => it.cId).ToList();
                exWhere.And(it => SqlFunc.ContainsArray(cgIds, it.Id));

                if (queryModel.orderBy.HasValue)
                {
                    switch (queryModel.orderBy.Value)
                    {
                        case 1:
                            order = it => new { it.sale, it.sort };
                            break;
                        case 2:
                            order = it => new { it.createTime, it.sort };
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                if (queryModel.orderBy.HasValue)
                {
                    switch (queryModel.orderBy.Value)
                    {
                        case 0: //销量排行
                            order = it => new { it.sale, it.sort };
                            break;
                        case 1://最近上新
                            order = it => new { it.createTime, it.sort };
                            break;
                        default:
                            break;
                    }
                }
            }

            var data = await GoodsDb.GetPageListAsync(exWhere, p, order, OrderByType.Desc);
            var list = new List<GoodsView>();
            if (data != null && data.Count > 0)
            {
                list = mapper.ToViewList(data);
                var ids = data.Select(a => a.Id).ToList();
                var skulist = await GoodSkuDb.GetListAsync(a => SqlFunc.ContainsArray(ids, a.goodsId));
                skulist = skulist.OrderByDescending(a => a.sort).ThenByDescending(a => a.stock).ToList();
                //分类
                var cIds = data.Select(it => it.classId).Distinct().ToList();
                var cList = await ClassDb.GetListAsync(it => SqlFunc.ContainsArray(cIds, it.Id));


                foreach (var item in list)
                {
                    item.coverPicture = WebFileHelper.GetUrl(item.coverPicture);
                    item.pictureList = "";
                    item.contents = "";
                    item.skuList = skulist.FindAll(a => a.goodsId == item.Id);
                    //分类
                    if (cList.Count(it => it.Id == item.classId) > 0)
                    {
                        var cInfo = cList.Find(it => it.Id == item.classId);
                        item.className = cInfo.title;
                    }
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }
        #endregion

        #region 详情
        public async Task<ResultModel> frontEndInfo(QueryModel model, UserInfo user)
        {
            var res = new ResultModel();
            if (model == null || !model.queryId.HasValue) { res.msg = "参数错误"; return res; }
            var info = await GoodsDb.GetByIdAsync(model.queryId.Value);
            if (info == null || info.status == 99)
            {
                res.msg = "商品不存在或已下架";
                return res;
            }
            var goods = mapper.ToView(info);
            goods.imgList = WebFileHelper.GetListUrl(goods.pictureList);

            var skuList = await GoodSkuDb.GetListAsync(a => a.goodsId == goods.Id && a.status == 0);
            if (skuList != null && skuList.Count > 0)
            {
                foreach (var item in skuList)
                {
                    item.url = WebFileHelper.GetUrl(item.url);
                }
            }
            goods.skuList = skuList;
            goods.contents = WebFileHelper.getContent(goods.contents);

            var isCollection = false;
            if (user != null && user.Id > 0)
            {
                isCollection = await CollectionRecordDb.CountAsync(it => it.cType == 0 && it.userId == user.Id && it.cId == goods.Id && it.status == 1) > 0;
            }
            res.data = new
            {
                goods,
                isCollection
            };
            res.msg = "ok";
            res.code = (int)ResultEnum.success;
            return res;
        }

        #endregion

        /// <summary>
        /// 新增商品入库
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<ResultModel> Operation(GoodsView model)
        {
            var res = new ResultModel();
            if (model == null) { res.msg = "数据错误"; return res; }

            if (model.skuList != null && model.skuList.Count > 0)
            {
                foreach (var item in model.skuList)
                {
                    if (string.IsNullOrWhiteSpace(item.name))
                    {
                        res.msg = "商品规格名称不能为空";
                        return res;
                    }
                    if (item.markPrice < 0)
                    {
                        res.msg = $"{item.name}市场价不能小于0";
                        return res;
                    }
                    if (item.price < 0)
                    {
                        res.msg = $"{item.name}销售单价不能小于0";
                        return res;
                    }
                    if (item.sale < 0)
                    {
                        res.msg = $"{item.name}销量不能小于0";
                        return res;
                    }
                    if (item.stock < 0)
                    {
                        res.msg = $"{item.name}库存不能小于0";
                        return res;
                    }
                    item.url = item.url.Replace(PubConstant.Config.DomianStaticName, "");

                }
                model.stock = model.skuList.Sum(a => a.stock);
            }
            else
            {
                res.msg = "商品规格不能为空";
                return res;
            }


            var info = mapper.ToModel(model);
            info.coverPicture = info.coverPicture.Replace(PubConstant.Config.DomianStaticName, "");
            info.pictureList = info.pictureList.Replace(PubConstant.Config.DomianStaticName, "");
            info.contents = info.contents.Replace(PubConstant.Config.DomianStaticName, "");
            db.Ado.BeginTran();
            try
            {
                //if (!string.IsNullOrWhiteSpace(info.Url)) info.ImgUrl = info.Url.Split(',')[0];

                if (model.Id > 0)
                {
                    info.updateTime = DateTime.Now;
                    await GoodsDb.UpdateAsync(info);
                }
                else
                {
                    info.createAdminId = (int)_claimsAccessor.UserId;
                    info.createTime = DateTime.Now;
                    info.updateTime = DateTime.Now;
                    info.Id = await GoodsDb.InsertReturnIdentityAsync(info);
                }
                #region 规格
                model.skuList.ForEach(item => { item.goodsId = info.Id; item.updateTime = DateTime.Now; });
                if (model.skuList.Count(a => a.Id > 0) > 0)
                {
                    var sku1 = model.skuList.Where(a => a.Id > 0).ToList();
                    await GoodSkuDb.UpdateRangeAsync(sku1);
                }
                if (model.skuList.Count(a => a.Id == 0) > 0)
                {
                    var sku1 = model.skuList.Where(a => a.Id == 0).ToList();

                    foreach (var item in sku1)
                    {
                        //添加规格
                        item.status = 0;
                        item.createTime = DateTime.Now;
                        item.Id = GoodSkuDb.InsertReturnIdentity(item);
                    }
                }
                #endregion

                db.Ado.CommitTran();
                res.code = (int)ResultEnum.success;
                res.msg = "操作成功";

            }
            catch (Exception ex)
            {
                res.msg = ex.Message;
                db.Ado.RollbackTran();
            }

            return res;
        }



    }
}