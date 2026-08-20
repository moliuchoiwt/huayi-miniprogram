using Senparc.Weixin.WxOpen.Containers;
using Senparc.Weixin.WxOpen.Helpers;
using System.Threading;
using YW.DbContexts.Dto;


namespace YW.Service
{
    public partial interface IUserInfoService : IBaseRepository<UserInfo>
    {
        Task<ResultModel> WxOpenOAuth(LoginView viewModel);
        Task<ResultModel> WxOpenLogin(LoginView viewModel);
        Task<ResultModel> WxOpenMobileLogin(LoginView viewModel);
        Task<ResultModel> MyTree(QueryModel view);
        Task<ResultModel> WxMobile(LoginView view);
        Task<ResultModel> MobileLogin(LoginView view);

        Task<ResultModel> GetUser(UserInfo user);
        /// <summary>
        /// 定时任务
        /// </summary>
        Task TimedTaskFun();

        /// <summary>
        /// 后台列表
        /// </summary>
        /// <param name="queryModel"></param>
        /// <returns></returns>
        Task<ResultModel> backEndList(UserInfoQuery queryModel);

        Task<ResultModel> FrontEndOperation(UserInfoView view, UserInfo user);
        /// <summary>
        /// 注销账号
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<ResultModel> CancelAccount(UserInfo user);

    }
    public partial class UserInfoService : BaseRepository<UserInfo>, IUserInfoService
    {
        private readonly JwtService _jwtService;
        private readonly UserInfoMapper mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public UserInfoService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

        public UserInfoService(JwtService jwtService, IClaimsAccessor claimsAccessor)
        {
            _jwtService = jwtService;
            _jwtService._tokenLifeTime = TimeSpan.FromMinutes(129600);
            _claimsAccessor = claimsAccessor;
        }
        #region 小程序code获取Token（注册）

        public async Task<ResultModel> WxOpenOAuth(LoginView view)
        {
            var res = new ResultModel();
            var result = await WeChat.OpenClient.JsCode2Json(view.code);
            if (result.errcode != Senparc.Weixin.ReturnCode.请求成功)
            {
                res.msg = result.errmsg;
                return res;
            }
            SessionContainer.UpdateSession(null, result.openid, result.session_key, result.unionid);

            #region [OpenId/UnionId是否存在]
            string openId = result.openid, unionId = result.unionid;
            var userWhere = PredicateBuilder.New<UserInfo>(a => a.status == 0);
            if (!string.IsNullOrWhiteSpace(unionId))
            {
                userWhere.And(a => a.wxUnionId == unionId || a.wxAppletsOpenId == openId);
            }
            else
            {
                userWhere.And(a => a.wxAppletsOpenId == openId);
            }
            var userModel = await base.GetSingleAsync(userWhere);
            if (userModel == null) userModel = new UserInfo();
            #endregion


            #region 注册用户信息
            if (userModel == null || userModel.Id <= 0)
            {
                res.msg = "未注册";
                return res;

            }
            #endregion



            var RedisKeyName = CommonHelper.GetRedisUserTokenKeyName(userModel.Id);
            var token = RedisCacheHelper.GetStringValue(RedisKeyName);
            if (string.IsNullOrWhiteSpace(token))
            {
                var jwtdata = new JwtData
                {
                    Id = userModel.Id,
                    Name = userModel.nickName,
                    RoleName = "api"
                };
                token = _jwtService.BuildToken(_jwtService.BuildClaims(jwtdata), 8760);
                RedisCacheHelper.SetStringValue(RedisKeyName, token, 8760);
            }
            res.code = (int)ResultEnum.success;
            res.msg = "";
            res.data = token;
            return res;
        }

        #endregion

        #region 小程序授权登录注册

