using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysShopController
    /// </summary>
    public class SysShopController : BaseController
    {

        private readonly IShopService _shopService;

        public SysShopController(IClaimsAccessor claimsAccessor, ShopService shopService)
        {
            _claimsAccessor = claimsAccessor;
            _shopService = shopService;
        }

        #region shop操作        
        [HttpPost]
        public async Task<ResultModel> List(ShopQuery view) => await _shopService.BackEndList(view);


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(ShopView model) => await _shopService.Operation(model);


        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> DelShop(DelModel del)
        {
            var res = new ResultModel();

            if (del.ids == null || del.ids.Length <= 0)
            {
                res.msg = "请选择删除数据";
                return res;
            }
            var isok = await _shopService.UpdateAsync(it => new Shop { status = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));

            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");


            return res;

        }
        #endregion
    }
}