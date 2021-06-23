using System.Threading.Tasks;
using Email.DEO;
using Utils.Common;

namespace Email
{
    public class EmailService : IEmailService
    {
        public EmailService(ApplicationContext context)
        {

        }

        public async Task<string> SendEmail(EmailDetails emailDetails)
        {
            throw new System.NotImplementedException();
        }
    }
}
