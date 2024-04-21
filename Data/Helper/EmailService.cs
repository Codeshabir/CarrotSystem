using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace CarrotSystem.Helpers
{
    public interface IEmailService
    {
        void ExecuteWithFile(string from, string email, string subject, string message, MemoryStream file, string fileName);
        void Execute(string from, string email, string subject, string message);
        void ExecuteByHTML(string from, string email, string ccEmail, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly ILogger<IEmailService> _logger;
        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<IEmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public EmailSettings _emailSettings { get; }

        public void Execute(string from, string email, string subject, string message)
        {
            try
            {
                string fromEmail = "admin@myproductionsystem.au";

                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(fromEmail, "Mercorella Group")
                };

                foreach (var address in email.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mail.To.Add(address);
                }

                _logger.LogError(mail.To.ToString());

                //mail.CC.Add(new MailAddress(_emailSettings.CcEmail));

                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.Normal;

                using (SmtpClient smtp = new SmtpClient(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort))
                {
                    smtp.Credentials = new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword);
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }
        }

        public void ExecuteWithFile(string from, string email, string subject, string message, MemoryStream file, string fileName)
        {
            try
            {
                string fromEmail = "admin@myproductionsystem.au";

                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(fromEmail, "Mercorella Group")
                };

                foreach (var address in email.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mail.To.Add(address);
                }

                _logger.LogError(mail.To.ToString());

                //mail.CC.Add(new MailAddress(_emailSettings.CcEmail));

                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.Normal;
                mail.Attachments.Add(new Attachment(file, fileName));

                using (SmtpClient smtp = new SmtpClient(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort))
                {
                    smtp.Credentials = new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword);
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }
        }

        public void ExecuteByHTML(string from, string email, string ccEmail, string subject, string message)
        {
            try
            {
                string fromEmail = "admin@myproductionsystem.au";

                MailMessage mail = new MailMessage()
                {
                    From = new MailAddress(fromEmail, "Carrot")
                };

                foreach (var address in email.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mail.To.Add(address);
                }

                _logger.LogError(mail.To.ToString());

                if (!ccEmail.Equals("None"))
                {
                    foreach (var cc in ccEmail.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mail.CC.Add(cc);
                    }

                    _logger.LogError(mail.CC.ToString());
                }

                mail.Subject = subject;
                mail.Body = message;
                mail.IsBodyHtml = true;
                mail.Priority = MailPriority.Normal;

                using (SmtpClient smtp = new SmtpClient(_emailSettings.PrimaryDomain, _emailSettings.PrimaryPort))
                {
                    smtp.Credentials = new NetworkCredential(_emailSettings.UsernameEmail, _emailSettings.UsernamePassword);
                    //smtp.EnableSsl = false;
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.StackTrace);
            }
        }

    }
}
