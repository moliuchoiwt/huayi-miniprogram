using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysTaskOrderController
    /// </summary>
    public class SysTaskOrderController : BaseController
    {
        private readonly ITaskOrderService _taskOrderService;
        public SysTaskOrderController(IClaimsAccessor claimsAccessor, TaskOrderService taskOrderService)
        {
            _claimsAccessor = claimsAccessor;
            _taskOrderService = taskOrderService;
        }

        #region 操作        
        [HttpPost]
        public async Task<ResultModel> List(TaskOrderQuery view) => await _taskOrderService.BackEndEndList(view);


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(TaskOrderView model) => await _taskOrderService.BackEndOperation(model);


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

            var isok = await _taskOrderService.UpdateAsync(it => new TaskOrder { status = (int)TaskOrderStateEnum.已删除, updateTime = DateTime.Now }, it => SqlFunc.ContainsArray(del.ids, it.Id));
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");
            return res;
        }
        #endregion

        #region 确认线下付款
        public async Task<ResultModel> OfflinePayment(TaskOrderView model) => await _taskOrderService.OfflinePayment(model);
        #endregion

        #region 确认退款

        [HttpPost]
        public async Task<ResultModel> ConfirmRefund(TaskOrderView model) => await _taskOrderService.ConfirmRefund(model);

        #endregion


    }
}