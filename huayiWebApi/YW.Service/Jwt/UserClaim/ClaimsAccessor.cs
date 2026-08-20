using System.Security.Claims;

namespace YW.Service.Jwt.UserClaim
{
    public class ClaimsAccessor : IClaimsAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal UserPrincipal
        {
            get
            {
                return _httpContextAccessor.HttpContext.User;
            }
        }
        public string UserName
        {
            get
            {
                if (UserPrincipal.Identity.IsAuthenticated)
                {
                    return UserPrincipal.Claims.First(x => x.Type == ClaimTypes.Name).Value;
                }
                else return "";
            }
        }
        public long UserId
        {
            get
            {
                if (UserPrincipal.Identity.IsAuthenticated)
                {
                    return long.Parse(UserPrincipal.Claims.First(x => x.Type == ClaimTypes.PrimarySid).Value);
                }
                else return 0;
            }

        }
        public string UserRole
        {
            get
            {
                if (UserPrincipal.Identity.IsAuthenticated)
                {
                    return UserPrincipal.Claims.First(x => x.Type == ClaimTypes.Role).Value;
                }
                else return "";
            }
        }
    }
}