using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysClassController
    /// </summary>
    public class SysClassController : BaseController
    {

        private readonly IClassService _classService;
        private readonly IGoodsService _goodsService;

        private readonly ClassMapper mapper = new();
        public SysClassController(IClaimsAccessor claimsAccessor, ClassService classService, GoodsService goodsService)
        {
            _claimsAccessor = claimsAccessor;
            _classService = classService;
            _goodsService = goodsService;
        }

        #region class操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel queryModel) => await _classService.backEndList(queryModel);


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(ClassView model)
        {
            var res = new ResultModel();
            var info = mapper.ToModel(model);
            bool isok = false;
            if (info.Id > 0)
            {
                info.updateTime = DateTime.Now;
                isok = await _classService.UpdateAsync(info);
            }
            else
            {
                info.createTime = DateTime.Now;
                info.updateTime = DateTime.Now;
                isok = await _classService.InsertAsync(info);
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
                var isok = await _classService.UpdateAsync(it => new Class { status = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));

                res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "删除" + (isok ? "成功" : "失败");
            }

            return res;

        }
        #endregion
    }
}