namespace YW.Service
{
    public partial interface ISysRoleService : IBaseRepository<SysRole>
    {
        Task<ResultModel> Operation(SysRoleView model);

    }
    public partial class SysRoleService : BaseRepository<SysRole>, ISysRoleService
    {
        private readonly SysRoleMapper _mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public SysRoleService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

        private readonly JwtService _jwtService;
        public SysRoleService(JwtService jwtService, IClaimsAccessor claimsAccessor)
        {
            _jwtService = jwtService;
            _claimsAccessor = claimsAccessor;
        }

        public async Task<ResultModel> Operation(SysRoleView model)
        {
            var res = new ResultModel();

            try
            {
                db.BeginTran();
                var adminId = (int)_claimsAccessor.UserId;
                var info = _mapper.ToView(model);
                bool isok = false;
                if (info.Id > 0)
                {
                    info.updateTime = DateTime.Now;
                    info.updateId = adminId;
                    isok = await sysRoleDb.UpdateAsync(new SysRole
                    {
                        Id = info.Id,
                        remark = info.remark,
                        roleName = info.roleName,
                        updateId = info.updateId,
                        updateTime = info.updateTime
                    });
                }
                else
                {
                    info.createTime = DateTime.Now;
                    info.createId = adminId;
                    info.updateTime = DateTime.Now;
                    info.updateId = adminId;
                    info.Id = await sysRoleDb.InsertReturnIdentityAsync(info);
                    isok = info.Id > 0;
                }

                #region 添加菜单权限
                if (isok)
                {
                    //删除旧权限数据
                    await sysRoleMenuDb.DeleteAsync(it => it.roleId == info.Id);
                    //添加新权限数据
                    if (model.menuIds != null && model.menuIds.Count > 0)
                    {
                        var roleList = new List<SysRoleMenu>();
                        foreach (var menuId in model.menuIds)
                        {
                            roleList.Add(new SysRoleMenu
                            {
                                roleId = info.Id,
                                menuId = menuId
                            });
                        }
                        await sysRoleMenuDb.InsertRangeAsync(roleList);
                    }
                }
                #endregion

                db.CommitTran();
                res.code = isok ? (int)ResultEnum.success : (int)ResultEnum.fail;
                res.msg = "操作" + (isok ? "成功" : "失败");
            }
            catch (Exception ex)
            {
                db.RollbackTran();
                res.msg = ex.Message;
            }

            return res;
        }

    }
}
