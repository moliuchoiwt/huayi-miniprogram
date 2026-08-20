using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.ApiControllers
{
    /// <summary>
    /// 评论
    /// </summary>
    public class CommentController : BaseController
    {
        private readonly ICommentService _commentService;
        private readonly IUserInfoService _userInfoService;
        public CommentController(CommentService commentService,
            IClaimsAccessor claimsAccessor,
            UserInfoService userInfoService)
        {
            _commentService = commentService;
            _claimsAccessor = claimsAccessor;
            _userInfoService = userInfoService;
        }

        /// <summary>获取评价列表（商品/店铺）</summary>
        [HttpPost]
        public async Task<ResultModel> CommentList(GoodsQuery queryModel) =>
            await _commentService.frontEndList(queryModel);

        /// <summary>批量提交评论（商品评论）</summary>
        [HttpPost]
        [Authorize(Roles = "api")]
        public async Task<ResultModel> SumbitComment(CommentQuery view)
        {
            var res = new ResultModel();
            if (view == null || view.commentList == null) { res.msg = "参数错误"; return res; }
            res = await _commentService.SumbitComment(view.commentList);
            return res;
        }

        // ===== 需求1：商戶評個體戶 =====

        /// <summary>
        /// 商戶評價個體戶（5 維評分：作品/需時/服務/物流/整體）
        /// 需要登入，一單只能評一次。
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "api")]
        public async Task<ResultModel> SubmitWorkerEval(CommentView model) =>
            await _commentService.SubmitWorkerEval(model, user);

        /// <summary>
        /// 查詢個體戶公開評分摘要（5 維平均分 + 評價總數）
        /// 參數：{ queryId: 個體戶userId }
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> GetWorkerRating(GoodsQuery queryModel)
        {
            var res = new ResultModel();
            if (!queryModel.queryId.HasValue) { res.msg = "请传入个体户用户Id"; return res; }
            return await _commentService.GetWorkerRating(queryModel.queryId.Value);
        }

        /// <summary>
        /// 查詢個體戶公開評價列表（帶分頁，含評價人頭像/圖片）
        /// 參數：{ queryId: 個體戶userId, pageNum, pageSize }
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> GetWorkerEvalList(GoodsQuery queryModel) =>
            await _commentService.GetWorkerEvalList(queryModel);
    }
}

