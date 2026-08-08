namespace YW.Service
{
    public partial interface IFeedbackService : IBaseRepository<Feedback>
    {
        #region 前端
        /// <summary>
        /// 前端新增
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ResultModel> frontEndInsertFeedback(FeedbackView model, UserInfo user);
        #endregion

    }
    public partial class FeedbackService : BaseRepository<Feedback>, IFeedbackService
    {
        private readonly IClaimsAccessor _claimsAccessor;
        private readonly FeedbackMapper _mapper = new();

        public FeedbackService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

        #region 前端

        public async Task<ResultModel> frontEndInsertFeedback(FeedbackView model, UserInfo user)
        {
            var res = new ResultModel();
            var info = _mapper.ToModel(model);
            bool isok = false;
            if (string.IsNullOrWhiteSpace(model.userName))
                info.userId = (int)_claimsAccessor.UserId;
            info.createTime = DateTime.Now;

            isok = await base.InsertAsync(info);
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");
            return res;
        }

        #endregion
    }

}
