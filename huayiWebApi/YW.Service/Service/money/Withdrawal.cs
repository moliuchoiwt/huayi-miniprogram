using YW.Service.WeChat;

namespace YW.Service
{
    public partial interface IWithdrawalService : IBaseRepository<Withdrawal>
    {

        Task<ResultModel> ApplyForWithdrawal(WithdrawalView model, UserInfo user);

        Task<ResultModel> AuditWithdrawal(AuditWithdrawView view);

        Task<ResultModel> FrontEndList(QueryModel view, UserInfo user);

        Task<ResultModel> BackEndList(QueryModel view);
    }
    //提现记录

    public partial class WithdrawalService : BaseRepository<Withdrawal>, IWithdrawalService
    {
        private readonly WithdrawalMapper _mapper = new();

        #region 列表
        public async Task<ResultModel> FrontEndList(QueryModel view, UserInfo user)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };

            var exWhere = PredicateBuilder.New<Withdrawal>(a => a.userType == 0 && a.status != 99 && a.userId == user.Id);

            //查询未领取的提现记录并查询微信转账状态进行更新数据
            var notReceivedList = await base.GetListAsync(it => it.userId == user.Id && it.status == 1 && !SqlFunc.IsNullOrEmpty(it.withdrawalNo));
            if (notReceivedList != null && notReceivedList.Count > 0)
            {
                foreach (var item in notReceivedList)
                {
                    var wxResult = await TenPayClient.TransferBillQueryByOutBillNo(item.withdrawalNo);
                    if (wxResult.ResultCode.Success)
                    {
                        try
                        {
                            await db.BeginTranAsync();//开始事务
                            if (wxResult.state == "SUCCESS") await base.UpdateAsync(it => new Withdrawal { status = 3, updateTime = DateTime.Now }, it => it.Id == item.Id);
                            else if (wxResult.state == "FAIL")
                            {
                                //转账失败退回用户余额
                                await WalletLogDb.InsertAsync(new WalletLog
                                {
                                    change = item.amount,
                                    createTime = DateTime.Now,
                                    orderNo = item.withdrawalNo,
                                    sourceType = (int)sourceTypeEnum.提现,
                                    title = "提现失败",
                                    wType = (int)walletTypeEnum.余额,
                                    updateTime = DateTime.Now,
                                    userId = item.userId,
                                    userType = 0,
                                    remark = "用户未领取,自动退回到账户."
                                });
                                user.amount += item.amount;
                                await UserInfoDb.UpdateAsync(a => new UserInfo { amount = user.amount, updateTime = DateTime.Now }, a => a.Id == user.Id);
                                await base.UpdateAsync(it => new Withdrawal { status = 4, updateTime = DateTime.Now }, it => it.Id == item.Id);
                            }
                            await db.CommitTranAsync();//提交事务
                        }
                        catch (Exception ex)
                        {
                            await db.RollbackTranAsync();//回滚事务
                            LogHelper.Error("微信查询转账状态调整用户账户失败：", ex);
                        }

                    }
                    else
                    {
                        LogHelper.Info($"微信查询转账单号失败,{item.withdrawalNo},错误信息：{wxResult.ResultCode.ErrorMessage}");
                    }
                }
            }





            var data = await base.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            //var list = new List<WithdrawalDto>();
            //if (data != null && data.Count > 0)
            //{
            //    var ylist = data.GroupBy(a => new { Year = a.CreateTime.Year, Month = a.CreateTime.Month }).
            //        Select(a => new { a.Key.Year, a.Key.Month }).OrderByDescending(a => a.Year).ThenByDescending(a => a.Month).ToList();
            //    foreach (var item in ylist)
            //    {
            //        var clist = data.Where(a => a.CreateTime.Year == item.Year && a.CreateTime.Month == item.Month).OrderByDescending(a => a.CreateTime).ToList();
            //        list.Add(new WithdrawalDto()
            //        {
            //            Year = item.Year,
            //            Month = item.Month,
            //            List = clist,
            //        });
            //    }
            //}

