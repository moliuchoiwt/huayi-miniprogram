using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysCommentController
    /// </summary>
    public class SysCommentController : BaseController
    {

        private readonly ICommentService _commentService;

        private readonly CommentMapper mapper = new();
        public SysCommentController(IClaimsAccessor claimsAccessor,
            CommentService commentService)
        {
            _claimsAccessor = claimsAccessor;
            _commentService = commentService;
        }

        #region comment操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(GoodsQuery queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<Comment>();
            if (queryModel.queryType.HasValue)
            {
                exWhere.And(a => a.cType == queryModel.queryType.Value);
            }
            if (queryModel.parentId.HasValue)
            {
                exWhere.And(a => a.parentId == queryModel.parentId.Value);
            }
            exWhere.And(a => a.status != 99);
            if (queryModel.queryState.HasValue)
            {
                exWhere.And(a => a.status == queryModel.queryState.Value);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                int.TryParse(queryModel.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.name.Contains(queryModel.queryName) || a.intro.Contains(queryModel.queryName));

            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= queryModel.endTime.Value);
            }

            var data = await _commentService.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            var list = new List<CommentView>();
            if (data.Count > 0)
            {
                list = mapper.ToViewList(data);
                foreach (var item in list)
                {
                    item.imgList = WebFileHelper.GetListUrl(item.url);

                }

            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new
            {
                total = p.TotalCount,
                items = list
            };
            return res;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> DelComment(DelModel del)
        {
            var res = new ResultModel();

            if (del.ids != null && del.ids.Length > 0)
            {
                var isok = await _commentService.UpdateAsync(it => new Comment { status = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));

                res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "删除" + (isok ? "成功" : "失败");
            }

            return res;

        }
        #endregion
    }
}