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

            // 需求3：用戶聯絡客服通知管理員（含上傳圖片）
            if (isok)
            {
                _ = NotifyAdminForFeedback(info);
            }

            return res;
        }

        /// <summary>
        /// 用戶聯絡客服通知管理員（需求3）
        /// </summary>
        private async Task NotifyAdminForFeedback(Feedback info)
        {
            try
            {
                var adminEmail = YW.Common.ConfigHelper.GetSectionValue("EmailSetting:AdminTo") ?? "studioofjoyhk@gmail.com";
                var body = $@"
                    <h3>【華藝】用戶聯絡客服</h3>
                    <p>管理員你好，</p>
                    <p>有用戶透過「意見反饋/聯絡客服」提交了一條訊息：</p>
                    <ul>
                        <li>用戶ID：{info.userId}</li>
                        <li>聯絡方式：{info.contact}</li>
                        <li>標題：{info.title}</li>
                        <li>內容：{info.contents}</li>
                        <li>時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</li>
                    </ul>
                    <p>請在應用內消息系統回覆用戶。</p>
                ";
                byte[] img = null;
                if (!string.IsNullOrEmpty(info.imgUrl))
                {
                    var absPath = Path.Combine(Directory.GetCurrentDirectory(),
                        info.imgUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (File.Exists(absPath)) img = await File.ReadAllBytesAsync(absPath);
                }
                await YW.Common.EmailClient.SendAsync(adminEmail, "【華藝】用戶聯絡客服", body, ("feedback.png", img));
            }
            catch { }
        }

        #endregion
    }

}
