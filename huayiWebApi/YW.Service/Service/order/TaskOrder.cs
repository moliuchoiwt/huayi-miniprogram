using System.Threading;

namespace YW.Service
{
    public interface ITaskOrderService : IBaseRepository<TaskOrder>
    {
        /// <summary>
        /// 创建订单
        /// </summary>
        Task<ResultModel> FrontEndCreate(TaskOrder view, UserInfo user);
        Task<ResultModel> OrderCallback(string payNo);
        Task<ResultModel> FrontEndTaskList(QueryModel view);
        Task<ResultModel> FrontEndTaskDetails(QueryModel view, UserInfo user);
        Task<ResultModel> TaskApplyMemberList(OrderTaskApplyQuery view, UserInfo user);

        /// <summary>
        /// 用户端订单列表
        /// </summary>
        /// <param name="view"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<ResultModel> FrontEndList(TaskOrderQuery view, UserInfo user);
        /// <summary>
        /// 店铺端订单列表
        /// </summary>
        /// <param name="view"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<ResultModel> FrontEndStoreList(TaskOrderQuery view, UserInfo user);
        Task<ResultModel> CancelTaskApply(TaskOrder view, UserInfo user);
        Task<ResultModel> ApplyTask(TaskOrder view, UserInfo user);
        Task<ResultModel> FrontEndCancelTask(TaskOrder view, UserInfo user);
        Task<ResultModel> BackEndEndList(TaskOrderQuery view);
        /// <summary>
        /// 线下支付
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ResultModel> OfflinePayment(TaskOrderView model);
        Task<ResultModel> AcceptTaskApply(OrderTaskApply view, UserInfo user);
        Task<ResultModel> BackEndOperation(TaskOrderView model);
        Task<ResultModel> OrderAgainPay(OrderPayView view, UserInfo user);
        Task<ResultModel> TaskDelivery(TaskOrder view, UserInfo user);
        Task<ResultModel> TaskConfirmReceipt(TaskOrder view, UserInfo user);
        Task<ResultModel> ConfirmRefund(TaskOrderView model);

        /// <summary>
        /// 定时任务
        /// </summary>
        Task TimedTaskFun();

    }
    public partial class TaskOrderService : BaseRepository<TaskOrder>, ITaskOrderService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TaskOrderMapper _mapper = new();
        private readonly OrderTaskApplyMapper _taskApplyMapper = new();

