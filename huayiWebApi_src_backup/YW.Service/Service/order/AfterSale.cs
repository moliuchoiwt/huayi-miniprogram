namespace YW.Service
{
    public partial interface IAfterSaleService : IBaseRepository<AfterSale>
    {
        /// <summary>
        /// 前端售后记录列表
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        Task<ResultModel> frontEndList(AfterSaleView view);

    }
    public partial class AfterSaleService : BaseRepository<AfterSale>, IAfterSaleService
    {
        private readonly AfterSaleMapper mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public AfterSaleService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

        #region 列表
        public async Task<ResultModel> frontEndList(AfterSaleView view)
        {
            var res = new ResultModel();
            if (string.IsNullOrWhiteSpace(view.detailNo))
            {
                res.msg = "参数错误";
                return res;
            }
            //退款列表
            var rList = new List<AfterSaleView>();
            var refundData = await AfterSaleDb.GetListAsync(a => a.status != 99 && a.detailNo == view.detailNo);
            if (refundData != null && refundData.Count > 0)
            {
                rList = mapper.ToViewList(refundData);
                foreach (var item in rList)
                {
                    if (!string.IsNullOrWhiteSpace(item.url)) item.imgList = WebFileHelper.GetListUrl(item.url);
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = rList;
            return res;
        }
        #endregion
    }
}