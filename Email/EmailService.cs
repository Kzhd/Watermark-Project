using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Email.DEO;
using Storage;
using Utils.Common;
using MimeKit;
using MailKit.Net.Smtp;
using System.Net.Security;
using MailKit.Security;

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
            string methodName = MethodBase.GetCurrentMethod().Name;
            MimeMessage message  = await ComposeMessage(emailDetails);
            using(var client = new SmtpClient())
            {
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, SslPolicyErrors sslPolicyErrors){ return true;};
                await client.ConnectAsync(_context.ApplicationConfiguration["Email:EmailServer"],Convert.ToInt32(_context.ApplicationConfiguration["Email:SMTPPort"]),SecureSocketOptions.Auto).ConfigureAwait(false);
                await client.AuthenticateAsync(_emailUsername,_emailPassword,_tokenSource.Token).ConfigureAwait(false);
                client.MessageSent += EmailSendAcknowledgement;
                await client.SendAsync(message).ConfigureAwait(false);
                client.Disconnect(true);
                return "Email Delivered Successfully";
            }
        }

        private async Task<MimeMessage> ComposeMessage(EmailDetails emailDE)
        {
            MimeMessage mailMessage = new MimeMessage();

            mailMessage.From.Add(new MailboxAddress(_emailUsername.Trim()));

            mailMessage.To.AddRange(emailDE.Recipients.To.Select(to=> new MailboxAddress(to.Trim())));

            if(emailDE.Recipients.Bcc !=null && emailDE.Recipients.Bcc.Any())
            {
                mailMessage.Bcc.AddRange(emailDE.Recipients.Bcc.Select(bcc=> new MailboxAddress(bcc.Trim())));
            }

            if(emailDE.Recipients.Cc !=null && emailDE.Recipients.Cc.Any())
            {
                mailMessage.Cc.AddRange(emailDE.Recipients.Cc.Select(cc=> new MailboxAddress(cc.Trim())));
            }

            mailMessage.Subject = emailDE.Subject;
            mailMessage.Priority = (MessagePriority)emailDE.Priority;

            var builder = new BodyBuilder
            {
                HtmlBody = emailDE.Body
            };

            if(emailDE.Attachments !=null && emailDE.Attachments.Any())
            {
                foreach (var file in emailDE.Attachments)
                {
                    string fileId = file.Key;
                    string fileName = file.Value;

                    if(string.IsNullOrEmpty(fileId) || string.IsNullOrEmpty(fileName))
                    return null;

                    var fileAttachment = await _blobStorageService.DownloadAsync(_context.CurrentClientId.ToString(),fileId,
                                                Path.GetExtension(fileName).Replace('.',' ').Trim(),_tokenSource.Token).ConfigureAwait(false);
                    builder.Attachments.Add(fileName, ((MemoryStream)fileAttachment).ToArray());
                }
            }

            mailMessage.Body = builder.ToMessageBody();
            return mailMessage;
        }


        private void EmailSendAcknowledgement(object sender, MailKit.MessageSentEventArgs e)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format($"Log Generated at: {System.DateTime.UtcNow.ToString()}"));
            sb.AppendLine(string.Format($"Email Form: {e.Message.From.FirstOrDefault()}"));
            sb.AppendLine(string.Format($"Email To: {string.Join(';',_emailDetails.Recipients.To.Select(t=>t))}"));

            if(_emailDetails.Recipients.Bcc !=null && _emailDetails.Recipients.Bcc.Any())
            {
                sb.Append(string.Format($"Email Bcc: {string.Join(';',_emailDetails.Recipients.Bcc.Select(bcc=> bcc))}"));
            }

            if(_emailDetails.Recipients.Cc !=null && _emailDetails.Recipients.Cc.Any())
            {
                sb.Append(string.Format($"Email Cc: {string.Join(';',_emailDetails.Recipients.Cc.Select(cc=> cc))}"));
            }

            sb.Append(string.Format($"Email Priority: {_emailDetails.Priority}"));    
            sb.Append(string.Format($"Email Subject: {_emailDetails.Subject}"));
            sb.Append(string.Format($"Response From Server: {e.Response}"));    

            _context.ApplicationLogger.LogInformation("Acknowledgement received from SMTP server {@EmailAcknowledgement}",$"{sb.ToString()}");      

        }
    
        private void Dispose()
        {
            _emailDetails = null;
        }
    }
}
