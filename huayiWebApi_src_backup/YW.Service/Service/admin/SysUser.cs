using System.Data;

namespace YW.Service
{
    public partial interface ISysUserService : IBaseRepository<SysUser>
    {
        ResultModel GetAdmin(SysUser admin);
        Task<ResultModel> Login(SysLoginView viewModel);
    }

    public partial class SysUserService : BaseRepository<SysUser>, ISysUserService
    {


        public SysUserService()
        {

        }

        private readonly JwtService _jwtService;
        private readonly SysUserMapper mapper = new();
        public SysUserService(JwtService jwtService)
        {
            _jwtService = jwtService;
        }



        public async Task<ResultModel> Login(SysLoginView view)
        {
            var res = new ResultModel();
            view.password = DESEncrypt.Encrypt(view.password);
            var user = await base.GetSingleAsync(a => a.userName == view.UserName && a.pwd == view.password);
            if (user == null || user.Id <= 0)
            {
                res.data = "";
                res.msg = "账号不存在或密码错误";
                return res;
            }
            user.Ip = CommonHelper.GetIP();
            user.lastLoginTime = DateTime.Now;
            await base.UpdateAsync(user);

            var RedisKeyName = CommonHelper.GetRedisAdminTokenKeyName(user.Id);
            var token = RedisCacheHelper.GetStringValue(RedisKeyName);
            if (string.IsNullOrWhiteSpace(token))
            {
                var jwtata = new JwtData
                {
                    Id = user.Id,
                    Name = user.nickName,
                    RoleName = "sys"
                };
                token = _jwtService.BuildToken(_jwtService.BuildClaims(jwtata), 24);
                RedisCacheHelper.SetStringValue(RedisKeyName, token, 24);
            }
            //else
            //{
            //    RedisCacheHelper.Replace(RedisKeyName, token, 24);//重置时间
            //}

            AdminLogDb.Insert(new AdminLog { adminId = user.Id, name = view.UserName, clientIp = Common.CommonHelper.GetIP(), createTime = DateTime.Now, contents = $"{view.UserName}请求登录" });
            res.code = (int)ResultEnum.success;
            res.msg = "ok";
            res.data = token;
            return res;
        }



        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <returns></returns>
        public ResultModel GetAdmin(SysUser admin)
        {
            var result = new ResultModel();
            if (admin != null && admin.Id > 0)
            {
                result.code = (int)ResultEnum.success;
                result.msg = "请求成功";
                result.data = new { name = admin.nickName, avatar = admin.avatar };
            }
            return result;
        }

        public async Task<ResultModel> AdminIndexData()
        {
            ResultModel result = new ResultModel();

            var ds = await db.Ado.GetDataSetAllAsync("exec pro_indexView");

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


    }
}
