namespace YW.Service
{
    public partial interface IBrowsesService : IBaseRepository<Browses>
    {

        /// <summary>
        /// 列表
        /// </summary>        
        /// <returns></returns>
        Task<ResultModel> frontEndList(QueryModel query, UserInfo user);

        Task<ResultModel> frontEndOperation(BrowsesView view, UserInfo user);
    }
    public partial class BrowsesService : BaseRepository<Browses>, IBrowsesService
    {
        private readonly BrowsesMapper _mapper = new();

        #region 列表
        public async Task<ResultModel> frontEndList(QueryModel query, UserInfo user)
        {
            var res = new ResultModel();
            try
            {
                var p = new PageModel() { PageIndex = query.pageNum, PageSize = query.pageSize };
                var exWhere = PredicateBuilder.New<Browses>(a => a.State == 0 && a.UserId == user.Id);
                if (query.queryType.HasValue) exWhere.And(it => it.BrowseType == query.queryType.Value);
                var data = await base.GetPageListAsync(exWhere, p, it => new { it.CreateTime }, OrderByType.Desc);
                if (data != null && data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        item.BrowsesImage = WebFileHelper.GetUrl(item.BrowsesImage);
                    }
                }
                res.data = new { total = p.TotalCount, items = data };
                res.code = (int)ResultEnum.success;
                res.msg = "请求成功";
            }
            catch (Exception ex)
            {
                res.msg = ex.Message;
            }
            return res;
        }
        #endregion

        #region 编辑
        public async Task<ResultModel> frontEndOperation(BrowsesView view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            try
            {
                var info = _mapper.ToModel(view);

                if (info.Id > 0)
                {
                    info.BrowsesEndTime = DateTime.Now;
                    info.ReadTimes = (int)(info.BrowsesEndTime - info.BrowsesTime).TotalSeconds;

                    await base.UpdateAsync(it => new Browses { BrowsesEndTime = info.BrowsesEndTime, ReadTimes = info.ReadTimes }, it => it.Id == info.Id);
                }
                else
                {
                    info.BrowseType = 0;
                    var gInfo = await GoodsDb.GetByIdAsync(info.BrowsesId);
                    if (gInfo == null) { res.msg = "未查询到商品信息"; return res; }
                    info.BrowsesTitle = gInfo.name;
                    info.BrowsesImage = gInfo.coverPicture;
                    info.BrowsesPrice = gInfo.price;

                    info.UserId = user.Id;
                    info.UserName = user.nickName;
                    info.CreateTime = DateTime.Now;
                    info.BrowsesTime = DateTime.Now;
                    info.Id = await base.InsertReturnIdentityAsync(info);
                }
                res.data = info;
                res.msg = "请求成功";
                res.code = (int)ResultEnum.success;
            }
            catch (Exception ex)
            {
                res.msg = ex.Message;
            }
            return res;
        }

        #endregion

    }
}
