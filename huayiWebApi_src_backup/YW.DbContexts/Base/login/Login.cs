using System.ComponentModel.DataAnnotations;

namespace YW.DbContexts
{
    public class LoginView
    {
        public string code { get; set; }
        public string rawData { get; set; }
        public string signature { get; set; }
        public string encryptedData { get; set; }
        public string iv { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string mobile { get; set; }
        /// <summary>
        /// 验证码
        /// </summary>
        public string smscode { get; set; }
        /// <summary>
        /// 推荐用户id
        /// </summary>
        public int parentId { get; set; }

        /// <summary>
        /// 微信中用来获取手机号的code
        /// </summary>
        public string getWxPhoneCode { get; set; } = string.Empty;


    }

    /// <summary>
    /// 手机号登录
    /// </summary>
    public class MobileLoginView
    {
        /// <summary>
        /// 推荐用户id
        /// </summary>
        public int ParentId { get; set; }
        /// <summary>
        /// 手机号码
        /// </summary>
        [Display(Name = "手机号")]
        [Required(ErrorMessage = "{0}必填")]
        [StringLength(11, ErrorMessage = "不能超过{0}个字符")]
        public string mobile { get; set; }
        /// <summary>
        /// 密码
        /// </summary> 
        [Display(Name = "登录密码")]
        [Required(ErrorMessage = "{0}必填")]
        [StringLength(16, ErrorMessage = "不能超过{0}个字符")]
        [RegularExpression(@"^[a-zA-Z0-9_]{4,16}$", ErrorMessage = "只能包含字符、数字和下划线")]
        public string password { get; set; }
    }


    public class SysLoginView
    {
        /// <summary>
        /// 登录账号
        /// </summary>

        [Display(Name = "登录账号")]
        [Required(ErrorMessage = "{0}必填")]
        [StringLength(16, ErrorMessage = "不能超过{0}个字符")]
        [RegularExpression(@"^[a-zA-Z0-9_]{4,16}$", ErrorMessage = "只能包含字符、数字和下划线")]
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary> 
        [Display(Name = "登录密码")]
        [Required(ErrorMessage = "{0}必填")]
        [StringLength(16, ErrorMessage = "不能超过{0}个字符")]
        [RegularExpression(@"^[a-zA-Z0-9_]{4,16}$", ErrorMessage = "只能包含字符、数字和下划线")]
        public string password { get; set; }

    }
}
