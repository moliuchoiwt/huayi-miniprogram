using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using YW.Service.Jwt.UserClaim;

namespace YW.WebApi.ApiControllers
{
    /// <summary>
    /// 用户
    /// </summary>
    [Authorize(Roles = "api")]
    public class UserController : BaseController
    {

        private readonly IUserInfoService _userInfoService;
        private readonly IFeedbackService _feedbackService;
        private readonly IUserAddressService _userAddressService;
        private readonly IBrowsesService _browsesService;
        private readonly ICollectionRecordService _collectionRecordService;
        private readonly IUserGradeService _userGradeService;
        private readonly IShopService _shopService;

        public UserController(UserInfoService userInfoService, IClaimsAccessor claimsAccessor,
            FeedbackService feedbackService,
            UserAddressService userAddressService,
            BrowsesService browsesService, CollectionRecordService collectionRecordService,
            UserGradeService userGradeService, ShopService shopService)
        {
            _claimsAccessor = claimsAccessor;
            _userInfoService = userInfoService;
            _feedbackService = feedbackService;
            _userAddressService = userAddressService;
            _browsesService = browsesService;
            _collectionRecordService = collectionRecordService;
            _userGradeService = userGradeService;
            _shopService = shopService;
        }

        #region 获取用户信息        
        public async Task<ResultModel> GetUser() => await _userInfoService.GetUser(user);
        public async Task<ResultModel> OperationUser(UserInfoView view) => await _userInfoService.FrontEndOperation(view, user);
        #endregion

        #region 我的好友/团队

        /// <summary>
        /// 我的直推好友
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> MyTree(QueryModel view)
        {
            view.userId = (int)_claimsAccessor.UserId;
            var res = await _userInfoService.MyTree(view);
            return res;
        }


        /// <summary>
        /// 用户直推好友
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> UserTree(QueryModel view)
        {
            var res = new ResultModel();
            if (!view.userId.HasValue || view.userId <= 0)
            {
                res.msg = "指定的用户ID参数错误";
                return res;
            }
            res = await _userInfoService.MyTree(view);
            return res;
        }
        #endregion

        #region 我的邀请码

        /// <summary>
        /// 我的邀请码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> MyInvitationCode()
        {
            var res = new ResultModel();
            var scene = $"userId={(int)_claimsAccessor.UserId}";
            //var strem = await WeChat.OpenClient.GetWxaCodeUnlimit(scene, "pages/index/index");
            var strem = new MemoryStream();
            var result = await Service.WeChat.OpenClient.GetWxCode(strem, $"pages/login/index?{scene}");
            if (result.ErrorCodeValue != 0 || strem == null || strem.Length <= 74)
            {
                res.msg = "生成微信小程序码失败";
                return res;
            }
            string base64 = Convert.ToBase64String(strem.ToArray());

            res.data = "data:image/png;base64," + base64;
            res.msg = "OK";
            res.code = (int)ResultEnum.success;
            return res;
        }
        #endregion

        #region 商品分享码

        /// <summary>
        /// 商品分享码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> MyGoodsQrCode(string path)
        {
            var res = new ResultModel();
            var strem = new MemoryStream();
            var result = await Service.WeChat.OpenClient.GetWxQrCode(strem, path, 120);
            if (result.ErrorCodeValue != 0 || strem == null || strem.Length <= 74)
            {
                res.msg = "生成商品分享码失败";
                return res;
            }
            string base64 = Convert.ToBase64String(strem.ToArray());

            res.data = base64;
            res.msg = "OK";
            res.code = (int)ResultEnum.success;
            return res;
        }
        #endregion

        #region 意见反馈
        /// <summary>
        /// 意见反馈
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> OperationFeedback(FeedbackView model) => await _feedbackService.frontEndInsertFeedback(model, user);
        #endregion

        #region 浏览记录    
        /// <summary>
        /// 用户浏览记录
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> BrowsesList(QueryModel query) => await _browsesService.frontEndList(query, user);
        #endregion

        #region 收藏列表
        /// <summary>
        /// 获取收藏列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> CollectionList(QueryModel queryModel) => await _collectionRecordService.frontEndList(queryModel, user);

        #endregion

        #region 地址管理
        /// <summary>
        /// 地址列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> AddressList(QueryModel view) => await _userAddressService.frontEndList(view, user);


        /// <summary>
        /// 新增/修改地址
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> EditAddress(UserAddressView model) => await _userAddressService.frontEndOperation(model, user);

        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> AddressDefault(QueryModel model) => await _userAddressService.frontEndSetDefault(model, user);

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> DelUserAddress(DelModel del) => await _userAddressService.frontEndDelete(del, user);
        #endregion

        #region 通过手机号获取用户信息

        /// <summary>
        /// 通过手机号获取用户信息
        /// </summary>
        [HttpPost]
        public async Task<ResultModel> ByMobileGetUser(LoginView view)
        {
            ResultModel result = new ResultModel();
            if (view == null || string.IsNullOrWhiteSpace(view.mobile))
            {
                result.msg = "联系号码不能为空";
                return result;
            }

            var user = await _userInfoService.GetSingleAsync(a => a.status == 0 && (a.mobile == view.mobile));
            if (user == null || user.Id <= 0)
            {
                result.msg = "用户不存在";
                return result;
            }
            result.data = new
            {
                UserId = user.Id,
                UserName = string.IsNullOrWhiteSpace(user.alias) ? user.nickName : user.alias,
                Avatar = GetFileUrl(user.avatar)
            };
            result.code = (int)ResultEnum.success;
            result.msg = "OK";
            return result;
        }

        #endregion

        #region 会员列表
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ResultModel> UserGradeList(QueryModel queryModel) => await _userGradeService.frontEndList(queryModel, user);

        #endregion

        #region 账号注销
        public async Task<ResultModel> CancelAccount() => await _userInfoService.CancelAccount(user);
        #endregion

        #region 获取店铺信息        
        public async Task<ResultModel> GetShop() => await _shopService.GetShop(user);
        #endregion

    }
}