            var list = new List<WithdrawalView>();

            if (data != null && data.Count > 0)
            {
                list = _mapper.ToViewList(data);
                //foreach (var item in list)
                //{

                //}
            }




            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = list.Count, items = list, mch_id = Senparc.Weixin.Config.SenparcWeixinSetting.TenPayV3_MchId, Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId };
            return res;
        }

        public async Task<ResultModel> BackEndList(QueryModel view)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<Withdrawal>(a => a.status < 99);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                int tId = 0;
                int.TryParse(view.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.withdrawalNo.Contains(view.queryName) ||
               a.openID.Contains(view.queryName) || a.bankCode.Contains(view.queryName) ||
               a.bankCard.Contains(view.queryName) || a.bankName.Contains(view.queryName) || a.reamrk.Contains(view.queryName) || a.auditIntro.Contains(view.queryName));

            }
            if (view.parentId.HasValue)
            {
                exWhere.And(a => a.userType == view.parentId.Value);
            }
            if (view.queryType.HasValue)
            {
                exWhere.And(a => a.wType == view.queryType.Value);
            }
            if (view.queryState.HasValue)
            {
                exWhere.And(a => a.status == view.queryState.Value);
            }
            if (view.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= view.startTime.Value);
            }
            if (view.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= view.endTime.Value);
            }
            var list = new List<WithdrawalView>();
            var data = await base.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            if (data != null && data.Count > 0)
            {
                var uIds = data.Select(it => it.userId).ToList();
                var uList = await UserInfoDb.GetListAsync(it => SqlFunc.ContainsArray(uIds, it.Id));

                list = _mapper.ToViewList(data);

                foreach (var item in list)
                {
                    //用户信息
                    if (item.userId > 0 && uList.Count(it => it.Id == item.userId) > 0)
                    {
                        var uInfo = uList.Find(it => it.Id == item.userId);
                        item.userName = uInfo.nickName;
                    }
                }
            }

            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }
        #endregion

        #region 申请提现
        public async Task<ResultModel> ApplyForWithdrawal(WithdrawalView model, UserInfo user)
        {
            var res = new ResultModel();
            if (model == null) { res.msg = "参数错误"; return res; }
            if (user == null)
            {
                res.msg = "用户不存在";
                res.code = (int)ResultEnum.notLogin;
                return res;
            }
            model.userId = user.Id;
            if (string.IsNullOrWhiteSpace(model.realName)) model.realName = user.nickName;
            if (model.amount <= 0)
            {
                res.msg = "提现金额必须大于0";
                return res;
            }
            if (PubConstant.Config.UserMinMoney > model.amount)
            {
                res.msg = $"最小提现金额为:{PubConstant.Config.UserMinMoney}";
                return res;
            }
            if (model.wType == 1 && (string.IsNullOrWhiteSpace(model.bankCard) || string.IsNullOrWhiteSpace(model.bankCode)))
            {
                res.msg = "银行卡信息不全";
                return res;
            }
            if (user == null && user.Id <= 0 || user.status != 0)
            {
                res.msg = "用户信息有误";
                return res;
            }
            if (model.amount > user.amount)
            {
                res.msg = $"可提现金额不足";
                return res;
            }
            if (model.wType == 0)
            {
                if (string.IsNullOrWhiteSpace(user.wxAppletsOpenId))
                {
                    res.msg = "用户微信小程序未授权";
                    return res;
                }
                model.openID = user.wxAppletsOpenId;
            }
            if (base.Count(a => a.userId == user.Id && a.status == 0) > 0)
            {
                res.msg = $"您有待审核的提现申请,请勿重复提交";
                return res;
            }
            try
            {
                await db.BeginTranAsync();
                var info = _mapper.ToModel(model);
                info.actualTotal = info.amount;
                //info.serviceRate = PubConstant.Config.UserWithdrawalRate;
                //info.serviceCharge = Math.Round(info.actualTotal * PubConstant.Config.UserWithdrawalRate, 2);
                //info.actualTotal -= info.serviceCharge;
                bool isok = false;
                info.status = 0;
                info.createTime = DateTime.Now;
                info.updateTime = DateTime.Now;
                info.withdrawalNo = CommonHelper.GenerateUniqueText();
                isok = await base.InsertAsync(info);

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

        public async Task<ResultModel> AuditWithdrawal(AuditWithdrawView view)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            var ids = view.ids;
            var state = view.status;
            var auditInfo = view.auditInfo ?? "";
            if (ids == null || ids.Count <= 0)
            {
                res.msg = "请选择审核数据";
                return res;
            }
            if (state != 1 && state != 2)
            {
                res.msg = "审核状态有误";
                return res;
            }
            //通过
            if (state == 1)
            {
                var list = await base.GetListAsync(it => it.status == 0 && SqlFunc.ContainsArray(ids, it.Id));
                if (list == null && list.Count <= 0)
                {
                    res.msg = "审核数据不存在";
                    return res;
                }
                var oknum = 0;
                string error = string.Empty;
                foreach (var item in list)
                {
                    try
                    {
                        await db.Ado.BeginTranAsync();//开始事务
                        //查询用户信息
                        var user = await UserInfoDb.GetByIdAsync(item.userId);
                        if (user == null || user.Id <= 0)
                        {
                            await db.Ado.RollbackTranAsync();
                            res.msg = "审核数据用户不存在";
                            return res;
                        }
                        if (user.amount < item.actualTotal)
                        {
                            await db.Ado.RollbackTranAsync();
                            res.msg = "用户可提现金额不足";
                            return res;
                        }
                        var wxResult = await TenPayClient.Transfers(item.withdrawalNo, item.openID, item.actualTotal, "提现", CommonHelper.GetIP(), 1);
                        if (wxResult == null || wxResult.state != "WAIT_USER_CONFIRM")
                        {
                            error += $"提现到微信失败:{(wxResult != null && !string.IsNullOrWhiteSpace(wxResult.ResultCode.Additional) ? wxResult.ResultCode.Additional : "")};";
                            await db.Ado.RollbackTranAsync();
                            res.msg = error;
                            return res;
                        }
                        item.package = wxResult.package_info;
                        oknum++;
                        item.status = state;
                        item.updateTime = DateTime.Now;
                        item.auditIntro = auditInfo;
                        await base.UpdateAsync(item);
                        //添加流水记录，更新用户佣金
                        await WalletLogDb.InsertAsync(new WalletLog
                        {
                            change = -item.amount,
                            createTime = DateTime.Now,
                            orderNo = item.withdrawalNo,
                            sourceType = (int)sourceTypeEnum.提现,
                            title = "提现",
                            wType = (int)walletTypeEnum.余额,
                            updateTime = DateTime.Now,
                            userId = item.userId,
                            userType = 0,
                            remark = ""
                        });
                        user.commission -= item.amount;
                        await UserInfoDb.UpdateAsync(a => new UserInfo { commission = user.commission }, a => a.Id == user.Id);
                        await db.Ado.CommitTranAsync();//提交事务
                    }
                    catch (Exception ex)
                    {
                        await db.Ado.RollbackTranAsync();//回滚事务
                        res.msg = ex.Message;
                    }
                }

                res.code = oknum > 0 ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "审核通过" + (oknum > 0 ? $"成功{oknum}条" : $"失败{error}");
            }
            else
            {
                var isok = await base.UpdateAsync(it => new Withdrawal { status = state, updateTime = DateTime.Now, auditIntro = auditInfo }, it => it.status == 0 && SqlFunc.ContainsArray(ids, it.Id));
                res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "驳回" + (isok ? "成功" : "失败");
            }
            return res;

        }
    }
}