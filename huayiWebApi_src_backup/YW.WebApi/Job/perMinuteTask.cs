using System.Threading.Tasks;

namespace YW.WebApi.Job
{
    public class perMinuteTask : IJob
    {
        private readonly ITaskOrderService _taskOrderService;
        public perMinuteTask(TaskOrderService taskOrderService)
        {
            _taskOrderService = taskOrderService;
        }

        public Task Execute(IJobExecutionContext context)
        {
            // LogHelper.Info("每分钟执行的任务");            
            return Task.Run(async () =>
            {
                await _taskOrderService.TimedTaskFun();
            });
        }
    }
}
