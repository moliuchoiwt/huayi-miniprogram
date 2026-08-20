using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysBannerController
    /// </summary>
    public class SysBannerController : BaseController
    {

        private readonly IBannerService _bannerService;

        private readonly BannerMapper mapper = new();

        public SysBannerController(IClaimsAccessor claimsAccessor, BannerService bannerService)
        {
            _claimsAccessor = claimsAccessor;
            _bannerService = bannerService;
        }

        #region banner操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel view) => await _bannerService.BackEndList(view);


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(BannerView model)
        {
            var res = new ResultModel();
            if (model == null) { res.msg = "参数错误"; return res; }
            var info = mapper.ToModel(model);
            if (info.imgUrl.Contains(PubConstant.Config.DomianStaticName)) info.imgUrl = info.imgUrl.Replace(PubConstant.Config.DomianStaticName, "");
            bool isok = false;
            if (info.Id > 0)
            {
                info.updateTime = DateTime.Now;
                isok = await _bannerService.UpdateAsync(info);
            }
            else
            {
                info.createTime = DateTime.Now;
                info.updateTime = DateTime.Now;
                isok = await _bannerService.InsertAsync(info);
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

            if (del.ids != null && del.ids.Length > 0)
            {
                var isok = await _bannerService.UpdateAsync(it => new Banner { status = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));

                res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "删除" + (isok ? "成功" : "失败");
            }

            return res;

        }
        #endregion
    }
}