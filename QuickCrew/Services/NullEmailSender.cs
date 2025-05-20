using Microsoft.AspNetCore.Identity.UI.Services;

namespace QuickCrew.Services
{
    public class NullEmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return;
        }
    }
}