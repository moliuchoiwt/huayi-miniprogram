namespace YW.Service
{
    public partial interface ILogisticsService : IBaseRepository<Logistics>
    {

        /// <summary>
        /// 物流信息
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        Task<ResultModel> ExpressInfo(LogisticsView view, UserInfo user);
    }
    public partial class LogisticsService : BaseRepository<Logistics>, ILogisticsService
    {
        private readonly LogisticsMapper mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public LogisticsService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

        #region 物流信息
        public async Task<ResultModel> ExpressInfo(LogisticsView view, UserInfo user)
        {
            var res = new ResultModel();
            try
            {
                if (LogisticsDb.Count(it => it.userId == user.Id && view.logisticsNo == it.logisticsNo && it.expressCode == view.expressCode) > 0)
                {
                    var data = await LogisticsDb.GetFirstAsync(it => it.userId == user.Id && view.logisticsNo == it.logisticsNo && it.expressCode == view.expressCode);

                    res.data = await ExpressHelper.getExpressData(PubConstant.Config.Customer, PubConstant.Config.CustomerKey, data.logisticsNo.Trim(), data.expressCode.Trim(), data.mobile.Trim());

                    res.msg = "OK";
                    res.code = (int)ResultEnum.success;
                }
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
