using LinqKit;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysSysUserController
    /// </summary>
    public class SysUserController : BaseController
    {

        private readonly ISysUserService _adminUserService;

        private readonly SysUserMapper mapper = new();
        private readonly ApiConfigDtoMapper apiConfigDtoMapper = new();

        public SysUserController(IClaimsAccessor claimsAccessor,
            SysUserService adminUserService)
        {
            _adminUserService = adminUserService;
            _claimsAccessor = claimsAccessor;
        }

        #region 获取登陆用户信息
        [HttpPost]
        public ResultModel AdminInfo() => _adminUserService.GetAdmin(admin);
        #endregion

        #region adminUser操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(QueryModel queryModel)
        {
            var res = new ResultModel();
            var p = new PageModel() { PageIndex = queryModel.pageNum, PageSize = queryModel.pageSize };
            var exWhere = PredicateBuilder.New<SysUser>();
            exWhere.And(a => !a.delFlag);
            if (!string.IsNullOrWhiteSpace(queryModel.queryName))
            {
                queryModel.queryName = queryModel.queryName.Trim();
                int tId = 0;
                if (int.TryParse(queryModel.queryName, out tId)) exWhere.And(a => a.Id == tId);
                else exWhere.And(a => a.userName.Contains(queryModel.queryName) || a.pwd.Contains(queryModel.queryName) ||
                a.nickName.Contains(queryModel.queryName)
                || a.phone.Contains(queryModel.queryName));
            }
            if (queryModel.startTime.HasValue)
            {
                exWhere.And(a => a.createTime >= queryModel.startTime.Value);
            }
            if (queryModel.endTime.HasValue)
            {
                exWhere.And(a => a.createTime <= queryModel.endTime.Value);
            }
            var list = new List<SysUserView>();
            var data = await _adminUserService.GetPageListAsync(exWhere, p, it => new { it.Id, it.createTime }, OrderByType.Desc);
            if (data != null && data.Count > 0)
            {
                list = mapper.ToViewList(data);

                foreach (var item in list)
                {
                    item.avatar = GetUrl(item.avatar);
                    item.pwd = DESEncrypt.Decrypt(item.pwd);
                }
            }
            res.code = (int)ResultEnum.success;
            res.msg = "请求成功";
            res.data = new { total = p.TotalCount, items = list };
            return res;
        }


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(SysUser model)
        {
            var res = new ResultModel();
            if (string.IsNullOrWhiteSpace(model.pwd) || string.IsNullOrWhiteSpace(model.userName))
            {
                res.msg = "登录名和密码不能为空";
                return res;
            }
            if (model.pwd.Length < 6 && model.pwd.Length > 8)
            {
                res.msg = "密码为6-8位";
                return res;
            }
            model.avatar = model.avatar.Replace(PubConstant.Config.DomianStaticName, "");
            model.pwd = DESEncrypt.Encrypt(model.pwd);
            bool isok = false;
            if (model.Id > 0)
            {
                model.updateTime = DateTime.Now;
                isok = await _adminUserService.UpdateAsync(model);
            }
            else
            {
                model.createTime = DateTime.Now;
                model.updateTime = DateTime.Now;
                isok = await _adminUserService.InsertAsync(model);
            }
            res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
            res.msg = "操作" + (isok ? "成功" : "失败");

            return res;
        }


        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Delete(DelModel del)
        {
            var res = new ResultModel();

            if (del.ids != null && del.ids.Length > 0)
            {
                var isok = await _adminUserService.UpdateAsync(it => new SysUser { delFlag = true }, it => SqlFunc.ContainsArray(del.ids, it.Id));

                res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "删除" + (isok ? "成功" : "失败");
            }

            return res;

        }
        #endregion

        #region sysconfig操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> GetSysConfig()
        {
            var result = new ResultModel();

            var data = apiConfigDtoMapper.ToView(PubConstant.Config);
            data.SiteLogo = WebFileHelper.GetUrl(data.SiteLogo);
            data.videoUrl = WebFileHelper.GetUrl(data.videoUrl);

            result.code = (int)ResultEnum.success;
            result.msg = "ok";
            await Task.Run(() => { result.data = data; });
            return result;
        }


        /// <summary>
        ///修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ResultModel OperationSysConfig(ConfigView model)
        {
            var result = new ResultModel();
            if (model == null) { result.msg = "参数错误"; return result; }

            var config = apiConfigDtoMapper.ToModel(model);
            config.SiteLogo = config.SiteLogo.Replace(PubConstant.Config.DomianStaticName, "");
            config.videoUrl = config.videoUrl.Replace(PubConstant.Config.DomianStaticName, "");

            XmlHelper.SaveXml(config, $"{System.IO.Directory.GetCurrentDirectory()}/Config/apiConfig.xml");
            PubConstant.Config = XmlHelper.ReadXml<ApiConfigDto>($"{System.IO.Directory.GetCurrentDirectory()}/Config/apiConfig.xml");
            result.code = (int)ResultEnum.success;
            result.msg = "ok";
            return result;
        }

        #endregion

        #region 退出登录
        /// <summary>
        /// 获取首页图表数据
        /// </summary>
        [HttpPost]
        public ResultModel LoginOut()
        {
            ResultModel result = new ResultModel();
            RedisCacheHelper.Remove(CommonHelper.GetRedisAdminTokenKeyName(adminId));
            result.code = (int)ResultEnum.success;
            result.msg = "退出登录成功";
            return result;

        }
        #endregion
    }
}