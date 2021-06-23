using System;
using System.Threading;
using System.Threading.Tasks;
using Email.DEO;
using Storage;
using Utils.Common;

namespace Email
{
    public class EmailService : IEmailService
    {

        private readonly ApplicationContext _context;
        private readonly IBlobStorageService _blobStorageService;
        private readonly string _emailUsername;
        private readonly string _emailPassword;
        private readonly CancellationTokenSource _tokenSource;
        private EmailDetails _emailDetails;
        private readonly string _className;

        public EmailService(ApplicationContext context, IBlobStorageService blobStorageService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _emailUsername ="Email Username";
            _emailPassword = "Email Password";
            _className = this.GetType().Name;
            _tokenSource = new CancellationTokenSource();
        }

        public async Task<string> SendEmail(EmailDetails emailDetails)
        {
            throw new System.NotImplementedException();
        }
    }
}
