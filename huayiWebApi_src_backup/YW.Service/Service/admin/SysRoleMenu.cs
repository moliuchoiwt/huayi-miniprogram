namespace YW.Service
{
    public partial interface ISysRoleMenuService : IBaseRepository<SysRoleMenu>
    {


    }
    public partial class SysRoleMenuService : BaseRepository<SysRoleMenu>, ISysRoleMenuService
    {
        private readonly SysRoleMenuMapper _mapper = new();
        private readonly IClaimsAccessor _claimsAccessor;

        public SysRoleMenuService(IClaimsAccessor claimsAccessor)
        {
            _claimsAccessor = claimsAccessor;
        }

        private readonly JwtService _jwtService;
        public SysRoleMenuService(JwtService jwtService, IClaimsAccessor claimsAccessor)
        {
            _jwtService = jwtService;
            _claimsAccessor = claimsAccessor;
        }


    }
}
