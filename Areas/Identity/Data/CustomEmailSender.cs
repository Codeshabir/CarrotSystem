using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CarrotSystem.Areas.Identity.Data

{
    public class CustomEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Implement your custom email sending logic here
            // This could involve using SMTP client, third-party email service, etc.
            return Task.CompletedTask;
        }
    }
}
