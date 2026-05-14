using System.Net;
using System.Net.Mail;

namespace SaigonRide.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config) => _config = config;

        public async Task SendOtpAsync(string toEmail, string otpCode)
        {
            var host = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var port = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var username = _config["Email:Username"]!;
            var password = _config["Email:Password"]!;
            var from = _config["Email:FromAddress"] ?? username;

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true,
            };

            var mail = new MailMessage(from, toEmail)
            {
                Subject = "SaigonRide – Xác minh tài khoản",
                IsBodyHtml = true,
                Body = $@"
                    <div style='font-family:Segoe UI,sans-serif;max-width:480px;margin:auto;'>
                        <h2 style='color:#2D2D2D;'>Chào mừng đến với SaigonRide!</h2>
                        <p>Mã OTP xác minh tài khoản của bạn là:</p>
                        <div style='font-size:36px;font-weight:bold;letter-spacing:8px;
                                    color:#C7A07A;text-align:center;padding:20px;
                                    background:#F5F2E8;border-radius:8px;margin:16px 0;'>
                            {otpCode}
                        </div>
                        <p style='color:#888;font-size:13px;'>Mã có hiệu lực trong <strong>5 phút</strong>. Không chia sẻ mã này cho ai.</p>
                    </div>"
            };

            await client.SendMailAsync(mail);
        }
    }
}
