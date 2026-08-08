using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.ApiControllers
{
    /// <summary>
    /// 资讯
    /// </summary>
    public class NewsController : BaseController
    {
        private readonly IClassService _classService;
        private readonly IBannerService _bannerService;
        public NewsController(IClaimsAccessor claimsAccessor,
            ClassService classService, BannerService bannerService
            )
        {
            _claimsAccessor = claimsAccessor;
            _classService = classService;
            _bannerService = bannerService;
        }


        /// <summary>
        /// 分类列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> ClassList(QueryModel view) => await _classService.frontEndList(view);
        /// <summary>
        /// 轮播列表
        /// </summary>        
        public async Task<ResultModel> BannerList(QueryModel view) => await _bannerService.FrontEndList(view);
    }
}
