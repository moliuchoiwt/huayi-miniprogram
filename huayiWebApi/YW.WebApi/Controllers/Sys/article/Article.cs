using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysArticleController
    /// </summary>
    public class SysArticleController : BaseController
    {

        private readonly IArticleService _articleService;

        private readonly ArticleMapper mapper = new();
        public SysArticleController(IClaimsAccessor claimsAccessor, ArticleService articleService)
        {
            _claimsAccessor = claimsAccessor;
            _articleService = articleService;
        }

        #region article操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel queryModel) => await _articleService.backEndList(queryModel);


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(ArticleView model)
        {
            var res = new ResultModel();
            var info = mapper.ToModel(model);

            bool isok = false;
            if (info.Id > 0)
            {
                info.updateTime = DateTime.Now;
                isok = await _articleService.UpdateAsync(info);
            }
            else
            {
                info.createTime = DateTime.Now;
                info.updateTime = DateTime.Now;
                isok = await _articleService.InsertAsync(info);
            }
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");

            return res;
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Delete(DelModel del)
        {
            var res = new ResultModel();

            if (del.ids == null || del.ids.Length <= 0)
            {
                res.msg = "请选择删除数据";
                return res;
            }
            var isok = await _articleService.UpdateAsync(it => new Article { status = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));

            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");


            return res;

        }
        #endregion        
    }
}