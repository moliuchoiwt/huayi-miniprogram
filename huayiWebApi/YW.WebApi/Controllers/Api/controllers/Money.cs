using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.ApiControllers
{
    /// <summary>
    /// 用户资金
    /// </summary>
    [Authorize(Roles = "api")]
    public class MoneyController : BaseController
    {
        private readonly IWalletLogService _walletLogService;
        private readonly IWithdrawalService _withdrawalService;

        private readonly WithdrawalMapper withdrawalMapper = new();

        public MoneyController(
            IClaimsAccessor claimsAccessor, WalletLogService walletLogService,
            WithdrawalService withdrawalService)
        {
            _claimsAccessor = claimsAccessor;
            _walletLogService = walletLogService;
            _withdrawalService = withdrawalService;
        }



        #region 我的资金钱包
        public async Task<ResultModel> MyWallet(QueryModel view) => await _walletLogService.MyWallet(view, user);
        #endregion

        #region 资金流水
        public async Task<ResultModel> MoneyList(QueryModel view) => await _walletLogService.FrontEndList(view, user);
        #endregion

        /// <summary>
        /// 提现申请
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Withdrawal(WithdrawalView model) => await _withdrawalService.ApplyForWithdrawal(model, user);

        /// <summary>
        /// 提现记录
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>

        [HttpPost]
        public async Task<ResultModel> WithdrawalList(QueryModel view) => await _withdrawalService.FrontEndList(view, user);



    }
}