        public TaskOrderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        #region 列表
        public async Task<ResultModel> FrontEndTaskList(QueryModel view)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<TaskOrder>(a => a.status == (int)TaskOrderStateEnum.已发布);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                exWhere.And(it => it.relatedDemand.Contains(view.queryName));
            }
            var data = await base.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            var list = new List<TaskOrderView>();
            if (data != null && data.Count > 0)
            {
                var sIds = data.Select(it => it.shopId).Distinct().ToList();
                var sList = await ShopDb.GetListAsync(it => sIds.Contains(it.Id));

                list = _mapper.ToViewList(data);

                foreach (var item in list)
                {
                    item.goodsImgList = WebFileHelper.GetListUrl(item.goodsImgs);
                    if (sList.Count(it => it.Id == item.shopId) > 0)
                    {
                        var sInfo = sList.Find(it => it.Id == item.shopId);
                        item.shopName = sInfo.name;
                    }
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }

        public async Task<ResultModel> FrontEndList(TaskOrderQuery view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<OrderTaskApply>(a => a.status != (int)TaskOrderApplyStateEnum.已删除 && a.userId == user.Id);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                int tId = 0;
                int.TryParse(view.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.orderNo.Contains(view.queryName));

            }
            if (view.status.HasValue) exWhere.And(a => a.status == view.status.Value);
            if (view.queryState.HasValue) exWhere.And(a => a.status == view.queryState.Value);
            if (view.startTime.HasValue) exWhere.And(a => a.createTime >= view.startTime.Value);
            if (view.endTime.HasValue) exWhere.And(a => a.createTime <= view.endTime.Value);

            var list = new List<OrderTaskApplyView>();
            var data = await OrderTaskApplyDb.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            if (data != null && data.Count > 0)
            {
                var orderNos = data.Select(it => it.orderNo).ToList();
                var orderList = await base.GetListAsync(it => SqlFunc.ContainsArray(orderNos, it.orderNo));

                list = _taskApplyMapper.ToViewList(data);

                foreach (var item in list)
                {
                    if (orderList.Count(it => it.orderNo == item.orderNo) > 0)
                    {
                        var order = orderList.Find(it => it.orderNo == item.orderNo);
                        item.goodsImgList = WebFileHelper.GetListUrl(order.goodsImgs);
                        item.price = order.price;
                        item.relatedDemand = order.relatedDemand;
                        item.receivingTime = order.receivingTime;
                        item.orderId = order.Id;
                    }
                    item.statusName = Enum.GetName(typeof(TaskOrderApplyStateEnum), item.status);
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }

        public async Task<ResultModel> FrontEndStoreList(TaskOrderQuery view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            if (ShopDb.Count(it => it.auditState == 1 && it.status != 99 && it.userId == user.Id) == 0)
            {
                res.msg = "无权限";
                return res;
            }
            var shop = await ShopDb.GetFirstAsync(it => it.userId == user.Id && it.auditState == 1 && it.status != 99);
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<TaskOrder>(a => a.status != (int)TaskOrderStateEnum.已删除 && a.shopId == shop.Id);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                int tId = 0;
                int.TryParse(view.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.orderNo.Contains(view.queryName)
               || a.payNo.Contains(view.queryName)
               || a.payMent.Contains(view.queryName) || a.remarks.Contains(view.queryName));

            }
            if (view.status.HasValue) exWhere.And(a => a.status == view.status.Value);
            if (view.queryState.HasValue) exWhere.And(a => a.status == view.queryState.Value);

            //if (view.queryStateArr != null && view.queryStateArr.Count > 0) exWhere.And(a => SqlFunc.ContainsArray(view.queryStateArr, a.status));
            if (view.startTime.HasValue) exWhere.And(a => a.createTime >= view.startTime.Value);
            if (view.endTime.HasValue) exWhere.And(a => a.createTime <= view.endTime.Value);

            var data = await base.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            var list = new List<TaskOrderView>();
            if (data != null && data.Count > 0)
            {
                list = _mapper.ToViewList(data);

                foreach (var item in list)
                {
                    item.goodsImgList = WebFileHelper.GetListUrl(item.goodsImgs);
                    item.receivingTypeName = Enum.GetName(typeof(TaskOrderReceivingTypeEnum), item.receivingType);
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }

        public async Task<ResultModel> BackEndEndList(TaskOrderQuery view)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<TaskOrder>(a => a.status != (int)TaskOrderStateEnum.已删除);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                int tId = 0;
                int.TryParse(view.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.orderNo.Contains(view.queryName)
               || a.payNo.Contains(view.queryName)
               || a.payMent.Contains(view.queryName) || a.remarks.Contains(view.queryName));

            }
            if (view.status.HasValue) exWhere.And(a => a.status == view.status.Value);
            if (view.startDate.HasValue)
            {
                var startTime = view.startDate.Value.ToDateTime(new TimeOnly(0, 0, 0));
                exWhere.And(a => a.createTime >= startTime);
            }
            if (view.endDate.HasValue)
            {
                var endTime = view.endDate.Value.ToDateTime(new TimeOnly(23, 59, 59));
                exWhere.And(a => a.createTime <= endTime);
            }
            //if (view.startTime.HasValue) exWhere.And(a => a.createTime >= view.startTime.Value);
            //if (view.endTime.HasValue) exWhere.And(a => a.createTime <= view.endTime.Value);

            var data = await base.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            var list = new List<TaskOrderView>();
            if (data != null && data.Count > 0)
            {
                var uIds = data.Select(it => it.userId).Distinct().ToList();
                var uList = await UserInfoDb.GetListAsync(a => SqlFunc.ContainsArray(uIds, a.Id));

                var sIds = data.Select(it => it.shopId).Distinct().ToList();
                var sList = await ShopDb.GetListAsync(a => SqlFunc.ContainsArray(sIds, a.Id));

                list = _mapper.ToViewList(data);

                foreach (var item in list)
                {
                    item.goodsImgList = WebFileHelper.GetListUrl(item.goodsImgs);
                    item.receivingTypeName = Enum.GetName(typeof(TaskOrderReceivingTypeEnum), item.receivingType);
                    if (uList.Count(it => it.Id == item.userId) > 0)
                    {
                        var uInfo = uList.Find(it => it.Id == item.userId);
                        item.userName = uInfo.nickName;
                    }
                    if (sList.Count(it => it.Id == item.shopId) > 0)
                    {
                        var sInfo = sList.Find(it => it.Id == item.shopId);
                        item.shopName = sInfo.name;
                    }
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }

        #endregion

        #region 详情
        public async Task<ResultModel> FrontEndTaskDetails(QueryModel view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null || !view.queryId.HasValue) { res.msg = "参数错误"; return res; }
            //if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            var task = await base.GetByIdAsync(view.queryId.Value);
            if (task == null || task.status == (int)TaskOrderStateEnum.已删除) { res.msg = "任务不存在"; return res; }

            var goodsImgList = WebFileHelper.GetListUrl(task.goodsImgs);
            var shopName = "";
            var shopMobile = "";
            var shopProvince = "";
            var shopCity = "";
            var shopArea = "";
            var shopAddress = "";
            var shop = await ShopDb.GetByIdAsync(task.shopId);
            if (shop != null)
            {
                shopName = shop.name;
                shopMobile = shop.mobile;
                shopProvince = shop.province;
                shopCity = shop.city;
                shopArea = shop.area;
                shopAddress = shop.address;
            }
            //已有xx名用户申请接单
            var applyCount = await OrderTaskApplyDb.CountAsync(it => it.orderNo == task.orderNo && it.status != 99);

            var receivingTypeName = Enum.GetName(typeof(TaskOrderReceivingTypeEnum), task.receivingType);

            var isApply = false;
            if (user != null)
            {
                isApply = await OrderTaskApplyDb.CountAsync(it => it.orderNo == task.orderNo && it.userId == user.Id && it.status != 99) > 0;
            }

            //接单的用户信息
            var userName = "";
            var userMobile = "";
            if (task.userId > 0 && UserInfoDb.Count(it => it.Id == task.userId) > 0)
            {
                var uInfo = await UserInfoDb.GetByIdAsync(task.userId);
                userName = uInfo.nickName;
                userMobile = uInfo.mobile;
            }

            res.data = new
            {
                info = task,
                goodsImgList,
                shopName,
                shopMobile,
                shopProvince,
                shopCity,
                shopArea,
                shopAddress,
                applyCount,
                receivingTypeName,
                isApply,
                userName,
                userMobile
            };
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            return res;
        }


        #endregion

        #region 发布任务
        public async Task<ResultModel> FrontEndCreate(TaskOrder view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }

            if (ShopDb.Count(it => it.status != 99 && it.auditState == 1 && it.userId == user.Id) == 0)
            {
                res.msg = "无权限";
                return res;
            }
            var shop = await ShopDb.GetFirstAsync(it => it.userId == user.Id && it.auditState == 1 && it.status != 99);
            if (string.IsNullOrWhiteSpace(view.relatedDemand)) { res.msg = "请填写需求"; return res; }
            if (string.IsNullOrWhiteSpace(view.goodsImgs)) { res.msg = "请上传任务图片"; return res; }
            if (view.receivingTime <= DateTime.Now) { res.msg = "请填写收货时间"; return res; }
            if (string.IsNullOrWhiteSpace(view.province) || string.IsNullOrWhiteSpace(view.city) || string.IsNullOrWhiteSpace(view.area))
            {
                res.msg = "请选择省市区";
                return res;
            }
            if (string.IsNullOrWhiteSpace(view.address))
            {
                res.msg = "详细地址不能为空";
                return res;
            }
            if (view.price < PubConstant.Config.orderMinPrice) { res.msg = "价格不能小于" + PubConstant.Config.orderMinPrice; return res; }


            var currentDate = DateOnly.FromDateTime(DateTime.Now);
            var username = user.mobile;

            if (view.goodsImgs.Contains(PubConstant.Config.DomianStaticName)) view.goodsImgs = view.goodsImgs.Replace(PubConstant.Config.DomianStaticName, "");

            var orderno = CommonHelper.GenerateUniqueText("T");

            //订单信息
            var order = new TaskOrder
            {
                orderNo = orderno,
                price = view.price,//价格
                shopId = shop.Id,
                remarks = view.remarks, //备注
                goodsImgs = view.goodsImgs,
                status = (int)TaskOrderStateEnum.待付款,
                auditState = 0, // 需求3：任務內容待審核（人工審批，不可自動批准）
                receivingType = view.receivingType,//收货方式
                receivingTime = view.receivingTime,//收货时间
                mobile = shop.mobile, //收货人手机号
                consignee = shop.realName,//收货人姓名
                province = view.province,//省份
                city = view.city,//城市
                area = view.area,//区域
                address = view.address,//收货详细地址
                relatedDemand = view.relatedDemand //相关需求
            };

            order.payMent = "微信支付";
            order.payType = (int)PayEnum.微信JSAPI;
            order.payNo = Common.CommonHelper.Timestamp();

            if (order.price < 0)
            {
                order.price = 0;
                order.payMent = "无需支付";
                order.payType = (int)PayEnum.无需支付;
            }

            try
            {
                await db.BeginTranAsync();//开始事务

                //创建订单
                await base.InsertAsync(order);

                //支付订单
                if (order.price > 0)
                {
                    //生成微信支付
                    var openId = user.wxAppletsOpenId;
                    //生成微信支付
                    string notifyUrl = PubConstant.Config.DomianName + "/api/Order/WxTaskOrderNotifyUrl";
                    int totalint = (int)(order.price * 100);
                    var payObj = await WeChat.TenPayClient.TenPayByJsapi(openId, order.payNo, order.payNo, totalint, "", notifyUrl, 1);
                    if (payObj != null)
                    {
                        var payModel = payObj as WxJSAPIPay;
                        res.code = (int)ResultEnum.success;
                        res.msg = "微信生成支付订单成功";
                        res.data = new { IsPay = true, partnerid = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_MchId, wxData = payModel };
                    }
                    else
                    {
                        await db.RollbackTranAsync();//回滚事务
                        res.msg = "微信生成支付订单失败";
                        res.data = new { IsPay = false, PayOrderNo = order.payNo };
                    }
                }
                await db.CommitTranAsync();//提交事务

                // 需求3：上載訂單通知管理員（含圖片，需人工審批內容）
                _ = NotifyAdminForTaskPublish(order, shop);
            }
            catch (Exception ex)
            {
                await db.RollbackTranAsync();//回滚事务
                res.msg = ex.Message;
            }
            return res;
        }
        #endregion

        #region 郵件通知（需求3：上載訂單）

        private static async Task NotifyAdminForTaskPublish(TaskOrder order, Shop shop)
        {
            try
            {
                var adminEmail = ConfigHelper.GetSectionValue("EmailSetting:AdminTo") ?? "studioofjoyhk@gmail.com";
                var body = $@"
                    <h3>【華藝】新任務訂單待審核</h3>
                    <p>管理員你好，</p>
                    <p>商戶發布了一個新任務，<b>需人工審核內容</b>：</p>
                    <ul>
                        <li>訂單號：{order.orderNo}</li>
                        <li>商戶：{shop?.name}（ID:{shop?.Id}）</li>
                        <li>需求：{order.relatedDemand}</li>
                        <li>價格：{order.price}</li>
                        <li>收貨：{order.province}{order.city}{order.area} {order.address}</li>
                        <li>發布時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</li>
                    </ul>
                    <p>請前往後台審核任務內容，審核通過後個體戶方可承接。</p>
                ";
                // 含任務圖片附件
                byte[][] imgs = null;
                if (!string.IsNullOrEmpty(order.goodsImgs))
                {
                    var imgUrls = order.goodsImgs.Split(',').Where(u => !string.IsNullOrEmpty(u)).ToArray();
                    imgs = new byte[imgUrls.Length][];
                    for (int i = 0; i < imgUrls.Length; i++)
                    {
                        var abs = Path.Combine(Directory.GetCurrentDirectory(), imgUrls[i].TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                        if (File.Exists(abs)) imgs[i] = await File.ReadAllBytesAsync(abs);
                    }
                }
                var attachments = imgs?.Select((b, i) => ($"task_{i}.png", b)).Where(t => t.Item2 != null).ToArray();
                await EmailClient.SendAsync(adminEmail, $"【華藝】新任務待審核：{order.orderNo}", body, attachments ?? Array.Empty<(string, byte[])>());
            }
            catch { }
        }

        #endregion
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            if (ShopDb.Count(it => it.auditState == 1 && it.status != 99 && it.userId == user.Id) == 0)
            {
                res.msg = "无权限";
                return res;
            }
            var shop = await ShopDb.GetFirstAsync(it => it.userId == user.Id && it.auditState == 1 && it.status != 99);
            var order = await base.GetByIdAsync(view.Id);
            if (order == null)
            {
                res.msg = "订单不存在";
                return res;
            }
            if (order.status != (int)TaskOrderStateEnum.待付款 && order.status != (int)TaskOrderStateEnum.已发布 && order.status != (int)TaskOrderStateEnum.待审核)
            {
                res.msg = "订单状态不可用";
                return res;
            }
            var payNo = order.payNo;
            try
            {
                await db.BeginTranAsync();
                var isOk = await base.UpdateAsync(it => new TaskOrder { status = (int)TaskOrderStateEnum.已取消, shopRefundAmount = order.price, updateTime = DateTime.Now }, it => it.Id == order.Id);
                if (isOk)
                {
                    if (order.isPay && order.payType != (int)PayEnum.线下支付)
                    {
                        #region 微信退款
                        if (order.price > 0)
                        {
                            int total = (int)(order.price * 100);
                            int ordertotal = (int)(order.price * 100);
                            string notifyUrl = PubConstant.Config.DomianName + "/api/Order/WxRefundNotifyUrl";
                            var wxResult = WeChat.TenPayClient.Refund(_serviceProvider, payNo, total, ordertotal, 1, notifyUrl);
                            if (wxResult != "成功")
                            {
                                await db.Ado.RollbackTranAsync();
                                res.msg = $"微信退款失败:{wxResult}";
                                return res;
                            }
                        }
                        #endregion
                    }
                    await OrderTaskApplyDb.UpdateAsync(it => new OrderTaskApply { status = (int)TaskOrderApplyStateEnum.已删除, updateTime = DateTime.Now }, it => it.orderNo == order.orderNo && it.status == (int)TaskOrderApplyStateEnum.已申请);
                }
                await db.CommitTranAsync();
                res.msg = $"操作{(isOk ? "成功" : "失败")}";
                res.code = (int)ResultEnum.success;
            }
            catch (Exception ex)
            {
                await db.RollbackTranAsync();
                res.msg = ex.Message;
            }
            return res;
        }
        #endregion

        #region 订单回调
        public async Task<ResultModel> OrderCallback(string payNo)
        {
            var res = new ResultModel();
            if (string.IsNullOrWhiteSpace(payNo)) { res.msg = "支付单号参数不能为空"; return res; }
            var orderList = await base.GetListAsync(a => !a.isPay && a.status == (int)OrderStateEnum.待支付 && a.payNo == payNo);

            try
            {
                await db.BeginTranAsync();

                foreach (var order in orderList)
                {
                    #region 变更订单状态
                    order.isPay = true;
                    order.payTime = DateTime.Now;
                    order.updateTime = DateTime.Now;
                    order.status = (int)TaskOrderStateEnum.待审核;

                    await base.UpdateAsync(order);
                    #endregion
                }
                await db.CommitTranAsync();
            }
            catch (Exception ex)
            {
                await db.RollbackTranAsync();
                LogHelper.Error("订单回调查询订单错误", ex);
            }
            res.msg = "SUCCESS";
            res.code = (int)ResultEnum.success;
            return res;

        }
        #endregion

        #region 任务申请
        /// <summary>
        /// 成员列表
        /// </summary>
        /// <param name="view"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<ResultModel> TaskApplyMemberList(OrderTaskApplyQuery view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            if (string.IsNullOrWhiteSpace(view.orderNo)) { res.msg = "订单号不能为空:orderNo"; return res; }
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<OrderTaskApply>(it => it.orderNo == view.orderNo && it.status == 0);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                var o_uIds = await db.Queryable<OrderTaskApply>().Where(it => it.orderNo == view.orderNo && it.status == 0).Select(it => it.userId).ToListAsync();//获取所有申请任务订单的用户id
                var uIds = await db.Queryable<UserInfo>().Where(it => SqlFunc.ContainsArray(o_uIds, it.Id) && it.status == 0 && it.nickName.Contains(view.queryName)).Select(it => it.Id).ToListAsync();//获取所有昵称包含view.queryName的用户id
                exWhere.And(it => SqlFunc.ContainsArray(uIds, it.userId));
            }

            var list = new List<OrderTaskApplyView>();
            var data = await OrderTaskApplyDb.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            if (data != null && data.Count > 0)
            {
                var uIds = data.Select(it => it.userId).ToList();
                var uList = await UserInfoDb.GetListAsync(it => uIds.Contains(it.Id));

                list = _taskApplyMapper.ToViewList(data);

                foreach (var item in list)
                {
                    if (uList.Count(it => it.Id == item.userId) > 0)
                    {
                        var uInfo = uList.Find(it => it.Id == item.userId);
                        item.nickName = uInfo.nickName;
                        item.avatar = WebFileHelper.GetUrl(uInfo.avatar);
                        item.gender = uInfo.gender;
                        item.intro = uInfo.intro;
                        item.province = uInfo.province;
                        item.city = uInfo.city;
                        item.area = uInfo.area;
                    }
                }
            }

            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }

        /// <summary>
        /// 取消申请
        /// </summary>
        /// <param name="view"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<ResultModel> CancelTaskApply(TaskOrder view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (string.IsNullOrWhiteSpace(view.orderNo)) { res.msg = "订单号不能为空:orderNo"; return res; }
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }

            var isOk = await OrderTaskApplyDb.UpdateAsync(it => new OrderTaskApply { status = 99, updateTime = DateTime.Now }, it => it.orderNo == view.orderNo && it.userId == user.Id && it.status == 0);
            res.code = isOk ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "取消申请" + (isOk ? "成功" : "失败");
            return res;
        }

        public async Task<ResultModel> ApplyTask(TaskOrder view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (string.IsNullOrWhiteSpace(view.orderNo)) { res.msg = "订单号不能为空:orderNo"; return res; }
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            var order = await base.GetSingleAsync(it => it.orderNo == view.orderNo);
            if (order == null)
            {
                res.msg = "订单不存在";
                return res;
            }
            if (order.status != (int)TaskOrderStateEnum.已发布)
            {
                res.msg = "任务已被接取";
                return res;
            }
            if (OrderTaskApplyDb.Count(it => it.orderNo == order.orderNo && it.userId == user.Id && it.status == 0) > 0)
            {
                res.msg = "您已申请过此任务";
                return res;
            }
            var isOk = await OrderTaskApplyDb.InsertAsync(new OrderTaskApply
            {
                orderNo = order.orderNo,
                userId = user.Id,
                createTime = DateTime.Now,
                updateTime = DateTime.Now,
                status = 0
            });
            res.code = isOk ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "申请" + (isOk ? "成功" : "失败");
            return res;
        }

        #endregion

        #region 接受任务申请
        public async Task<ResultModel> AcceptTaskApply(OrderTaskApply view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            if (ShopDb.Count(it => it.status != 99 && it.auditState == 1 && it.userId == user.Id) == 0)
            {
                res.msg = "无权限";
                return res;
            }
            var shop = await ShopDb.GetFirstAsync(it => it.userId == user.Id && it.auditState == 1 && it.status != 99);
            var apply = await OrderTaskApplyDb.GetByIdAsync(view.Id);
            if (apply == null || apply.status != 0)
            {
                res.msg = "任务申请不存在或已失效";
                return res;
            }
            var apply_user = await UserInfoDb.GetByIdAsync(apply.userId);
            if (apply_user == null || apply_user.status != 0)
            {
                res.msg = "用户不存在或已注销";
                return res;
            }
            var order = await base.GetSingleAsync(it => it.orderNo == apply.orderNo);
            if (order == null)
            {
                res.msg = "订单不存在";
                return res;
            }
            if (order.shopId != shop.Id)
            {
                res.msg = "无此权限";
                return res;
            }
            if (order.status != (int)TaskOrderStateEnum.已发布)
            {
                res.msg = "任务已被接取";
                return res;
            }

            try
            {
                await db.BeginTranAsync();
                var isOk = await OrderTaskApplyDb.UpdateAsync(it => new OrderTaskApply { status = (int)TaskOrderApplyStateEnum.进行中, updateTime = DateTime.Now }, it => it.Id == apply.Id && it.status == (int)TaskOrderApplyStateEnum.已申请);
                if (isOk)
                {
                    await base.UpdateAsync(a => new TaskOrder { status = (int)TaskOrderStateEnum.进行中, userId = apply.userId, updateTime = DateTime.Now }, a => a.Id == order.Id && a.status == (int)TaskOrderStateEnum.已发布);
                    await OrderTaskApplyDb.UpdateAsync(it => new OrderTaskApply { status = (int)TaskOrderApplyStateEnum.已删除, updateTime = DateTime.Now }, it => it.orderNo == order.orderNo && it.Id != apply.Id);
                }
                await db.CommitTranAsync();
                res.code = isOk ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "接取" + (isOk ? "成功" : "失败");
            }
            catch (Exception ex)
            {
                await db.RollbackTranAsync();
                res.msg = ex.Message;
            }
            return res;
        }
        #endregion

        #region 线下支付
        public async Task<ResultModel> OfflinePayment(TaskOrderView model)
        {
            var res = new ResultModel();
            if (base.Count(it => it.Id == model.Id && !it.isPay) == 0)
            {
                res.msg = "订单不存在或已支付";
                return res;
            }

            var order = await base.GetByIdAsync(model.Id);
            var payOrderNo = Common.CommonHelper.Timestamp();

            if (await base.UpdateAsync(a => new TaskOrder { payNo = payOrderNo, payType = (int)PayEnum.线下支付, payMent = "线下支付", updateTime = DateTime.Now }, a => a.Id == order.Id))
            {
                res = await OrderCallback(payOrderNo);
            }
            else
            {
                res.msg = "支付失败";
                return res;
            }
            return res;
        }

        #endregion

        #region 订单支付
        public async Task<ResultModel> OrderAgainPay(OrderPayView view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (ShopDb.Count(it => it.status != 99 && it.auditState == 1 && it.userId == user.Id) == 0)
            {
                res.msg = "无权限";
                return res;
            }
            var shop = await ShopDb.GetFirstAsync(it => it.userId == user.Id && it.auditState == 1 && it.status != 99);
            var orderNoarr = view.orderNo.Split(',').ToList();
            var orderList = await base.GetListAsync(a => a.status != 99 && a.shopId == shop.Id && a.status == (int)OrderStateEnum.待支付 && SqlFunc.ContainsArray(orderNoarr, a.orderNo));
            if (orderList == null || orderList.Count <= 0) { res.msg = "参数错误,订单不存在"; return res; }
            var payOrderNo = Common.CommonHelper.Timestamp();
            //实际支付金额 余额支付,积分数
            decimal sumAmount = orderList.Sum(a => a.price);

            orderList.ForEach(item => item.payNo = payOrderNo);
            var orderIds = orderList.Select(a => a.Id).ToList();
            var payMent = "微信支付";
            if (sumAmount <= 0)
            {
                view.payType = (int)PayEnum.无需支付;
                payMent = "无需支付";
            }
            if (!await base.UpdateAsync(a => new TaskOrder { payNo = payOrderNo, payType = view.payType, payMent = payMent, updateTime = DateTime.Now }, a => SqlFunc.ContainsArray(orderIds, a.Id)))
            {
                res.msg = "支付失败";
                return res;
            }
            if (sumAmount <= 0)
            {
                res = await OrderCallback(payOrderNo);
                res.data = new { IsPay = false };
            }
            else
            {
                var openId = user.wxAppletsOpenId;
                //生成微信支付
                string notifyUrl = PubConstant.Config.DomianName + "/api/Order/WxTaskOrderNotifyUrl";
                int totalint = (int)(sumAmount * 100);
                var payObj = await WeChat.TenPayClient.TenPayByJsapi(openId, payOrderNo, payOrderNo, totalint, "", notifyUrl, 1);
                if (payObj != null)
                {
                    var payModel = payObj as WxJSAPIPay;
                    res.code = (int)ResultEnum.success;
                    res.msg = "微信生成支付订单成功";
                    res.data = new { IsPay = true, partnerid = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_MchId, wxData = payModel };
                }
                else
                {
                    res.msg = "微信生成支付订单失败";
                    res.data = new { IsPay = false, PayOrderNo = payOrderNo };
                }
            }
            return res;
        }
        #endregion

        #region 编辑
        public async Task<ResultModel> BackEndOperation(TaskOrderView model)
        {
            var res = new ResultModel();
            if (model == null) { res.msg = "参数错误"; return res; }
            if (model.Id > 0 && base.Count(it => it.Id == model.Id) == 0)
            {
                res.msg = "参数错误,无此订单";
                return res;
            }
            var info = _mapper.ToModel(model);
            bool isok = false;
            try
            {
                await db.CommitTranAsync();
                if (info.Id > 0)
                {
                    var oldInfo = await base.GetByIdAsync(info.Id);

                    isok = await base.UpdateAsync(it => new TaskOrder
                    {
                        status = info.status,
                        updateTime = info.updateTime
                    }, it => it.Id == info.Id);
                    if (isok && oldInfo.status == (int)TaskOrderStateEnum.待审核 && info.status == (int)TaskOrderStateEnum.已驳回 && oldInfo.isPay)
                    {
                        if (oldInfo.payType == (int)PayEnum.微信JSAPI)
                        {
                            #region 微信退款
                            if (oldInfo.price > 0)
                            {
                                int total = (int)(oldInfo.price * 100);
                                int ordertotal = (int)(oldInfo.price * 100);
                                string notifyUrl = PubConstant.Config.DomianName + "/api/Order/WxRefundNotifyUrl";
                                var wxResult = WeChat.TenPayClient.Refund(_serviceProvider, oldInfo.payNo, total, ordertotal, 1, notifyUrl);
                                if (wxResult != "成功")
                                {
                                    await db.Ado.RollbackTranAsync();
                                    res.msg = $"微信退款失败:{wxResult}";
                                    return res;
                                }
                            }
                            #endregion
                        }
                    }
                }
                else
                {

                    info.createTime = DateTime.Now;

                    info.updateTime = DateTime.Now;

                    //isok = await _taskOrderService.InsertAsync(info);
                }
                await db.CommitTranAsync();
                res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "操作" + (isok ? "成功" : "失败");
            }
            catch (Exception ex)
            {
                await db.RollbackTranAsync();
                res.msg = ex.Message;
            }

            return res;
        }
        #endregion

        #region 任务发货
        public async Task<ResultModel> TaskDelivery(TaskOrder view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            if (string.IsNullOrWhiteSpace(view.orderNo)) { res.msg = "订单号不能为空:orderNo"; return res; }
            var order = await base.GetSingleAsync(it => it.orderNo == view.orderNo && it.userId == user.Id);
            if (order == null)
            {
                res.msg = "订单不存在";
                return res;
            }
            if (order.status != (int)TaskOrderStateEnum.进行中)
            {
                res.msg = "订单状态不可用";
                return res;
            }
            try
            {
                await db.BeginTranAsync();
                var isOk = await base.UpdateAsync(it => new TaskOrder
                {
                    deliveryType = view.deliveryType,
                    expressCode = view.deliveryType == 0 ? view.expressCode : "",
                    expressName = view.deliveryType == 0 ? view.expressName : "",
                    logisticsNo = view.deliveryType == 0 ? view.logisticsNo : "",
                    status = (int)TaskOrderStateEnum.待收货,
                    updateTime = DateTime.Now,
                    deliveryTime = DateTime.Now
                }, it => it.Id == order.Id && it.status == (int)TaskOrderStateEnum.进行中);
                if (isOk)
                {
                    await OrderTaskApplyDb.UpdateAsync(it => new OrderTaskApply
                    {
                        status = (int)TaskOrderApplyStateEnum.待确认,
                        updateTime = DateTime.Now
                    }, it => it.orderNo == order.orderNo && it.userId == user.Id && it.status == (int)TaskOrderApplyStateEnum.进行中);
                }
                await db.CommitTranAsync();
                res.code = isOk ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "操作" + (isOk ? "成功" : "失败");
                res.data = isOk;
            }
            catch (Exception ex)
            {
                await db.RollbackTranAsync();
                res.msg = ex.Message;
            }
            return res;
        }
        #endregion

        #region 订单收货
        public async Task<ResultModel> TaskConfirmReceipt(TaskOrder view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            if (string.IsNullOrWhiteSpace(view.orderNo)) { res.msg = "订单号不能为空:orderNo"; return res; }
            if (ShopDb.Count(it => it.status != 99 && it.auditState == 1 && it.userId == user.Id) == 0)
            {
                res.msg = "无权限";
                return res;
            }
            var shop = await ShopDb.GetFirstAsync(it => it.userId == user.Id && it.auditState == 1 && it.status != 99);
            var order = await base.GetSingleAsync(it => it.orderNo == view.orderNo && it.shopId == shop.Id);
            if (order == null)
            {
                res.msg = "订单不存在";
                return res;
            }
            if (order.status != (int)TaskOrderStateEnum.待收货)
            {
                res.msg = "订单状态不可用";
                return res;
            }
            res = await OrderReceiving(new List<string> { order.orderNo });
            return res;
        }
        private async Task<ResultModel> OrderReceiving(List<string> orderNoArr)
        {
            var res = new ResultModel();
            if (orderNoArr == null || orderNoArr.Count <= 0) { res.msg = "订单单号参数不能为空"; return res; }
            var orderList = await base.GetListAsync(it => SqlFunc.ContainsArray(orderNoArr, it.orderNo) && it.status == (int)TaskOrderStateEnum.待收货);
            if (orderList != null && orderList.Count > 0)
            {
                foreach (var order in orderList)
                {
                    try
                    {
                        await db.BeginTranAsync();
                        var isOk = await base.UpdateAsync(it => new TaskOrder
                        {
                            status = (int)TaskOrderStateEnum.已完成,
                            updateTime = DateTime.Now
                        }, it => it.Id == order.Id && it.status == (int)TaskOrderStateEnum.待收货);
                        if (isOk)
                        {
                            await OrderTaskApplyDb.UpdateAsync(it => new OrderTaskApply
                            {
                                status = (int)TaskOrderApplyStateEnum.已完成,
                                updateTime = DateTime.Now
                            }, it => it.orderNo == order.orderNo && it.userId == order.userId && it.status == (int)TaskOrderApplyStateEnum.待确认);
                            if (order.price >= 1 && order.userId > 0)
                            {
                                var sendAmount = order.price * (1 - PubConstant.Config.PlatformProportion);
                                //分给用户
                                var uInfo = await UserInfoDb.GetByIdAsync(order.userId);
                                if (uInfo != null)
                                {
                                    var wlog = GetWalletLog(walletUserTypeEnum.用户, walletTypeEnum.余额, sourceTypeEnum.任务奖励, uInfo.Id, "任务订单完成奖励", sendAmount, order.orderNo, "", order.shopId);
                                    await WalletLogDb.InsertAsync(wlog);
                                    uInfo.amount += sendAmount;
                                    await UserInfoDb.UpdateAsync(it => new UserInfo { amount = uInfo.amount, updateTime = DateTime.Now }, it => it.Id == uInfo.Id);
                                }
                            }
                        }
                        await db.CommitTranAsync();
                        res.code = isOk ? (int)ResultEnum.success : (int)ResultEnum.fail;
                        res.msg = "操作" + (isOk ? "成功" : "失败");
                        res.data = isOk;
                    }
                    catch (Exception ex)
                    {
                        await db.RollbackTranAsync();
                        res.msg = ex.Message;
                    }
                }
            }
            return res;
        }
        #endregion

        #region 确认退款
        public async Task<ResultModel> ConfirmRefund(TaskOrderView model)
        {
            var res = new ResultModel();
            if (model == null || model.Id <= 0) { res.msg = "参数错误"; return res; }
            var order = await base.GetByIdAsync(model.Id);
            if (order == null)
            {
                res.msg = "订单不存在";
                return res;
            }
            if (order.status != (int)TaskOrderStateEnum.已发布 && order.status != (int)TaskOrderStateEnum.进行中 && order.status != (int)TaskOrderStateEnum.待收货)
            {
                res.msg = "订单状态不可用";
                return res;
            }

            if (order.status == (int)TaskOrderStateEnum.已发布)
            {
                var payNo = order.payNo;
                try
                {
                    await db.BeginTranAsync();
                    var isOk = await base.UpdateAsync(it => new TaskOrder { status = (int)TaskOrderStateEnum.售后完成, shopRefundAmount = order.price, updateTime = DateTime.Now }, it => it.Id == order.Id);
                    if (isOk)
                    {
                        if (order.isPay && order.payType != (int)PayEnum.线下支付)
                        {
                            #region 微信退款
                            if (order.price > 0)
                            {
                                int total = (int)(order.price * 100);
                                int ordertotal = (int)(order.price * 100);
                                string notifyUrl = PubConstant.Config.DomianName + "/api/Order/WxRefundNotifyUrl";
                                var wxResult = WeChat.TenPayClient.Refund(_serviceProvider, payNo, total, ordertotal, 1, notifyUrl);
                                if (wxResult != "成功")
                                {
                                    await db.Ado.RollbackTranAsync();
                                    res.msg = $"微信退款失败:{wxResult}";
                                    return res;
                                }
                            }
                            #endregion
                        }
                        await OrderTaskApplyDb.UpdateAsync(it => new OrderTaskApply { status = (int)TaskOrderApplyStateEnum.已删除, updateTime = DateTime.Now }, it => it.orderNo == order.orderNo && it.status == (int)TaskOrderApplyStateEnum.已申请);
                    }
                    await db.CommitTranAsync();
                    res.msg = $"操作{(isOk ? "成功" : "失败")}";
                    res.code = (int)ResultEnum.success;
                }
                catch (Exception ex)
                {
                    await db.RollbackTranAsync();
                    res.msg = ex.Message;
                }
            }
            else if (order.status == (int)TaskOrderStateEnum.进行中 || order.status == (int)TaskOrderStateEnum.待收货)
            {
                //if (order.price < (model.userRefundAmount + model.shopRefundAmount))
                //{
                //    res.msg = "退款金额大于任务订单金额";
                //    return res;
                //}
                var shop = await ShopDb.GetByIdAsync(order.shopId);
                try
                {
                    await db.BeginTranAsync();

                    #region 退款给用户
                    var user = await UserInfoDb.GetByIdAsync(order.userId);
                    if (user != null)
                    {
                        var money = model.userRefundAmount;
                        var wlog = GetWalletLog(walletUserTypeEnum.用户, walletTypeEnum.余额, sourceTypeEnum.订单退款, user.Id, "订单退款", money, order.orderNo, "", order.shopId);
                        user.amount += money;
                        await UserInfoDb.UpdateAsync(it => new UserInfo { amount = user.amount, updateTime = DateTime.Now }, it => it.Id == user.Id);
                        await WalletLogDb.InsertAsync(wlog);
                    }
                    #endregion

                    #region 退款给商家
                    if (shop != null)
                    {
                        var s_user = await UserInfoDb.GetByIdAsync(shop.userId);
                        if (s_user != null)
                        {
                            var money = model.shopRefundAmount;
                            var wlog = GetWalletLog(walletUserTypeEnum.用户, walletTypeEnum.余额, sourceTypeEnum.订单退款, s_user.Id, "订单退款", money, order.orderNo, "", order.shopId);
                            s_user.amount += money;
                            await UserInfoDb.UpdateAsync(it => new UserInfo { amount = s_user.amount, updateTime = DateTime.Now }, it => it.Id == s_user.Id);
                            await WalletLogDb.InsertAsync(wlog);
                        }
                    }
                    #endregion

                    //更改订单状态
                    var isOk = await base.UpdateAsync(it => new TaskOrder { status = (int)TaskOrderStateEnum.售后完成, userRefundAmount = model.userRefundAmount, shopRefundAmount = model.shopRefundAmount, updateTime = DateTime.Now }, it => it.Id == order.Id);
                    if (isOk)
                    {
                        await OrderTaskApplyDb.UpdateAsync(it => new OrderTaskApply { status = (int)TaskOrderApplyStateEnum.售后完成, updateTime = DateTime.Now }, it => it.orderNo == order.orderNo && it.status == (int)TaskOrderApplyStateEnum.进行中 && it.userId == order.userId);
                    }
                    await db.CommitTranAsync();
                    res.code = isOk ? (int)ResultEnum.success : (int)ResultEnum.fail;
                    res.msg = "操作" + (isOk ? "成功" : "失败");
                }
                catch (Exception ex)
                {
                    await db.RollbackTranAsync();
                    res.msg = ex.Message;
                }
            }
            return res;
        }
        #endregion
    }


    public partial class TaskOrderService
    {
        #region 定时任务
        private readonly SemaphoreSlim _asyncLock = new(1, 1);
        public async Task TimedTaskFun()
        {
            await _asyncLock.WaitAsync(); // 异步等待锁
            try
            {
                #region  任务订单7天未收货，自动收货
                var date_7day = DateTime.Now.AddDays(-7);
                if (base.Count(a => a.status == (int)OrderStateEnum.待收货 && a.deliveryTime < date_7day) > 0)
                {
                    try
                    {
                        var noArr = await db.Queryable<TaskOrder>().Where(a => a.status == (int)TaskOrderStateEnum.待收货 && a.deliveryTime < date_7day).Select(it => it.orderNo).ToListAsync();
                        var res = await OrderReceiving(noArr);
                        if (res.code != (int)ResultEnum.success)
                        {
                            LogHelper.Error("任务订单7天未收货，自动收货处理错误:" + res.msg);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Error("任务订单7天未收货，自动收货处理错误", ex);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                LogHelper.Error("订单定时任务出错：", ex);
            }
            finally
            {
                _asyncLock.Release(); // 释放锁
            }
        }


        #endregion

    }
}