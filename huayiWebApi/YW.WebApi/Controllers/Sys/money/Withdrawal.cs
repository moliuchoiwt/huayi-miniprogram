using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysWithdrawalController
    /// </summary>
    public class SysWithdrawalController : BaseController
    {

        private readonly IWithdrawalService _withdrawalService;
        public SysWithdrawalController(IClaimsAccessor claimsAccessor, WithdrawalService withdrawalService)
        {
            _claimsAccessor = claimsAccessor;
            _withdrawalService = withdrawalService;
        }

        #region withdrawal操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel view) => await _withdrawalService.BackEndList(view);


        /// <summary>
        /// 审核
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> AuditWithdrawal(AuditWithdrawView view) => await _withdrawalService.AuditWithdrawal(view);


        #endregion

    }
}