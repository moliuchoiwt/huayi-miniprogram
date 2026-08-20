using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace YW.WebApi.SysControllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("sysapi/[controller]/[action]")]
    [ApiExplorerSettings(GroupName = "v2")]
    public class SysLoginController : ControllerBase
    {
        protected readonly ISysUserService _adminService;
        public SysLoginController(SysUserService adminUserService)
        {
            _adminService = adminUserService;
        }

        [HttpPost]
        public async Task<ResultModel> Login(SysLoginView viewModel) => await _adminService.Login(viewModel);

    }
}
