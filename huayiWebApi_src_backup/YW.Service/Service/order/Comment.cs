namespace YW.Service
{
    public partial interface ICommentService : IBaseRepository<Comment>
    {

        /// <summary>
        /// 前端列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<ResultModel> frontEndList(GoodsQuery queryModel);

        /// <summary>
        /// 批量提交评论
        /// </summary>
        Task<ResultModel> SumbitComment(List<CommentView> list);
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
    }
}