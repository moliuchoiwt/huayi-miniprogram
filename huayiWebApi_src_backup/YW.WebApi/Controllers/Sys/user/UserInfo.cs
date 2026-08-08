using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace YW.WebApi.SysControllers
{
    /// <summary>
    /// 控制器层 SysUserInfoController
    /// </summary>
    public class SysUserInfoController : BaseController
    {

        private readonly IUserInfoService _userInfoService;
        private readonly UserInfoMapper mapper = new();
        public SysUserInfoController(UserInfoService userInfoService)
        {
            _userInfoService = userInfoService;
        }

        #region userInfo操作
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> List(UserInfoQuery queryModel) => await _userInfoService.backEndList(queryModel);


        /// <summary>
        /// 新增/修改
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> Operation(UserInfoView model)
        {
            var res = new ResultModel();
            var info = mapper.ToModel(model);
            bool isok = false;
            if (info.Id > 0)
            {
                info.updateTime = DateTime.Now;
                isok = await _userInfoService.UpdateAsync(info);
            }
            else
            {
                info.createTime = DateTime.Now;
                info.updateTime = DateTime.Now;
                // isok = await _userInfoService.Insert(info);
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
        public async Task<ResultModel> DelUserInfo(DelModel del)
        {
            var res = new ResultModel();

            if (del.ids != null && del.ids.Length > 0)
            {
                var isok = await _userInfoService.UpdateAsync(it => new UserInfo { status = 99 }, it => SqlFunc.ContainsArray(del.ids, it.Id));

                res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "删除" + (isok ? "成功" : "失败");
            }

            return res;

        }
        #endregion
    }
}