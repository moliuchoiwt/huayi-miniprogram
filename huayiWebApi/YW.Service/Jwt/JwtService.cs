using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace YW.Service.Jwt
{
    public class JwtService
    {
        private readonly JwtSetting _jwtSetting;
        public TimeSpan _tokenLifeTime;

        public JwtService(IOptions<JwtSetting> options)
        {
            _jwtSetting = options.Value;
            _tokenLifeTime = TimeSpan.FromMinutes(options.Value.LifeTime);
        }
        /*
             iss (issuer)：签发人
             exp (expiration time)：过期时间
             sub (subject)：主题
             aud (audience)：受众
             nbf (Not Before)：生效时间
             iat (Issued At)：签发时间
             jti (JWT ID)：编号
             */

        /// <summary>
        /// 生成身份信息
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="roleName">登录时的角色</param>
        /// <returns></returns>
        public Claim[] BuildClaims(JwtData userData)
        {
            // 配置用户标识
            var userClaims = new Claim[]
            {
                new Claim(ClaimTypes.PrimarySid,userData.Id.ToString()),//id
                new Claim(ClaimTypes.Name,userData.Name.ToString()),//name
                new Claim(ClaimTypes.Role,userData.RoleName),//rolename
            };
            return userClaims;
        }

        /// <summary>
        /// 生成jwt令牌
        /// </summary>
        /// <param name="claims">自定义的claim</param>
        /// <param name="expiresHour">过期时间（小时）</param>
        /// <returns></returns>
        public string BuildToken(Claim[] claims, int expiresHour = 0)
        {
            var nowTime = DateTime.Now;
            var exTime = nowTime.Add(_tokenLifeTime);
            if (expiresHour > 0) exTime = nowTime.AddHours(expiresHour);
            var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSetting.SecurityKey)), SecurityAlgorithms.HmacSha256);
            JwtSecurityToken tokenkey = new JwtSecurityToken(
                issuer: _jwtSetting.Issuer,
                audience: _jwtSetting.Audience,
                claims: claims,
                notBefore: nowTime,
                expires: exTime,
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenkey);
        }
    }
}