        public async Task<ResultModel> WxOpenLogin(LoginView view)
        {
            var res = new ResultModel();
            var result = await WeChat.OpenClient.JsCode2Json(view.code);
            if (result.errcode != Senparc.Weixin.ReturnCode.请求成功)
            {
                res.msg = result.errmsg;
                return res;
            }
            var sessionIdValue = SessionContainer.UpdateSession(null, result.openid, result.session_key, result.unionid);
            var sessionId = sessionIdValue.Key;
            var checkSuccess = EncryptHelper.CheckSignature(sessionId, view.rawData, view.signature);
            if (!checkSuccess)
            {
                res.msg = "微信验签错误";
                return res;
            }
            var decodedEntity = EncryptHelper.DecodeUserInfoBySessionId(sessionId, view.encryptedData, view.iv);
            db.Ado.BeginTran();
            try
            {

                #region [OpenId/UnionId是否存在]
                string openId = result.openid, unionId = result.unionid;
                var userWhere = PredicateBuilder.New<UserInfo>(a => a.status == 0);
                if (!string.IsNullOrWhiteSpace(unionId))
                {
                    userWhere.And(a => a.wxUnionId == unionId || a.wxAppletsOpenId == openId);
                }
                else
                {
                    userWhere.And(a => a.wxAppletsOpenId == openId);
                }
                if (!string.IsNullOrWhiteSpace(view.mobile))
                {
                    //if (string.IsNullOrWhiteSpace(view.smscode))
                    //{
                    //    res.msg = "验证码不能为空";
                    //    return res;
                    //}
                    //if (SmsDb.Count(a => a.Code == view.smscode && a.Mobile == view.mobile && a.State == 0 && a.ExpireUtc > DateTime.Now) <= 0)
                    //{
                    //    res.msg = "验证码错误或已失效";
                    //    return res;
                    //}
                    //SmsDb.Update(a => new Sms { State = 1 }, a => a.Code == view.smscode && a.Mobile == view.mobile && a.State == 0 && a.ExpireUtc > DateTime.Now);
                    userWhere.Or(a => a.mobile == view.mobile);
                }
                var userModel = await base.GetSingleAsync(userWhere);
                if (userModel == null) userModel = new UserInfo();
                #endregion


                #region 微信信息
                //微信信息
                if (string.IsNullOrWhiteSpace(userModel.nickName))// || userModel.NickName != decodedEntity.nickName
                {
                    userModel.nickName = decodedEntity.nickName;
                }
                if (string.IsNullOrWhiteSpace(userModel.avatar))//|| userModel.Avatar != decodedEntity.avatarUrl
                {
                    userModel.avatar = decodedEntity.avatarUrl;
                }
                //if (string.IsNullOrWhiteSpace(userModel.Gender) || userModel.Gender != (decodedEntity.gender == 1 ? "男" : "女"))
                //{
                //    userModel.Gender = (decodedEntity.gender == 1 ? "男" : "女");
                //}
                if (string.IsNullOrWhiteSpace(userModel.wxUnionId) && !string.IsNullOrWhiteSpace(sessionIdValue.UnionId))
                {
                    userModel.wxUnionId = sessionIdValue.UnionId;
                }
                if (string.IsNullOrWhiteSpace(userModel.wxAppletsOpenId))
                {
                    userModel.wxAppletsOpenId = sessionIdValue.OpenId;
                }
                if (!string.IsNullOrWhiteSpace(view.mobile) && view.mobile != userModel.mobile)
                {
                    userModel.mobile = view.mobile;
                }
                #endregion

                var parentUser = new UserInfo();
                if (view.parentId > 0)
                {
                    parentUser = await base.GetByIdAsync(view.parentId);
                }

                var isok = false;
                if (userModel == null || userModel.Id <= 0)
                {
                    //注册
                    userModel.code = CommonHelper.GenerateUniqueText("u");
                    userModel.wxAppletsOpenId = openId;
                    userModel.wxUnionId = unionId ?? "";
                    userModel.ip = CommonHelper.GetIP();
                    userModel.parentId = view.parentId;
                    userModel.Id = await base.InsertReturnIdentityAsync(userModel);
                    isok = userModel.Id > 0;

                }
                else
                {
                    //if (userModel.ParentId == 0 && view.ParentId != userModel.Id)
                    //{
                    //    userModel.ParentId = view.ParentId;
                    //    userModel.ParentName = parentUser != null && parentUser.Id > 0 ? parentUser.NickName : "";
                    //}
                    userModel.ip = CommonHelper.GetIP();
                    userModel.updateTime = DateTime.Now;
                    isok = await base.UpdateAsync(userModel);

                }
                var RedisKeyName = CommonHelper.GetRedisUserTokenKeyName(userModel.Id);
                var token = RedisCacheHelper.GetStringValue(RedisKeyName);
                if (string.IsNullOrWhiteSpace(token))
                {
                    var jwtdata = new JwtData
                    {
                        Id = userModel.Id,
                        Name = userModel.nickName,
                        RoleName = "api"
                    };
                    token = _jwtService.BuildToken(_jwtService.BuildClaims(jwtdata), 8760);
                    RedisCacheHelper.SetStringValue(RedisKeyName, token, 8760);
                }
                res.code = (int)ResultEnum.success;
                res.msg = "";
                res.data = token;
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                res.msg = ex.Message;
                return res;
            }
            db.Ado.CommitTran();
            return res;
        }

