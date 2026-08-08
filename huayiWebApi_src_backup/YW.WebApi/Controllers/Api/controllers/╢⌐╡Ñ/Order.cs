using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Collections.Generic;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.ApiControllers
{
    /// <summary>
    /// 订单
    /// </summary>
    [Authorize(Roles = "api")]
    public class OrderController : BaseController
    {
        private readonly IGoodsOrderService _orderService;
        private readonly ITaskOrderService _taskOrderService;
        private readonly IAfterSaleService _AfterSaleService;
        private readonly ILogisticsService _logisticsService;
        public OrderController(IClaimsAccessor claimsAccessor,
        GoodsOrderService orderService, AfterSaleService AfterSaleService,
        LogisticsService logisticsService, TaskOrderService taskOrderService)
        {
            _claimsAccessor = claimsAccessor;
            _orderService = orderService;
            _AfterSaleService = AfterSaleService;
            _logisticsService = logisticsService;
            _taskOrderService = taskOrderService;
        }

        #region 商城订单列表        
        public async Task<ResultModel> OrderList(QueryModel view) => await _orderService.frontEndList(view, user);
        #endregion

        #region 店铺订单列表
        public async Task<ResultModel> StoreOrderList(QueryModel view) => await _orderService.FrontEndStoreList(view, user);
        #endregion

        #region 商城订单数

        /// <summary>
        /// 商城订单数
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> OrderCount() => await _orderService.frontEndOrderCount(user);

        #endregion

        #region 商城确认订单页面

        [HttpPost]
        public async Task<ResultModel> CreateOrderView(OrderView view) => await _orderService.CreateOrderView(view, user);

        #endregion

        #region 售后记录
        [HttpPost]
        public async Task<ResultModel> RefundList(AfterSaleView view) => await _AfterSaleService.frontEndList(view);
        #endregion

        #region 订单详情

        /// <summary>
        /// 订单详情
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> OrderDetail(QueryModel view) => await _orderService.OrderDetail(view);
        #endregion

        #region 订单确认收货
        /// <summary>
        /// 订单确认收货
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> OrderReceiving(GoodsOrderView view)
        {
            var res = new ResultModel();
            if (view == null || string.IsNullOrWhiteSpace(view.orderNo)) { res.msg = "订单单号参数不能为空"; return res; }
            if (await _orderService.CountAsync(a => a.userId == user.Id && a.isPay && a.orderNo == view.orderNo && a.status == (int)OrderStateEnum.待收货) <= 0)
            {
                res.msg = "非用户订单不能操作";
                return res;
            }
            var orderNos = new List<string> { view.orderNo };
            res = await _orderService.OrderReceiving(orderNos);
            return res;

        }
        #endregion

        #region 订单自提收货
        /// <summary>
        /// 订单自提收货
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> OrderSelfMention(List<string> views)
        {
            var res = new ResultModel();
            if (views == null || views.Count <= 0) { res.msg = "订单单号参数不能为空"; return res; }
            if (_orderService.CountAsync(a => a.userId == _claimsAccessor.UserId && a.isPay && SqlFunc.ContainsArray(views, a.orderNo) && a.status == (int)OrderStateEnum.待发货).Result <= 0)
            {
                res.msg = "非用户订单不能操作";
                return res;
            }
            res = await _orderService.OrderSelfMention(views);
            return res;

        }
        #endregion

        #region 申请退款

        [HttpPost]
        public async Task<ResultModel> ApplyRefund(AfterSaleView model) => await _orderService.ApplyRefund(model, user);

        #endregion

        #region 订单支付

        [HttpPost]
        public async Task<ResultModel> OrderAgainPay(OrderPayView view) => await _orderService.OrderAgainPay(view, user);

        #endregion

        #region 订单回调

        /// <summary>
        /// 订单微信回调
        /// </summary>
        [AllowAnonymous]
        [AcceptVerbs("GET", "POST")]
        public async Task<string> WxOrderNotifyUrl()
        {
            string refStr = "FAIL";

            string PayOrderNo;
            string xml;
            bool f = Service.WeChat.TenPayClient.PayNotifyUrl(HttpContext, out PayOrderNo, out xml);
            #region 支付成功业务处理
            if (f)
            {
                var res = await _orderService.OrderCallback(PayOrderNo);
                if (res != null && res.code == (int)ResultEnum.success) refStr = xml;
            }
            #endregion

            return refStr;
        }
        /// <summary>
        /// 支付宝回调
        /// </summary>
        [AllowAnonymous]
        [AcceptVerbs("GET", "POST")]
        public async Task<string> AliPayNotifyUrl()
        {
            string refStr = "fail";
            string PayOrderNo;
            bool f = Service.AliPay.AliPayClient.AliPayNotifyUrl(HttpContext, out PayOrderNo);
            #region 支付成功业务处理
            if (f)
            {
                var res = await _orderService.OrderCallback(PayOrderNo);
                if (res != null && res.code == (int)ResultEnum.success) refStr = "success";
            }
            #endregion
            return refStr;
        }
        #endregion

        #region 物流信息
        /// <summary>
        /// 物流信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> ExpressInfo(LogisticsView view) => await _logisticsService.ExpressInfo(view, user);

        #endregion

        #region 订单退款回调

        /// <summary>
        /// 订单微信回调
        /// </summary>
        [AllowAnonymous]
        [AcceptVerbs("GET", "POST")]
        public string WxRefundNotifyUrl()
        {
            string refStr = "FAIL";

            string PayOrderNo;
            string xml;
            bool f = Service.WeChat.TenPayClient.PayNotifyUrl(HttpContext, out PayOrderNo, out xml);
            #region 退款成功业务处理
            if (f) refStr = xml;
            #endregion

            return refStr;
        }
        /// <summary>
        /// 支付宝回调
        /// </summary>
        [AllowAnonymous]
        [AcceptVerbs("GET", "POST")]
        public string AliPayRefundNotifyUrl()
        {
            string refStr = "fail";
            string PayOrderNo;
            bool f = Service.AliPay.AliPayClient.AliPayNotifyUrl(HttpContext, out PayOrderNo);
            #region 退款成功业务处理
            if (f) refStr = "success";

            #endregion
            return refStr;
        }
        #endregion



        #region 任务订单
        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<ResultModel> TaskList(QueryModel view) => await _taskOrderService.FrontEndTaskList(view);

        /// <summary>
        /// 详情
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<ResultModel> TaskDetails(QueryModel view) => await _taskOrderService.FrontEndTaskDetails(view, user);

        /// <summary>
        /// 任务订单微信回调
        /// </summary>
        [AllowAnonymous]
        [AcceptVerbs("GET", "POST")]
        public async Task<string> WxTaskOrderNotifyUrl()
        {
            string refStr = "FAIL";

            string PayOrderNo;
            string xml;
            bool f = Service.WeChat.TenPayClient.PayNotifyUrl(HttpContext, out PayOrderNo, out xml);
            #region 支付成功业务处理
            if (f)
            {
                var res = await _taskOrderService.OrderCallback(PayOrderNo);
                if (res != null && res.code == (int)ResultEnum.success) refStr = xml;
            }
            #endregion

            return refStr;
        }

        /// <summary>
        /// 任务订单申请用户列表
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public async Task<ResultModel> TaskApplyUserList(OrderTaskApplyQuery view) => await _taskOrderService.TaskApplyMemberList(view, user);


        #region 用户端
        /// <summary>
        /// 用户的任务订单列表
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public async Task<ResultModel> TaskOrderList(TaskOrderQuery view) => await _taskOrderService.FrontEndList(view, user);


        /// <summary>
        /// 申请任务
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public async Task<ResultModel> ApplyTask(TaskOrder view) => await _taskOrderService.ApplyTask(view, user);


        /// <summary>
        /// 取消申请
        /// </summary>
        /// <param name="view"></param>        
        /// <returns></returns>
        public async Task<ResultModel> CancelTaskApply(TaskOrder view) => await _taskOrderService.CancelTaskApply(view, user);

        /// <summary>
        /// 任务发货
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public async Task<ResultModel> TaskDelivery(TaskOrder view) => await _taskOrderService.TaskDelivery(view, user);

        #endregion

        #region 店铺端
        /// <summary>
        /// 发布任务
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public async Task<ResultModel> CreateTask(TaskOrderView view) => await _taskOrderService.FrontEndCreate(view, user);

        /// <summary>
        /// 取消任务
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public async Task<ResultModel> CancelTask(TaskOrder view) => await _taskOrderService.FrontEndCancelTask(view, user);
        /// <summary>
        /// 店铺端任务订单列表
        /// </summary>
        /// <param name="view"></param>        
        /// <returns></returns>
        public async Task<ResultModel> StoreTaskOrderList(TaskOrderQuery view) => await _taskOrderService.FrontEndStoreList(view, user);

        /// <summary>
        /// 接受申请
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public async Task<ResultModel> AcceptTaskApply(OrderTaskApply view) => await _taskOrderService.AcceptTaskApply(view, user);

        [EnableRateLimiting("UserPayOrder")]
        public async Task<ResultModel> TaskOrderAgainPay(OrderPayView view) => await _taskOrderService.OrderAgainPay(view, user);

        /// <summary>
        /// 确认收货
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public async Task<ResultModel> TaskConfirmReceipt(TaskOrder view) => await _taskOrderService.TaskConfirmReceipt(view, user);

        #endregion


        #endregion



    }
}
