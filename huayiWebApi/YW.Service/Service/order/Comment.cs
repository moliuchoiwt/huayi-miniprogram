namespace YW.Service
{
    public partial interface ICommentService : IBaseRepository<Comment>
    {
        Task<ResultModel> frontEndList(GoodsQuery queryModel);
        Task<ResultModel> SumbitComment(List<CommentView> list);

        /// <summary>需求1：商戶評個體戶（5維評分）</summary>
        Task<ResultModel> SubmitWorkerEval(CommentView model, UserInfo user);

        /// <summary>需求1：查詢個體戶公開評分摘要</summary>
        Task<ResultModel> GetWorkerRating(int workerUserId);

        /// <summary>需求1：查詢個體戶公開評價列表</summary>
        Task<ResultModel> GetWorkerEvalList(GoodsQuery queryModel);
    }

    //评论记录
    public partial class CommentService : BaseRepository<Comment>, ICommentService
    {
        private readonly CommentMapper mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public CommentService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

        #region 列表
        public async Task<ResultModel> frontEndList(GoodsQuery queryModel)
        {
            var res = new ResultModel();
            if (queryModel == null) { res.msg = "参数错误"; return res; }
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<Comment>(a => a.status != 99);
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                int.TryParse(queryModel.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.name.Contains(queryModel.queryName) ||
               a.intro.Contains(queryModel.queryName));

            }
            if (queryModel.queryType.HasValue)
            {

                exWhere.And(a => a.cType == queryModel.queryType.Value);
                if (queryModel.queryId.HasValue)
                {
                    exWhere.And(a => a.comId == queryModel.queryId.Value);
                }
            }
            else exWhere.And(it => it.cType == 0);

            if (queryModel.userId.HasValue)
            {
                exWhere.And(a => a.userId == queryModel.userId.Value);
            }
            if (queryModel.parentId.HasValue)
            {
                exWhere.And(a => a.parentId == queryModel.parentId.Value);
            }
            if (queryModel.queryState.HasValue)
            {
                exWhere.And(a => a.status == queryModel.queryState.Value);
            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= queryModel.endTime.Value);
            }

            var data = await CommentDb.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            var list = new List<CommentView>();
            if (data != null && data.Count > 0)
            {
                list = mapper.ToViewList(data);

                //用户
                var uIds = list.Select(a => a.userId).ToList();
                var ulist = await UserInfoDb.GetListAsync(a => SqlFunc.ContainsArray(uIds, a.Id));

                foreach (var item in list)
                {
                    //用户
                    if (ulist.Count(it => it.Id == item.userId) > 0)
                    {
                        var uInfo = ulist.Find(it => it.Id == item.userId);
                        item.avatar = WebFileHelper.GetUrl(uInfo.avatar);
                        item.userName = uInfo.nickName;
                    }
                    //图片列表
                    if (!string.IsNullOrWhiteSpace(item.url)) item.imgList = WebFileHelper.GetListUrl(item.url);
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }
        #endregion


        /// <summary>
        ///提交批量
        /// </summary>
        public async Task<ResultModel> SumbitComment(List<CommentView> views)
        {
            var res = new ResultModel();
            if (views == null || views.Count <= 0)
            {
                res.msg = "评论数据不能为空";
                return res;
            }
            db.Ado.BeginTran();
            try
            {
                var addList = new List<Comment>();
                var clist = mapper.ToModelList(views);
                var user = await UserInfoDb.GetByIdAsync((int)_claimsAccessor.UserId);
                string uname = string.IsNullOrWhiteSpace(user.alias) ? user.nickName : user.nickName;

                #region 商品评论

                if (clist.Count(a => a.comId > 0 && a.cType == 0) > 0)
                {
                    var ordernoList = new List<string>();
                    var list0 = clist.Where(a => a.comId > 0 && a.cType == 0).ToList();
                    var ids = list0.Select(a => a.comId).ToList();
                    var goodslist = await GoodsDb.GetListAsync(a => SqlSugar.SqlFunc.ContainsArray(ids, a.Id));
                    foreach (var item in list0)
                    {
                        if (goodslist != null && goodslist.Count(a => a.Id == item.comId) > 0)
                        {
                            var ginfo = goodslist.FirstOrDefault(a => a.Id == item.comId);
                            item.status = 0;
                            item.userId = user.Id;
                            item.comId = ginfo.Id;
                            if (string.IsNullOrWhiteSpace(item.name)) item.name = ginfo.name;
                            addList.Add(item);
                            ordernoList.Add(item.orderNo);
                        }
                    }
                    ordernoList = ordernoList.Distinct().ToList();

                    //更新订单状态
                    GoodsOrderDb.Update(a => new GoodsOrder { status = (int)OrderStateEnum.已完成, updateTime = System.DateTime.Now }, it => it.status == (int)OrderStateEnum.待评论 && SqlSugar.SqlFunc.ContainsArray(ordernoList, it.orderNo));
                    GoodsOrderDetailDb.Update(a => new GoodsOrderDetail { status = (int)OrderStateEnum.已完成, updateTime = System.DateTime.Now }, it => it.status == (int)OrderStateEnum.待评论 && SqlSugar.SqlFunc.ContainsArray(ordernoList, it.orderNo));
                }
                #endregion

                #region 店铺评论

                //if (clist.Count(a => a.ComId > 0 && a.Type == 1) > 0)
                //{
                //    var list0 = clist.Where(a => a.ComId > 0 && a.Type == 1).ToList();
                //    var ids = list0.Select(a => a.ComId).ToList();
                //    var slist = await ShopDb.GetListAsync(a => SqlSugar.SqlFunc.ContainsArray(ids, a.Id));
                //    foreach (var item in list0)
                //    {
                //        if (slist != null && slist.Count(a => a.Id == item.ComId) > 0)
                //        {
                //            var sinfo = slist.FirstOrDefault(a => a.Id == item.ComId);
                //            item.State = 0;
                //            item.UserId = user.Id;
                //            item.UserName = uname;
                //            item.ComId = sinfo.Id;
                //            if (string.IsNullOrWhiteSpace(item.Name)) item.Name = sinfo.Name;
                //            addList.Add(item);
                //            sids.Add(sinfo.Id);
                //        }
                //    }
                //}
                #endregion

                //同步店铺评分
                //sids = sids.Distinct().ToList();
                //foreach (var shopid in sids)
                //{                    

                //    db.Ado.ExecuteCommand($@"declare @score decimal(18,2) 
                //    select @score=isnull(avg(Score),5) from Comment 
                //    where ( Type=0 and  exists (select 1 from Goods where 
                //    comment.ComId=Goods.Id and ShopId={shopid})) 
                //    update Shop set Score=@score where id={shopid}");
                //}
                await CommentDb.InsertRangeAsync(addList);

                db.Ado.CommitTran();
                res.code = (int)ResultEnum.success;
                res.msg = "提交评论成功";
            }
            catch (System.Exception ex)
            {
                db.Ado.RollbackTran();
                res.msg = ex.Message;
            }

            return res;
        }

        #region 需求1：商戶評個體戶

        /// <summary>
        /// 商戶評價個體戶（5 維評分：作品/需時/服務/物流/整體）
        /// 評價人角色記錄當時交易角色（evalRole=0 商戶），解決角色可互換問題。
        /// 一單只能評一次，評完後個體戶公開評分立即更新。
        /// </summary>
        public async Task<ResultModel> SubmitWorkerEval(CommentView model, UserInfo user)
        {
            var res = new ResultModel();
            if (user == null) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            if (string.IsNullOrWhiteSpace(model.orderNo)) { res.msg = "缺少订单号"; return res; }
            if (model.comId <= 0) { res.msg = "请指定被评价的个体户"; return res; }

            // 防重複：同一訂單只能評一次
            if (await CommentDb.CountAsync(a => a.orderNo == model.orderNo && a.evalType == 1 && a.userId == user.Id) > 0)
            {
                res.msg = "该订单已评价，不能重复提交"; return res;
            }

            // 驗證訂單：確認評價人係發单方（商戶），且訂單已完成
            var order = await TaskOrderDb.GetFirstAsync(a => a.orderNo == model.orderNo && a.status == 5);
            if (order == null) { res.msg = "订单不存在或尚未完成"; return res; }

            // 驗證被評人（個體戶）是接单方
            if (order.userId != model.comId) { res.msg = "被评价用户与订单不符"; return res; }

            // 整體分 = 五維平均（若前端傳了整體分則直接用）
            decimal avgScore = (model.scoreWork + model.scoreTime + model.scoreService + model.scoreLogistics) / 4m;
            if (model.score <= 0) model.score = Math.Round(avgScore, 1);

            var info = mapper.ToModel(model);
            info.userId = user.Id;              // 評價人 = 當前登入用戶（商戶）
            info.evalType = 1;                  // 商戶評個體戶
            info.evalRole = 0;                  // 評價人角色 = 商戶(發单方)
            info.cType = 2;                     // Comment 類型 = 個體戶接單評價
            info.status = 0;                    // 公開展示
            info.createTime = DateTime.Now;
            info.updateTime = DateTime.Now;

            // 去掉域名前綴，只存相對路徑
            if (!string.IsNullOrEmpty(info.url) && info.url.Contains("http"))
                info.url = "";  // 前端上傳後已返回相對路徑，此處保留

            await db.Ado.BeginTranAsync();
            try
            {
                await CommentDb.InsertAsync(info);
                db.Ado.CommitTran();
                res.code = (int)ResultEnum.success;
                res.msg = "评价提交成功";
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                res.msg = ex.Message;
            }
            return res;
        }

        /// <summary>
        /// 查詢個體戶公開評分摘要（5維平均分 + 評價總數）
        /// </summary>
        public async Task<ResultModel> GetWorkerRating(int workerUserId)
        {
            var res = new ResultModel();
            var list = await CommentDb.GetListAsync(a =>
                a.comId == workerUserId && a.evalType == 1 && a.status == 0);

            if (list == null || list.Count == 0)
            {
                res.code = (int)ResultEnum.success;
                res.data = new
                {
                    totalCount = 0,
                    avgScore = 0m, avgWork = 0m, avgTime = 0m,
                    avgService = 0m, avgLogistics = 0m
                };
                return res;
            }

            res.code = (int)ResultEnum.success;
            res.data = new
            {
                totalCount = list.Count,
                avgScore    = Math.Round(list.Average(a => a.score), 1),
                avgWork     = Math.Round(list.Average(a => a.scoreWork), 1),
                avgTime     = Math.Round(list.Average(a => a.scoreTime), 1),
                avgService  = Math.Round(list.Average(a => a.scoreService), 1),
                avgLogistics= Math.Round(list.Average(a => a.scoreLogistics), 1)
            };
            return res;
        }

        /// <summary>
        /// 查詢個體戶公開評價列表（含評價人頭像/暱稱/圖片）
        /// </summary>
        public async Task<ResultModel> GetWorkerEvalList(GoodsQuery queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var where = PredicateBuilder.New<Comment>(a =>
                a.evalType == 1 && a.status == 0);
            if (queryModel.queryId.HasValue)
                where.And(a => a.comId == queryModel.queryId.Value);

            var data = await CommentDb.GetPageListAsync(where, p,
                it => new { it.createTime, it.Id }, OrderByType.Desc);
            var list = mapper.ToViewList(data ?? new List<Comment>());

            if (list.Count > 0)
            {
                var uIds = list.Select(a => a.userId).ToList();
                var ulist = await UserInfoDb.GetListAsync(a => SqlFunc.ContainsArray(uIds, a.Id));
                foreach (var item in list)
                {
                    var u = ulist.Find(x => x.Id == item.userId);
                    if (u != null) { item.avatar = WebFileHelper.GetUrl(u.avatar); item.userName = u.nickName; }
                    if (!string.IsNullOrEmpty(item.url)) item.imgList = WebFileHelper.GetListUrl(item.url);
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