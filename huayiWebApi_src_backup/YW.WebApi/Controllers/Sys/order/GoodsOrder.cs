using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysGoodsOrderController
    /// </summary>
    public class SysGoodsOrderController : BaseController
    {

        private readonly IGoodsOrderService _GoodsOrderService;

        private readonly GoodsOrderMapper mapper = new();
        public SysGoodsOrderController(IClaimsAccessor claimsAccessor,
            GoodsOrderService GoodsOrderService)
        {
            _claimsAccessor = claimsAccessor;
            _GoodsOrderService = GoodsOrderService;
        }

        #region GoodsOrder操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(GoodsOrderQuery view) => await _GoodsOrderService.backEndList(view, admin);

        /// <summary>
        /// 订单详情
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> OrderDetail(QueryModel view) => await _GoodsOrderService.OrderDetail(view);
        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(GoodsOrderView model)
        {
            var res = new ResultModel();
            var info = mapper.ToModel(model);
            bool isok = false;
            if (info.Id > 0)
            {
                info.updateTime = DateTime.Now;
                isok = await _GoodsOrderService.UpdateAsync(info);
            }
            else
            {
                info.createTime = DateTime.Now;
                info.updateTime = DateTime.Now;
                // isok =await _GoodsOrderService.Insert(info);
            }
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");

            return res;
        }
        #endregion

        #region 确认线下付款

        [HttpPost]
        public async Task<ResultModel> OfflinePayment(GoodsOrderView model) => await _GoodsOrderService.OfflinePayment(model);

        #endregion

        #region 确认发货

        [HttpPost]
        public async Task<ResultModel> ConfirmDeliver(LogisticsView model) => await _GoodsOrderService.ConfirmDeliver(model);

        #endregion

        #region 确认退款

        [HttpPost]
        public async Task<ResultModel> ConfirmRefund(AfterSaleView model) => await _GoodsOrderService.ConfirmRefund(model);

        #endregion


    }
}