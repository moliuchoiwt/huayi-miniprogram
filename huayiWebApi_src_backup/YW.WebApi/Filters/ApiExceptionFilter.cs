using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace YW.WebApi.Filters
{

    #region [Api action统一处理过滤器]

    /// <summary>
    /// Api action统一处理过滤器
    /// 处理正常返回值 {code:200,body:{}}
    /// </summary>
    public class ApiResponseFilterAttribute : ActionFilterAttribute
    {

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ActionArguments.Count > 0)
            {
                //模型验证
                if (!context.ModelState.IsValid)
                {
                    var result = new ResultModel()
                    {
                        code = (int)ResultEnum.fail,
                        msg = "参数错误:"
                    };
                    foreach (var keyName in context.ModelState.Keys)
                    {
                        result.msg += keyName + "|";
                    }
                    // foreach (var item in context.ModelState.Values)
                    // {
                    //     foreach (var error in item.Errors)
                    //     {
                    //         result.msg += error.ErrorMessage + "|";
                    //     }
                    // }
                    if (!string.IsNullOrWhiteSpace(result.msg)) result.msg = result.msg.TrimEnd('|');

                    context.Result = new JsonResult(result);
                }
            }
            base.OnActionExecuting(context);
        }

        /// <summary>
        /// 处理正常返回的结果对象，进行统一json格式包装
        /// 异常只能交由ExceptionFilterAttribute 去处理 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            //if (context.Result != null)
            //{
            //    var result = context.Result as ObjectResult;
            //    JsonResult newresult;
            //    if (context.Result is ObjectResult)
            //    {
            //        newresult = new JsonResult(new { code = 200, body = result.Value });
            //    }
            //    else if (context.Result is EmptyResult)
            //    {
            //        newresult = new JsonResult(new { code = 200, body = new { } });
            //    }
            //    else
            //    {
            //        throw new Exception($"未经处理的Result类型：{ context.Result.GetType().Name}");
            //    }
            //    context.Result = newresult;
            //}
            //base.OnActionExecuted(context);
        }
    }
    #endregion

    #region  [api异常统一处理过滤器]

    /// <summary>
    /// api异常统一处理过滤器
    /// 系统级别异常 500 应用级别异常501
    /// </summary>
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            context.Result = BuildExceptionResult(context.Exception);
            base.OnException(context);
        }

        /// <summary>
        /// 包装处理异常格式
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        private JsonResult BuildExceptionResult(Exception ex)
        {

            LogHelper.Error("api异常", ex);
            int code = 0;
            string message = "";
            string innerMessage = "";
            //应用程序业务级异常
            if (ex is ApplicationException)
            {
                code = 400;
                message = ex.Message;
            }
            else
            {
                // exception 系统级别异常，不直接明文显示的
                code = 400;
                message = ex.Message;
                innerMessage = ex.Message;
            }

            if (ex.InnerException != null && ex.Message != ex.InnerException.Message)
                innerMessage += "," + ex.InnerException.Message;

            return new JsonResult(new { code, msg = message, data = innerMessage });
        }
    }
    #endregion
}
