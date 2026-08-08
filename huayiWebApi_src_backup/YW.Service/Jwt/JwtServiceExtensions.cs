using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace YW.Service.Jwt
{
    public static class JwtServiceExtensions
    {
        public static IServiceCollection AddJwtService(this IServiceCollection services, IConfiguration configuration)
        {
            //绑定appsetting中的jwtsetting
            services.Configure<JwtSetting>(configuration.GetSection(nameof(JwtSetting)));

            //注册jwtservice
            services.AddSingleton<JwtService>();
            //注册IHttpContextAccessor
            services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IClaimsAccessor, ClaimsAccessor>();

            var jwtConfig = configuration.GetSection("JwtSetting");

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["SecurityKey"])),

                        ValidateIssuer = true,
                        ValidIssuer = jwtConfig["Issuer"],

                        ValidateAudience = true,
                        ValidAudience = jwtConfig["Audience"],

                        //总的Token有效时间 = JwtRegisteredClaimNames.Exp + ClockSkew ；
                        RequireExpirationTime = true,
                        ValidateLifetime = true,// 是否验证Token有效期，使用当前时间与Token的Claims中的NotBefore和Expires对比.同时启用ClockSkew 
                        ClockSkew = TimeSpan.Zero //注意这是缓冲过期时间，总的有效时间等于这个时间加上jwt的过期时间，如果不配置，默认是5分钟

                    };
                    o.Events = new JwtBearerEvents
                    {
                        //此处为权限验证失败后触发的事件
                        OnChallenge = context =>
                        {
                            #region 失效清理Redis数据
                            try
                            {
                                string bearer = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                                if (!string.IsNullOrWhiteSpace(bearer) && bearer.Contains("Bearer") && bearer.Contains("."))
                                {

                                    var token = bearer.Substring(6).Trim();

                                    var tokenObj = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(token);

                                    var claimsIdentity = new System.Security.Claims.ClaimsIdentity(tokenObj.Claims);
                                    var claimsPrincipal = new System.Security.Claims.ClaimsPrincipal(claimsIdentity);

                                    var uid = GetIntClaimsValue(claimsPrincipal);
                                    var role = GetStringClaimsValue(claimsPrincipal);
                                    bool isok = false;
                                    switch (role)
                                    {
                                        case "sys":
                                            isok = RedisCacheHelper.Remove(CommonHelper.GetRedisAdminTokenKeyName(uid));
                                            break;
                                        case "api":
                                            isok = RedisCacheHelper.Remove(CommonHelper.GetRedisUserTokenKeyName(uid));
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Error("jwt失效清理Redis数据", ex);
                            }
                            #endregion

                            //此处代码为终止.Net Core默认的返回类型和数据结果，这个很重要哦，必须
                            context.HandleResponse();
                            //自定义自己想要返回的数据结果，我这里要返回的是Json对象，通过引用Newtonsoft.Json库进行转换
                            var payload = JsonConvert.SerializeObject(new { code = 501, msg = "Token无效", data = "" });

                            //自定义返回的数据类型
                            context.Response.ContentType = "application/json";
                            //自定义返回状态码，默认为401 我这里改成 200
                            context.Response.StatusCode = StatusCodes.Status200OK;
                            //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            //输出Json数据结果
                            context.Response.WriteAsync(payload);
                            return Task.FromResult(0);
                        }
                    };
                });
            return services;
        }

        public static int GetIntClaimsValue(this System.Security.Claims.ClaimsPrincipal claimsPrincipal, string ctype = System.Security.Claims.ClaimTypes.PrimarySid)
        {
            try
            {
                var claim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == ctype);
                if (claim == null || string.IsNullOrEmpty(claim.Value))
                {
                    return 0;
                }

                return int.Parse(claim.Value);
            }
            catch
            {
                return 0;
            }
        }

        public static string GetStringClaimsValue(this System.Security.Claims.ClaimsPrincipal claimsPrincipal, string ctype = System.Security.Claims.ClaimTypes.Role)
        {
            try
            {
                var claim = claimsPrincipal?.Claims.FirstOrDefault(c => c.Type == ctype);
                if (claim == null || string.IsNullOrEmpty(claim.Value))
                {
                    return "";
                }

                return claim.Value;
            }
            catch
            {
                return "";
            }
        }
    }
}