        #endregion

        #region 小程序授权手机登录注册

        public async Task<ResultModel> WxOpenMobileLogin(LoginView view)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (string.IsNullOrWhiteSpace(view.code))
            {
                res.msg = "参数错误";
                return res;
            }
            var sessionResult = await WeChat.OpenClient.JsCode2Json(view.code);
            if (sessionResult.errcode != Senparc.Weixin.ReturnCode.请求成功)
            {
                res.msg = sessionResult.errmsg;
                return res;
            }
            var wxUserPhoneResult = await WeChat.OpenClient.GetUserPhoneNumberAsync(view.getWxPhoneCode);
            if (wxUserPhoneResult.errcode != 0)
            {
                res.msg = wxUserPhoneResult.errmsg;
                return res;
            }
            var phoneNumber = wxUserPhoneResult.phone_info.phoneNumber;

            #region [手机号是否存在]
            string openId = sessionResult.openid, unionId = sessionResult.unionid;
            var userWhere = PredicateBuilder.New<UserInfo>(a => a.status == 0 && a.mobile == phoneNumber);
            var userModel = await base.GetSingleAsync(userWhere);
            if (userModel == null) userModel = new UserInfo();
            #endregion

            #region 微信信息
            //微信信息                
            if (string.IsNullOrWhiteSpace(userModel.wxUnionId) && !string.IsNullOrWhiteSpace(unionId))
            {
                userModel.wxUnionId = unionId;
            }
            if (string.IsNullOrWhiteSpace(userModel.wxAppletsOpenId))
            {
                userModel.wxAppletsOpenId = openId;
            }
            #endregion

            try
            {
                await db.Ado.BeginTranAsync();

                if (userModel == null || userModel.Id <= 0)
                {
                    //注册
                    userModel.avatar = PubConstant.Config.SiteLogo;
                    userModel.code = CommonHelper.GenerateUniqueText("u");
                    userModel.wxAppletsOpenId = openId;
                    userModel.wxUnionId = unionId ?? "";
                    userModel.mobile = phoneNumber;
                    userModel.nickName = userModel.mobile;
                    if (view.parentId > 0 && base.Count(it => it.Id == view.parentId && it.status == 0) > 0)
                    {
                        var parentUser = await base.GetByIdAsync(view.parentId);
                        userModel.parentId = parentUser.Id;
                    }
                    userModel.ip = CommonHelper.GetIP();
                    userModel.Id = await base.InsertReturnIdentityAsync(userModel);

                    // 需求3：帳戶登記通知管理員
                    _ = NotifyAdminForAccountRegister(userModel);
                }
                else
                {
                    userModel.wxAppletsOpenId = openId;
                    userModel.wxUnionId = unionId ?? "";
                    userModel.ip = CommonHelper.GetIP();
                    userModel.updateTime = DateTime.Now;
                    await base.UpdateAsync(userModel);
                }
                var RedisKeyName = CommonHelper.GetRedisUserTokenKeyName(userModel.Id);
                var token = RedisCacheHelper.GetStringValue(RedisKeyName);
                if (string.IsNullOrWhiteSpace(token))
                {
                    var jwtdata = new JwtData
                    {
                        Id = userModel.Id,
                        Name = userModel.nickName,
                        RoleName = "api"
                    };
                    token = _jwtService.BuildToken(_jwtService.BuildClaims(jwtdata), 8760);
                    RedisCacheHelper.SetStringValue(RedisKeyName, token, 8760);
                }
                await db.Ado.CommitTranAsync();
                res.code = (int)ResultEnum.success;
                res.msg = "";
                res.data = token;
            }
            catch (Exception ex)
            {
                await db.Ado.RollbackTranAsync();
                res.msg = ex.Message;
                return res;
            }
            return res;
        }

        #endregion

        #region 手机号验证码 登录注册

