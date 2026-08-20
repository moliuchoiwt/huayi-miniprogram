namespace YW.Service
{
    public partial interface IUserAddressService : IBaseRepository<UserAddress>
    {
        Task<ResultModel> frontEndList(QueryModel view, UserInfo user);

        Task<ResultModel> frontEndOperation(UserAddressView model, UserInfo user);

        Task<ResultModel> frontEndSetDefault(QueryModel model, UserInfo user);

        Task<ResultModel> frontEndDelete(DelModel del, UserInfo user);

    }
    public partial class UserAddressService : BaseRepository<UserAddress>, IUserAddressService
    {

        private readonly IClaimsAccessor _claimsAccessor;
        private readonly UserAddressMapper _mapper = new();

        public UserAddressService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }
        #region 列表
        public async Task<ResultModel> frontEndList(QueryModel view, UserInfo user)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = view.pageNum, PageSize = view.pageSize };
            var exWhere = PredicateBuilder.New<UserAddress>(a => a.status == 0 && a.userId == (int)_claimsAccessor.UserId);
            var data = await base.GetPageListAsync(exWhere, p, it => new { it.isDefault, it.updateTime, it.Id }, OrderByType.Desc);
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = data };
            return res;
        }

        #endregion

        #region 编辑
        public async Task<ResultModel> frontEndOperation(UserAddressView model, UserInfo user)
        {
            var res = new ResultModel();
            model.userId = (int)_claimsAccessor.UserId;
            model.userName = _claimsAccessor.UserName;
            model.status = 0;
            var info = _mapper.ToModel(model);
            bool isok = false;
            if (info.Id > 0)
            {
                info.updateTime = DateTime.Now;
                isok = await base.UpdateAsync(info);
            }
            else
            {
                info.createTime = DateTime.Now;
                info.updateTime = DateTime.Now;
                info.Id = await base.InsertReturnIdentityAsync(info);
                isok = info.Id > 0;
            }
            if (info.isDefault) await base.UpdateAsync(a => new UserAddress { isDefault = false }, it => it.Id != info.Id && it.status == 0 && it.userId == info.userId);
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");
            return res;
        }

        #endregion

        #region 设置默认
        public async Task<ResultModel> frontEndSetDefault(QueryModel model, UserInfo user)
        {
            var res = new ResultModel();
            if (model == null || !model.queryId.HasValue) { res.msg = "参数错误"; return res; }
            var queryId = model.queryId.Value;
            if (queryId <= 0 || base.Count(it => it.userId == user.Id && it.Id == queryId && it.status == 0) == 0)
            {
                res.msg = "地址不存在";
                return res;
            }

            var info = await base.GetByIdAsync(model.queryId.Value);
            info.updateTime = DateTime.Now;
            info.isDefault = true;
            var isok = await base.UpdateAsync(info);
            await base.UpdateAsync(a => new UserAddress { isDefault = false }, it => it.Id != info.Id && it.status == 0 && it.userId == info.userId);

            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");
            return res;
        }
        #endregion

        #region 删除
        public async Task<ResultModel> frontEndDelete(DelModel del, UserInfo user)
        {
            var res = new ResultModel();
            if (del == null) { res.msg = "参数错误"; return res; }
            if (del.ids == null || del.ids.Length == 0) { res.msg = "请选择删除的地址"; return res; }
            if (user == null) { res.msg = "登录已失效，请重新登录"; return res; }

            var isok = await base.UpdateAsync(it => new UserAddress { status = 99 }, it => it.status == 0 && it.userId == user.Id && SqlFunc.ContainsArray(del.ids, it.Id));

            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "删除" + (isok ? "成功" : "失败");
            return res;
        }

        #endregion
    }
}
