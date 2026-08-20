namespace YW.DbContexts
{
    /// <summary>
    /// 返回数据
    /// </summary>
    public class ResultModel
    {

        /// <summary>
        /// 状态码
        /// </summary>
        public int code { get; set; } = (int)ResultEnum.fail;

        /// <summary>
        /// 提示
        /// </summary>
        public string msg { get; set; } = "请求出错,请稍后在试";
        /// <summary>
        /// 返回结果
        /// </summary>
        public object data { get; set; }
    }
}
