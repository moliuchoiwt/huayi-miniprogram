namespace YW.Service
{
    public partial interface ICollectionRecordService : IBaseRepository<CollectionRecord>
    {

        /// <summary>
        /// 前端列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<ResultModel> frontEndList(QueryModel queryModel, UserInfo user);

        Task<ResultModel> frontEndOperation(CollectionRecordView model, UserInfo user);

    }
    public partial class CollectionRecordService : BaseRepository<CollectionRecord>, ICollectionRecordService
    {
        private readonly CollectionRecordMapper _mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public CollectionRecordService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

        #region 列表
        public async Task<ResultModel> frontEndList(QueryModel queryModel, UserInfo user)
        {
            var res = new ResultModel();
            if (user == null)
            {
                res.msg = "登录过期，请重新登录";
                return res;
            }
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<CollectionRecord>(it => it.status == 1 && it.userId == user.Id);
            if (queryModel.queryType.HasValue)
            {
                exWhere.And(a => a.cType == queryModel.queryType.Value);
            }
            else exWhere.And(it => it.cType == 0);

            var list = new List<CollectionRecordView>();
            var data = await CollectionRecordDb.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            if (data != null && data.Count > 0)
            {
                var gIds = data.Where(it => it.cType == 0).Select(it => it.cId).Distinct().ToList();
                var gList = await GoodsDb.GetListAsync(it => SqlFunc.ContainsArray(gIds, it.Id));

                list = _mapper.ToViewList(data);
                foreach (var item in list)
                {
                    //商品
                    if (item.cType == 0 && gList.Count(it => it.Id == item.cId) > 0)
                    {
                        var gInfo = gList.Find(it => it.Id == item.cId);
                        item.name = gInfo.name;
                        item.price = gInfo.price;
                        item.coverImage = gInfo.coverPicture;
                    }

                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }

        #endregion

        #region 编辑
        public async Task<ResultModel> frontEndOperation(CollectionRecordView model, UserInfo user)
        {
            var res = new ResultModel();
            var info = _mapper.ToModel(model);
            bool isok = false;
            info.readState = 0;
            info.userId = (int)_claimsAccessor.UserId;
            var gInfo = await GoodsDb.GetByIdAsync(info.cId);
            if (gInfo == null) { res.msg = "未查询到商品信息"; return res; }


            if (base.CountAsync(a => a.cType == info.cType && a.cId == model.cId && a.userId == info.userId).Result > 0)
            {
                var coll = await base.GetSingleAsync(a => a.cType == info.cType && a.cId == model.cId && a.userId == info.userId);
                coll.status = coll.status == 0 ? 1 : 0;
                coll.updateTime = DateTime.Now;
                isok = await base.UpdateAsync(coll);
            }
            else
            {
                info.cType = 0;
                info.status = 1;
                info.createTime = DateTime.Now;
                info.updateTime = DateTime.Now;
                isok = await base.InsertAsync(info);
            }
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");

            return res;
        }
        #endregion
    }
}