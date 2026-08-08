using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace YW.Common
{
    /// <summary>
    /// 郵件發送客戶端（管理員通知用）
    /// 配置節點：appsettings.json -> EmailSetting
    /// 默認使用 Gmail SMTP（studioofjoyhk@gmail.com 自寄）。
    /// 若要換 SendGrid / 騰訊企業郵，只要改下方對應配置即可，代碼無需改動。
    /// </summary>
    public class EmailClient
    {
        /// <summary>
        /// 發送一封郵件。任何異常都會被吞掉並返回 false，不影響主業務流程。
        /// （圖片篩查、上傳等動作不應因「發郵件失敗」而中斷）
        /// </summary>
        /// <param name="to">收件人</param>
        /// <param name="subject">標題</param>
        /// <param name="bodyHtml">HTML 正文</param>
        /// <param name="attachments">附件（可選，用於夾帶上傳圖片）</param>
        public static async Task<bool> SendAsync(
            string to,
            string subject,
            string bodyHtml,
            params (string fileName, byte[] data)[] attachments)
        {
            try
            {
                var host = ConfigHelper.GetSectionValue("EmailSetting:SmtpHost");
                var port = ConfigHelper.GetSectionValue("EmailSetting:SmtpPort");
                var user = ConfigHelper.GetSectionValue("EmailSetting:UserName");
                var pwd = ConfigHelper.GetSectionValue("EmailSetting:Password");
                var fromName = ConfigHelper.GetSectionValue("EmailSetting:FromName");
                var enableSsl = ConfigHelper.GetSectionValue("EmailSetting:EnableSsl");

                if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pwd))
                {
                    // 未配置郵件，靜默跳過（不拋異常）
                    return false;
                }

                using var msg = new MailMessage();
                msg.From = new MailAddress(user, fromName ?? user);
                // 支持多收件人（以逗號分隔）
                foreach (var addr in to.Split(',', ';'))
                {
                    if (!string.IsNullOrWhiteSpace(addr))
                        msg.To.Add(addr.Trim());
                }
                msg.Subject = subject;
                msg.SubjectEncoding = System.Text.Encoding.UTF8;
                msg.Body = bodyHtml;
                msg.BodyEncoding = System.Text.Encoding.UTF8;
                msg.IsBodyHtml = true;

                if (attachments != null)
                {
                    foreach (var (fileName, data) in attachments)
                    {
                        if (data != null && data.Length > 0)
                            msg.Attachments.Add(new Attachment(new System.IO.MemoryStream(data), fileName ?? "image.png"));
                    }
                }

                using var client = new SmtpClient(host, int.Parse(port ?? "587"));
                client.Credentials = new NetworkCredential(user, pwd);
                client.EnableSsl = !"false".Equals(enableSsl, StringComparison.OrdinalIgnoreCase);
                client.DeliveryMethod = SmtpDeliveryMethod.Network;

                await client.SendMailAsync(msg);
                return true;
            }
            catch (Exception ex)
            {
                // 記日誌但不拋出，避免阻塞主流程
                Console.WriteLine($"[EmailClient] 發送郵件失敗: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 發送純文字郵件（便捷方法）
        /// </summary>
        public static async Task<bool> SendTextAsync(string to, string subject, string body)
        {
            return await SendAsync(to, subject, body.Replace("\n", "<br/>"));
        }
    }
}