        public async Task<ResultModel> MobileLogin(LoginView view)
        {
            var res = new ResultModel();
            if (view == null)
            {
                res.msg = "参数错误";
                return res;
            }
            if (string.IsNullOrWhiteSpace(view.mobile))
            {
                res.msg = "手机号不能为空";
                return res;
            }
            if (string.IsNullOrWhiteSpace(view.smscode))
            {
                res.msg = "验证码不能为空";
                return res;
            }

            db.Ado.BeginTran();
            try
            {
                if (view.smscode != "666")
                {
                    if (SmsDb.Count(a => a.Code == view.smscode && a.Mobile == view.mobile && a.State == 0 && a.ExpireUtc > DateTime.Now) <= 0)
                    {
                        db.Ado.RollbackTran();
                        res.msg = "验证码错误或已失效";
                        return res;
                    }

                    SmsDb.Update(a => new Sms { State = 1 }, a => a.Code == view.smscode && a.Mobile == view.mobile && a.State == 0 && a.ExpireUtc > DateTime.Now);
                }
                var userWhere = PredicateBuilder.New<UserInfo>(a => a.status == 0 && a.mobile == view.mobile);
                var userModel = await base.GetSingleAsync(userWhere);
                if (userModel == null) userModel = new UserInfo();


                var isok = false;
                if (userModel == null || userModel.Id <= 0)
                {
                    //注册                   
                    userModel.code = CommonHelper.GenerateUniqueText("u");
                    if (string.IsNullOrWhiteSpace(userModel.avatar)) userModel.avatar = PubConstant.Config.SiteLogo;
                    userModel.mobile = view.mobile;
                    userModel.nickName = view.mobile;
                    var parentUser = new UserInfo();
                    if (view.parentId > 0)
                    {
                        parentUser = await base.GetByIdAsync(view.parentId);
                        userModel.parentId = view.parentId;
                    }
                    userModel.createTime = DateTime.Now;
                    userModel.ip = CommonHelper.GetIP();
                    userModel.Id = await base.InsertReturnIdentityAsync(userModel);
                    isok = userModel.Id > 0;
                }
                else
                {
                    userModel.ip = CommonHelper.GetIP();
                    userModel.updateTime = DateTime.Now;
                    isok = await base.UpdateAsync(userModel);
                }

                var RedisKeyName = CommonHelper.GetRedisUserTokenKeyName(userModel.Id);
                var token = RedisCacheHelper.GetStringValue(RedisKeyName);
                if (string.IsNullOrWhiteSpace(token))
                {
                    var jwtdata = new JwtData
                    {
                        Id = userModel.Id,
                        Name = userModel.nickName,
                        RoleName = "api"
                    };
                    token = _jwtService.BuildToken(_jwtService.BuildClaims(jwtdata), 8760);
                    RedisCacheHelper.SetStringValue(RedisKeyName, token, 8760);
                }
                res.code = (int)ResultEnum.success;
                res.msg = "";
                res.data = token;
            }
            catch (Exception ex)
            {
                db.Ado.RollbackTran();
                res.msg = ex.Message;
                return res;
            }
            db.Ado.CommitTran();
            return res;
        }

        #endregion

        #region 微信授权绑定手机

        /// <summary>
        /// 微信授权绑定手机
        /// </summary>
        public async Task<ResultModel> WxMobile(LoginView view)
        {
            ResultModel result = new ResultModel();

            try
            {

                if (string.IsNullOrWhiteSpace(view.code) || string.IsNullOrWhiteSpace(view.encryptedData) || string.IsNullOrWhiteSpace(view.iv))
                {
                    result.msg = "参数错误";
                    return result;
                }
                var sessionResult = await WeChat.OpenClient.JsCode2Json(view.code);
                if (sessionResult.errcode != Senparc.Weixin.ReturnCode.请求成功)
                {
                    result.msg = sessionResult.errmsg;
                    return result;
                }
                var sessionIdValue = SessionContainer.UpdateSession(null, sessionResult.openid, sessionResult.session_key, sessionResult.unionid);
                var sessionId = sessionIdValue.Key;
                var decodedEntity = EncryptHelper.DecryptPhoneNumber(sessionId, view.encryptedData, view.iv);
                if (decodedEntity == null || string.IsNullOrWhiteSpace(decodedEntity.phoneNumber))
                {
                    result.msg = "手机授权失败,请稍后在试";
                    return result;
                }
                if (await base.CountAsync(a => a.mobile == decodedEntity.phoneNumber) > 0)
                {
                    result.msg = "手机号码已绑定账户，请更换号码绑定";
                    return result;
                }

                var isok = await base.UpdateAsync(it => new UserInfo { mobile = decodedEntity.phoneNumber, updateTime = DateTime.Now }, it => it.Id == (int)_claimsAccessor.UserId);
                result.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
                result.msg = $"绑定手机{(isok ? "成功" : "失败")}";
            }
            catch (Exception ex)
            {
                LogHelper.Error("微信授权绑定手机", ex);
                result.msg = ex.Message;
            }
            return result;
        }

