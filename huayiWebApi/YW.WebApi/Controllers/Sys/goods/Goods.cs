using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysGoodsController
    /// </summary>
    public class SysGoodsController : BaseController
    {

        private readonly IGoodsService _goodsService;
        private readonly IGoodSkuService _goodSkuService;
        private readonly IUserGradeService _userGradeService;


        public SysGoodsController(IClaimsAccessor claimsAccessor,
            GoodsService goodsService,
            GoodSkuService goodSkuService,
            UserGradeService userGradeService)
        {
            _claimsAccessor = claimsAccessor;
            _goodsService = goodsService;
            _goodSkuService = goodSkuService;
            _userGradeService = userGradeService;
        }

        #region goods操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(GoodsQuery queryModel) => await _goodsService.backEndList(queryModel);



        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(GoodsView model) => await _goodsService.Operation(model);



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
                var isok = await _goodsService.UpdateAsync(it => new Goods { status = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));

                res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "删除" + (isok ? "成功" : "失败");
            }

            return res;

        }
        #endregion
    }
}