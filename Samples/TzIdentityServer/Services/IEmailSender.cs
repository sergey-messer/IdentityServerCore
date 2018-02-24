using System.Threading.Tasks;

namespace TzIdentityServer.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