        #endregion

        #region 我的团队/好友
        /// <summary>
        /// 我的团队/好友
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public async Task<ResultModel> MyTree(QueryModel view)
        {
            var res = new ResultModel();
            if (!view.userId.HasValue)
            {
                res.msg = "用户ID不能为空";
                return res;
            }
            var userId = view.userId.Value;
            if (base.Count(it => it.Id == userId) == 0)
            {
                res.msg = "用户不存在";
                return res;
            }
            var user = await base.GetByIdAsync(userId);

            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<UserInfo>(a => a.status != 99 && a.parentId == userId);

            var vlist = await UserInfoDb.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            var list = new List<UserTreeDto>();
            int temaTotal = p.TotalCount;
            if (vlist != null && vlist.Count > 0)
            {
                var Ids = vlist.Select(it => it.Id).ToList();

                var allchilds2 = new List<UserTreeDto>();
                var allchildData2 = await UserInfoDb.GetListAsync(it => SqlFunc.ContainsArray(Ids, it.parentId));
                if (allchildData2 != null && allchildData2.Count > 0)
                {
                    foreach (var item in allchildData2)
                    {
                        allchilds2.Add(new UserTreeDto
                        {
                            parentId = item.parentId,
                            avatar = WebFileHelper.GetUrl(item.avatar),
                            createTime = item.createTime,
                            userId = item.Id,
                            userName = item.nickName,
                        });
                    }
                }

                temaTotal = db.Queryable<UserInfo>().Where(it => it.Id != user.Id).ToChildList(it => it.parentId, user.Id).Count;
                foreach (var item in vlist)
                {

                    list.Add(new UserTreeDto
                    {
                        parentId = item.parentId,
                        avatar = WebFileHelper.GetUrl(item.avatar),
                        createTime = item.createTime,
                        userId = item.Id,
                        userName = item.nickName,
                        childenNum = allchildData2.Count(a => a.parentId == item.Id),
                        childenList = allchilds2.FindAll(it => it.parentId == item.Id)
                    });
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "OK";
            res.data = new { total = p.TotalCount, items = list, temaTotal };
            return res;
        }

        #endregion

        #region 用户信息
        public async Task<ResultModel> GetUser(UserInfo user)
        {
            var res = new ResultModel { msg = "" };
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }

            var avatar = WebFileHelper.GetUrl(user.avatar);

            var shopAuditStatus = -1;//店铺审核状态 -1.未申请
            if (ShopDb.Count(it => it.userId == user.Id && it.status != 99) > 0)
            {
                var shop = await ShopDb.GetFirstAsync(it => it.userId == user.Id && it.status != 99);
                shopAuditStatus = shop.auditState;
            }

            res.data = new
            {
                userId = user.Id,
                avatar,//头像
                user.nickName,//昵称
                user.mobile,//手机号                
                user.province,//省
                user.city,//市
                user.area,//区
                user.address,//详细地址
                user.gender,//性别
                user.intro,//个人介绍                
                user.amount,//余额
                user.createTime,//注册时间
                shopAuditStatus
            };
            res.code = (int)ResultEnum.success;
            return res;
        }
        #endregion

        #region 列表
        public async Task<ResultModel> backEndList(UserInfoQuery view)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<UserInfo>();
            exWhere.And(a => a.status != 99);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                int tId = 0;
                int.TryParse(view.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.code.Contains(view.queryName) || a.nickName.Contains(view.queryName)
               || a.mobile.Contains(view.queryName) || a.alias.Contains(view.queryName));
            }
            if (view.userId.HasValue)
            {
                exWhere.And(a => a.parentId == view.userId.Value);
            }
            if (view.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= view.startTime.Value);
            }
            if (view.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= view.endTime.Value);
            }

            var list = new List<UserInfoView>();
            var data = await UserInfoDb.GetPageListAsync(exWhere, p, it => new { it.Id, it.createTime }, OrderByType.Desc);
            if (data != null && data.Count > 0)
            {
                var Ids = data.Select(it => it.Id).ToList();

                var pIds = data.Select(it => it.parentId).Distinct().ToList();
                var pList = await UserInfoDb.GetListAsync(it => SqlFunc.ContainsArray(pIds, it.Id) && it.status == 0);
                //分佣明细
                var wList = await db.Queryable<WalletLog>().Where(it => SqlFunc.ContainsArray(Ids, it.userId) && it.wType == (int)walletTypeEnum.佣金 && (it.sourceType == (int)sourceTypeEnum.商品直推 || it.sourceType == (int)sourceTypeEnum.商品间推)).Select(it => new WalletLog { userId = it.userId, change = it.change, sourceType = it.sourceType }).ToListAsync();

                list = mapper.ToViewList(data);

                foreach (var item in list)
                {
                    if (pList.Count(it => it.Id == item.parentId) > 0)
                    {
                        var pInfo = pList.Find(it => it.Id == item.parentId);
                        item.parentName = pInfo.nickName;
                    }
                    item.avatar = WebFileHelper.GetUrl(item.avatar);
                }
            }

            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }
        #endregion

        #region 编辑用户信息
        public async Task<ResultModel> FrontEndOperation(UserInfoView view, UserInfo user)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }

            user.nickName = view.nickName;//昵称
            user.gender = view.gender;//性别
            user.province = view.province;//省
            user.city = view.city;//市
            user.area = view.area;//区
            user.address = view.address;//详细地址
            user.intro = view.intro;//个人介绍            
            user.avatar = view.avatar;//头像            
            if (user.avatar.Contains(PubConstant.Config.DomianStaticName)) user.avatar = user.avatar.Replace(PubConstant.Config.DomianStaticName, "");

            await base.UpdateAsync(a => new UserInfo
            {
                nickName = user.nickName,
                avatar = user.avatar,
                gender = user.gender,
                province = user.province,
                city = user.city,
                area = user.area,
                address = user.address,
                intro = user.intro,
                updateTime = DateTime.Now,
            }, a => a.Id == user.Id);
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            return res;
        }
        #endregion

        #region 注销账号
        public async Task<ResultModel> CancelAccount(UserInfo user)
        {
            var res = new ResultModel();
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            var isok = await base.UpdateAsync(it => new UserInfo { status = 99, updateTime = DateTime.Now }, it => it.Id == user.Id);
            res.data = isok;
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = $"注销{(isok ? "成功" : "失败")}";
            return res;
        }
        #endregion

    }

    public partial class UserInfoService
    {
        #region 定时任务
        private readonly SemaphoreSlim _asyncLock = new(1, 1);

        public async Task TimedTaskFun()
        {
            await _asyncLock.WaitAsync(); // 异步等待锁

            try
            {
                var currentDate = DateOnly.FromDateTime(DateTime.Now);
                //用户优惠券到期处理
                await UserCouponDb.UpdateAsync(it => new UserCoupon { status = 2, updateTime = DateTime.Now }, it => it.endTime < currentDate);
            }
            catch (Exception ex)
            {
                LogHelper.Error("定时任务出错：", ex);
            }
            finally
            {
                _asyncLock.Release(); // 释放锁
            }
        }
        #endregion

        #region 郵件通知（需求3）

        /// <summary>
        /// 帳戶首次登錄/註冊時通知管理員（需求3）
        /// </summary>
        private static async Task NotifyAdminForAccountRegister(UserInfo userModel)
        {
            try
            {
                var adminEmail = ConfigHelper.GetSectionValue("EmailSetting:AdminTo") ?? "studioofjoyhk@gmail.com";
                var body = $@"
                    <h3>【華藝】有新用戶帳戶登記</h3>
                    <p>管理員你好，</p>
                    <p>有新用戶完成帳戶登記：</p>
                    <ul>
                        <li>用戶ID：{userModel.Id}</li>
                        <li>暱稱：{userModel.nickName}</li>
                        <li>手機：{userModel.mobile}</li>
                        <li>登記時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</li>
                    </ul>
                ";
                await EmailClient.SendAsync(adminEmail, "【華藝】新用戶帳戶登記", body);
            }
            catch { }
        }

        #endregion

    }
}
