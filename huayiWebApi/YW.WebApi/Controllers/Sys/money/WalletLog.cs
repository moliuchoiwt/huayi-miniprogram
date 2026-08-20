using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysWalletLogController
    /// </summary>
    public class SysWalletLogController : BaseController
    {

        private readonly IWalletLogService _walletLogService;
        private readonly WalletLogMapper mapper = new();
        public SysWalletLogController(IClaimsAccessor claimsAccessor, WalletLogService walletLogService)
        {
            _claimsAccessor = claimsAccessor;
            _walletLogService = walletLogService;
        }

        #region walletLog操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel view) => await _walletLogService.BackEndList(view);


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(WalletLogView model)
        {
            var res = new ResultModel();
            var info = mapper.ToModel(model);
            bool isok = false;
            if (info.Id > 0)
            {
                info.updateTime = DateTime.Now;
                isok = await _walletLogService.UpdateAsync(info);
            }
            else
            {

                info.updateTime = DateTime.Now;
                isok = await _walletLogService.InsertAsync(info);
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
        public async Task<ResultModel> DelWalletLog(DelModel del)
        {
            var res = new ResultModel();

            if (del.ids == null || del.ids.Length <= 0)
            {
                res.msg = "请选择删除数据";
                return res;
            }
            var isok = await _walletLogService.DeleteAsync(del.ids);

            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");


            return res;

        }
        #endregion
    }
}