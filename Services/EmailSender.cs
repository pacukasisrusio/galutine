using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace galutine.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Console.WriteLine($"[EmailSender] To:{email} Subject:{subject}");
            return Task.CompletedTask;
        }
    }
}
