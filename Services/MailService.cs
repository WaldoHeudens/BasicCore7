    using MailKit.Net.Smtp;
    using Microsoft.AspNetCore.Identity.UI.Services;
    using Microsoft.Extensions.Options;
    using MimeKit;
    using MimeKit.Text;
    using NETCore.MailKit.Infrastructure.Internal;

    namespace BasicCore7.Services
    {
        public class MailKitEmailSender : IEmailSender
        {
            public MailKitEmailSender(IOptions<MailKitOptions> options)
            {
                this.Options = options.Value;
            }

            public MailKitOptions Options { get; set; }

            public Task SendEmailAsync(string email, string subject, string message)
            {
                return Execute(email, subject, message);
            }

            public Task Execute(string to, string subject, string message)
            {
                // create message
                var email = new MimeMessage();
                email.Sender = MailboxAddress.Parse(Globals.EmailEmail);
                if (!string.IsNullOrEmpty(Globals.EmailSender))
                    email.Sender.Name = Globals.EmailSender;
                email.From.Add(email.Sender);
                email.To.Add(MailboxAddress.Parse(to));
                email.Subject = subject;
                email.Body = new TextPart(TextFormat.Html) { Text = message };

                // send email
                using (var smtp = new SmtpClient())
                {
                    smtp.Connect(Globals.EmailServer, Globals.EmailPort, Globals.EmailSecurity);
                    smtp.Authenticate(Globals.EmailAccount, Globals.EmailPassword);
                    smtp.Send(email);
                    smtp.Disconnect(true);
                }
                return Task.FromResult(true);
            }
        }
    }

