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
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> CommentList(GoodsQuery queryModel) => await _commentService.frontEndList(queryModel);



        /// <summary>
        ///提交批量
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "api")]
        public async Task<ResultModel> SumbitComment(CommentQuery view)
        {
            var res = new ResultModel();
            if (view == null || view.commentList == null) { res.msg = "参数错误"; return res; }
            res = await _commentService.SumbitComment(view.commentList);
            return res;
        }
    }
}
