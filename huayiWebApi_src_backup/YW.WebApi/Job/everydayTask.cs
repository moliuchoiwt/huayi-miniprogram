using System.Threading.Tasks;

namespace YW.WebApi.Job
{
    public class everydayTask : IJob
    {
        private readonly IUserInfoService _userService;
        public everydayTask(UserInfoService userService)
        {
            _userService = userService;
        }

        public Task Execute(IJobExecutionContext context)
        {
            //LogHelper.Info("每天执行的任务");
            //await _userService.TimedTaskFun();
            return Task.Run(() =>
            {

            });
        }
    }
}
