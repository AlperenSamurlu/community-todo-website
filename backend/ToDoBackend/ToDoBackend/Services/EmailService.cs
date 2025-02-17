using System.Net;
using System.Net.Mail;

namespace ToDoBackend.Services
{
    public class EmailService
    {
        

        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;

        public EmailService(IConfiguration config)
        {
            _smtpServer = config["SmtpSettings:Server"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(config["SmtpSettings:Port"] ?? "587");
            _senderEmail = config["SmtpSettings:SenderEmail"];
            _senderPassword = config["SmtpSettings:Password"];
        }


        public void SendVerificationCode(string recipientEmail, string verificationCode)
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail),
                    Subject = "Email Verification Code",
                    Body = $"Your verification code is: {verificationCode}",
                    IsBodyHtml = false
                };

                mailMessage.To.Add(recipientEmail);

                client.Send(mailMessage);
            }
        }
        // Yeni Görev Bildirimi Metodu
        public void SendTaskNotification(string recipientEmail, string communityName, string taskTitle, string taskCreator)
        {
            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_senderEmail),
                        Subject = "New Task Assigned",
                        Body = $"Hello,\n\nA new task titled '{taskTitle}' has been assigned in the {communityName} community by {taskCreator}.\n\nBest regards!",
                        IsBodyHtml = false
                    };

                    mailMessage.To.Add(recipientEmail);
                    client.Send(mailMessage);
                    Console.WriteLine($"Email sent successfully: {recipientEmail}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Email sending error: {ex.Message}");
            }
        }
        // Yeni Şifre Sıfırlama Kodu Gönderme Metodu
        public void SendResetCode(string recipientEmail, string resetCode)
        {
            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_senderEmail),
                        Subject = "Password Reset Code",
                        Body = $"Hello,\n\nYour password reset code is: {resetCode}\n\nThis code is valid for 10 minutes.\n\nBest regards!",
                        IsBodyHtml = false
                    };

                    mailMessage.To.Add(recipientEmail);
                    client.Send(mailMessage);
                    Console.WriteLine($"Password reset code sent successfully: {recipientEmail}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Password reset code submission error: {ex.Message}");
            }
        }
    }
}
