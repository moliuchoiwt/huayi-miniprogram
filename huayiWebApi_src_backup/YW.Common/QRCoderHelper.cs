using QRCoder;
using System;
using System.Drawing;

namespace YW.Common
{
    public class QRCoderHelper
    {
        #region 普通二维码
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url">存储内容</param>
        /// <param name="pixel">像素大小</param>
        /// <returns></returns>
        public static Bitmap GetPTQRCode(string url, int pixel)
        {
            QRCodeGenerator generator = new QRCodeGenerator();
            QRCodeData codeData = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.M, true);
            QRCoder.Base64QRCode qrcode = new QRCoder.Base64QRCode(codeData);
            var qrImage = qrcode.GetGraphic(pixel, Color.Black, Color.White, true);
            return ConvertToBitmap(qrImage);
        }
        #endregion

        private static Bitmap ConvertToBitmap(string base64String)
        {
            // 将Base64字符串转换为字节数组
            byte[] imageBytes = Convert.FromBase64String(base64String);

            // 使用字节数组创建Bitmap对象
            using (var ms = new System.IO.MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                ms.Write(imageBytes, 0, imageBytes.Length);
                return new Bitmap(ms);
            }
        }
    }
}
