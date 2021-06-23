using System.Threading.Tasks;
using Email.DEO;

namespace Email
{
    public interface IEmailService
    {
        Task<string> SendEmail(EmailDetails emailDetails);
    }
}