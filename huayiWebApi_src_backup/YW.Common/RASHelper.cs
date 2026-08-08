using System;
using System.Security.Cryptography;
using System.Text;
using XC.RSAUtil;

namespace YW.Common
{
    /// <summary>
    /// RSA加密
    /// </summary>
    public class RASHelper
    {

        #region 公钥加密

        /// <summary>
        /// 公钥加密
        /// </summary>
        /// <param name="text">需加密文本</param>
        /// <param name="publicKey">公钥</param>
        /// <param name="fOAEP">fOAEP设置为true(填充方案:RSAES-OAEP)-微信rsa </param>
        /// <returns></returns>
        public static string RSAEncrypt(string text, string publicKey, bool fOAEP = false)
        {
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(RsaKeyConvert.PublicKeyPemToXml(publicKey));
                var buff = rsa.Encrypt(Encoding.UTF8.GetBytes(text), fOAEP);

                return Convert.ToBase64String(buff);
            }
        }
        #endregion
    }
}
