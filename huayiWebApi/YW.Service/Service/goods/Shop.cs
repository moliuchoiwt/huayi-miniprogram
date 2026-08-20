using System.Data;


namespace YW.Service
{

    public partial interface IShopService : IBaseRepository<Shop>
    {
        Task<ResultModel> Operation(ShopView model);
        Task<ResultModel> IndexData();
        Task<ResultModel> LngLatGetShop(ShopQuery view);

        Task<ResultModel> frontEndList(QueryModel view);

        Task<ResultModel> BackEndList(ShopQuery view);

        //申请商家入驻
        Task<ResultModel> ApplyForMerchantsToSettleIn(ShopView model, UserInfo user);
        //详情
        Task<ResultModel> frontEndDetails(QueryModel view);

        Task<ResultModel> GetShop(UserInfo user);
    }

    public partial class ShopService : BaseRepository<Shop>, IShopService
    {
        private readonly ShopMapper _mapper = new();

        private readonly IClaimsAccessor _claimsAccessor;
        public ShopService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

        #region 列表
        public async Task<ResultModel> frontEndList(QueryModel view)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<Shop>(a => a.status == 0 && a.auditState == 1);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                int tId = 0;
                int.TryParse(view.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.name.Contains(view.queryName) || a.address.Contains(view.queryName)
               || a.labels.Contains(view.queryName) || a.mobile.Contains(view.queryName) ||
                a.times.Contains(view.queryName) || a.intro.Contains(view.queryName) || a.contents.Contains(view.queryName));
            }
            if (view.queryType.HasValue)
            {
                exWhere.And(a => a.stype == view.queryType.Value);
            }
            if (view.parentId.HasValue)
            {
                exWhere.And(a => a.parentId == view.parentId.Value);
            }
            if (view.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= view.startTime.Value);
            }
            if (view.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= view.endTime.Value);
            }

            var data = await base.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = data };
            return res;
        }

        public async Task<ResultModel> BackEndList(ShopQuery view)
        {
            var res = new ResultModel();
            if (view == null) { res.msg = "参数错误"; return res; }
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<Shop>(a => a.status != 99);

            if (view.status.HasValue) exWhere.And(a => a.status == view.status.Value);
            if (view.auditState.HasValue) exWhere.And(a => a.auditState == view.auditState.Value);
            if (view.parentId.HasValue) exWhere.And(a => a.parentId == view.parentId.Value);
            if (view.queryType.HasValue) exWhere.And(a => a.stype == view.queryType.Value);
            if (!string.IsNullOrWhiteSpace(view.queryName))
            {
                view.queryName = view.queryName.Trim();
                int tId = 0;
                int.TryParse(view.queryName, out tId);
                exWhere.And(a => a.Id == tId || a.name.Contains(view.queryName) || a.logo.Contains(view.queryName) || a.bannerUrls.Contains(view.queryName) || a.address.Contains(view.queryName) || a.labels.Contains(view.queryName) || a.mobile.Contains(view.queryName) || a.times.Contains(view.queryName) || a.intro.Contains(view.queryName) || a.contents.Contains(view.queryName));

            }
            if (view.startTime.HasValue) exWhere.And(a => a.createTime >= view.startTime.Value);
            if (view.endTime.HasValue) exWhere.And(a => a.createTime <= view.endTime.Value);

            var list = new List<ShopView>();
            var data = await base.GetPageListAsync(exWhere, p, it => new { it.createTime, it.Id }, OrderByType.Desc);
            if (data != null && data.Count > 0)
            {
                list = _mapper.ToViewList(data);
                foreach (var item in list)
                {
                    item.logo = WebFileHelper.GetUrl(item.logo);
                    item.businessImg = WebFileHelper.GetUrl(item.businessImg);
                    item.bannerList = WebFileHelper.GetListUrl(item.bannerUrls);
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }

        #endregion

        #region 申请商家入驻
        public async Task<ResultModel> ApplyForMerchantsToSettleIn(ShopView model, UserInfo user)
        {
            var res = new ResultModel();
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            if (model == null) { res.msg = "参数错误"; return res; }
            if (string.IsNullOrWhiteSpace(model.name))
            {
                res.msg = "店铺名称不能为空";
                return res;
            }
            if (string.IsNullOrWhiteSpace(model.businessImg))
            {
                res.msg = "营业执照不能为空";
                return res;
            }
            if (model.businessImg.Contains(PubConstant.Config.DomianStaticName)) model.businessImg = model.businessImg.Replace(PubConstant.Config.DomianStaticName, "");

            if (string.IsNullOrWhiteSpace(model.province) || string.IsNullOrWhiteSpace(model.city) || string.IsNullOrWhiteSpace(model.area))
            {
                res.msg = "请选择省市区";
                return res;
            }
            if (string.IsNullOrWhiteSpace(model.address))
            {
                res.msg = "详细地址不能为空";
                return res;
            }
            if (string.IsNullOrWhiteSpace(model.realName))
            {
                res.msg = "联系人姓名不能为空";
                return res;
            }
            if (string.IsNullOrWhiteSpace(model.mobile))
            {
                res.msg = "联系人电话不能为空";
                return res;
            }

            //if (string.IsNullOrWhiteSpace(model.IdCard))
            //{
            //    res.msg = "身份证号不能为空";
            //    return res;
            //}

            //if (string.IsNullOrWhiteSpace(model.IdImg1))
            //{
            //    res.msg = "身份证正面不能为空";
            //    return res;
            //}
            //if (string.IsNullOrWhiteSpace(model.IdImg2))
            //{
            //    res.msg = "身份证反面不能为空";
            //    return res;
            //}
            if (ShopDb.Count(it => it.status != 99 && it.userId == user.Id) > 0)
            {
                var shop = await ShopDb.GetFirstAsync(it => it.userId == user.Id && it.status != 99);
                model.Id = shop.Id;

                // 如果已通过的商家只更新非执照字段（地址、电话、店铺名），不要重新进入审核
                // 如果改了营业执照，则需要重新审核
                if (shop.auditState == 1 && shop.businessImg == model.businessImg)
                {
                    // 资料微调，不重置审核状态
                    model.auditState = 1;
                    model.auditIntro = shop.auditIntro;
                }
            }
            var info = _mapper.ToModel(model);
            bool isok = false;
            // 只有「资料更新」（auditState=1 且 执照未改）才保留 1，其他情况都进入待审核 0
            if (info.auditState != 1)
            {
                info.auditState = 0;
            }
            info.status = 1;
            info.amount = 0;
            info.userId = user.Id;
            if (info.Id > 0)
            {
                isok = await base.UpdateAsync(it => new Shop
                {
                    name = info.name,
                    businessImg = info.businessImg,
                    province = info.province,
                    city = info.city,
                    area = info.area,
                    address = info.address,
                    realName = info.realName,
                    mobile = info.mobile,
                    auditState = info.auditState,
                    status = info.status,
                    amount = info.amount,
                    userId = info.userId,
                    updateTime = DateTime.Now
                }, it => it.Id == info.Id && it.status != 99);
            }
            else
            {
                info.createTime = DateTime.Now;
                info.updateTime = DateTime.Now;
                isok = await base.InsertAsync(info);
            }
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "提交" + (isok ? "成功" : "失败");

            // 需求3：商戶登記通知管理員（含執照圖），需人工審批
            // 只有「真正进入待审核」或「已通过但改了执照」才需要通知管理员
            if (isok && info.auditState == 0)
            {
                _ = NotifyAdminForMerchantRegister(user, model);
            }

            return res;
        }

        #region 郵件通知（需求3）

        /// <summary>
        /// 商戶登記通知管理員（人工審批，不可自動批准）
        /// </summary>
        private async Task NotifyAdminForMerchantRegister(UserInfo user, ShopView model)
        {
            try
            {
                var adminEmail = YW.Common.ConfigHelper.GetSectionValue("EmailSetting:AdminTo") ?? "studioofjoyhk@gmail.com";
                var body = $@"
                    <h3>【華藝】新的商戶登記待審批</h3>
                    <p>管理員你好，</p>
                    <p>有用戶提交了商戶（花店）入駐登記，<b>需人工審批</b>：</p>
                    <ul>
                        <li>用戶：{user?.nickName}（ID:{user?.Id}，手機:{user?.mobile}）</li>
                        <li>店鋪名稱：{model?.name}</li>
                        <li>聯繫人：{model?.realName} / {model?.mobile}</li>
                        <li>地區：{model?.province}{model?.city}{model?.area} {model?.address}</li>
                        <li>提交時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</li>
                    </ul>
                    <p>請前往後台審核商戶登記。</p>
                ";
                // 含執照圖片附件
                byte[] licenseImg = null;
                if (!string.IsNullOrEmpty(model?.businessImg))
                {
                    var absPath = Path.Combine(Directory.GetCurrentDirectory(),
                        model.businessImg.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (File.Exists(absPath)) licenseImg = await File.ReadAllBytesAsync(absPath);
                }
                await YW.Common.EmailClient.SendAsync(adminEmail, "【華藝】商戶登記待審批", body,
                    ("business_license.png", licenseImg));
            }
            catch { }
        }

        #endregion
        #endregion

        /// <summary>
        /// 首页图数据
        /// </summary>
        /// <returns></returns>
        public async Task<ResultModel> IndexData()
        {
            ResultModel result = new ResultModel();

            var ds = await db.Ado.GetDataSetAllAsync($"exec pro_indexView {_claimsAccessor.UserId}");

            //获取一年每月的销售额和销售数量
            //销售额
            decimal[] volumeData = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //销售数量
            decimal[] numData = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            if (ds != null && ds.Tables.Count > 0)
            {
                var lineDt = ds.Tables[0];
                foreach (DataRow item in lineDt.Rows)
                {
                    var month = int.Parse(item["mon"].ToString());//月份
                    var salesVolume = decimal.Parse(item["salesVolume"].ToString());//销售额
                    var salesNum = int.Parse(item["salesNum"].ToString());//销售数量

                    volumeData[month - 1] = salesVolume;
                    numData[month - 1] = salesNum;
                }
            }

            var dt = new DataTable();
            if (ds != null && ds.Tables.Count > 1) dt = ds.Tables[1];

            result.code = (int)ResultEnum.success;
            result.msg = "请求成功";
            result.data = new { volumeData, numData, index_data = dt };
            return result;

        }

        public async Task<ResultModel> Operation(ShopView model)
        {
            var res = new ResultModel();
            var info = _mapper.ToModel(model);
            bool isok = false;
            if (info.score < 0) info.score = 5;
            if (info.Id > 0 && base.Count(it => it.Id == info.Id) == 0) info.Id = 0;
            try
            {
                await db.BeginTranAsync();
                if (info.Id > 0)
                {
                    var oldInfo = await ShopDb.GetByIdAsync(info.Id);
                    if (oldInfo.auditState == 0 && info.auditState == 1)
                    {
                        info.status = 0;
                    }
                    isok = await base.UpdateAsync(it => new Shop
                    {
                        status = info.status,
                        auditState = info.auditState,
                        updateTime = DateTime.Now
                    }, it => it.Id == info.Id);
                }
                else
                {
                    info.createTime = DateTime.Now;
                    info.updateTime = DateTime.Now;
                    info.Id = await base.InsertReturnIdentityAsync(info);
                    isok = info.Id > 0;
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
        /// <summary>
        /// 通过经纬度获取最近的店铺ID
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public async Task<ResultModel> LngLatGetShop(ShopQuery view)
        {
            var res = new ResultModel();

            decimal lat = 0, lng = 0;
            if (view.Lng.HasValue && view.Lat.HasValue)
            {
                lng = view.Lng.Value;
                lat = view.Lat.Value;
            }
            else
            {
                var json = await CommonHelper.GetAddressByIP();

                if (Convert.ToInt16(json["status"]) == 0)
                {
                    var result = json["result"].ToString();
                    Newtonsoft.Json.Linq.JObject resultJson = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(result);
                    string location = resultJson["location"].ToString();
                    Newtonsoft.Json.Linq.JObject locationJson = (Newtonsoft.Json.Linq.JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(location);
                    lng = Convert.ToDecimal(locationJson["lng"]);
                    lat = Convert.ToDecimal(locationJson["lat"]);
                }
            }


            var shopid = await db.Ado.GetIntAsync(@$" declare @shopid int=0
            select dbo.fnGetDistance(Lat,Lng,{lat},{lng}) distance,
            Id,Name into #tab from shop where state=0 and Stype=1 
            select top 1 @shopid=Id from #tab order by distance 
            select @shopid 
            drop table #tab ");

            string ShopName = string.Empty;
            var shop = ShopDb.GetById(shopid);
            if (shop != null) ShopName = shop.name;
            res.msg = "ok";
            res.code = (int)ResultEnum.success;
            res.data = new { ShopId = shopid, ShopName };

            return res;
        }

        #region 详情
        public async Task<ResultModel> frontEndDetails(QueryModel view)
        {
            var res = new ResultModel();
            if (!view.queryId.HasValue)
            {
                res.msg = "店铺参数错误";
                return res;
            }
            var info = await base.GetByIdAsync(view.queryId.Value);
            if (info == null || info.status != 0)
            {
                res.msg = "店铺不存在或已停业";
                return res;
            }
            var shop = _mapper.ToDto(info);
            shop.contents = WebFileHelper.getContent(info.contents);
            // shop.PayRule = getContent(info.PayRule);
            shop.logo = WebFileHelper.GetUrl(info.logo);
            shop.imgList = WebFileHelper.GetListUrl(shop.bannerUrls);

            res.msg = "ok";
            res.data = shop;
            res.code = (int)ResultEnum.success;
            return res;
        }
        #endregion

        #region 获取店铺信息
        public async Task<ResultModel> GetShop(UserInfo user)
        {
            var res = new ResultModel();
            if (user == null || user.status != 0) { res.msg = "请重新登录"; res.code = (int)ResultEnum.notLogin; return res; }
            if (ShopDb.Count(it => it.status != 99 && it.userId == user.Id) == 0)
            {
                res.msg = "店铺不存在";
                return res;
            }
            var shop = await ShopDb.GetFirstAsync(it => it.userId == user.Id && it.status != 99);
            shop.logo = WebFileHelper.GetUrl(shop.logo);
            shop.businessImg = WebFileHelper.GetUrl(shop.businessImg);
            var bannerList = WebFileHelper.GetListUrl(shop.bannerUrls);

            res.data = new
            {
                shop,
                bannerList
            };
            res.msg = "ok";
            res.code = (int)ResultEnum.success;
            return res;
        }
        #endregion
    }
}