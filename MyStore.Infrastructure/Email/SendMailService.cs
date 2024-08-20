using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MyStore.Application.ISendMail;

namespace MyStore.Infrastructure.Email
{
    public class SenderSettings
    {
        public string Mail { get; set; }
        public string? DisplayName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public class SendMailService : ISendMailService
    {
        private readonly SenderSettings _sender;
        public SendMailService(IOptions<SenderSettings> configuration) => _sender = configuration.Value;

        public async Task SendMailToOne(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.Sender = new MailboxAddress(_sender.DisplayName, _sender.Mail);
            message.From.Add(new MailboxAddress(_sender.DisplayName, _sender.Mail));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = htmlMessage;

            message.Body = builder.ToMessageBody();

            using(var stmpClient = new SmtpClient())
            {
                try
                {
                    await stmpClient.ConnectAsync(_sender.Host, _sender.Port, SecureSocketOptions.StartTls);
                    await stmpClient.AuthenticateAsync(_sender.Mail, _sender.Password);
                    await stmpClient.SendAsync(message);
                }
                catch (Exception ex)
                {
                    var path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, "MyStore.Infrastructure", "Email", "ErrorMail");
                    Directory.CreateDirectory(path);

                    var savefile = Path.Combine(path, email + '_' + Guid.NewGuid());
                    await message.WriteToAsync(savefile);
                    await File.AppendAllTextAsync(savefile, '\n' + ex.Message);
                }
                await stmpClient.DisconnectAsync(true);
            }
        }

        public async Task SendMailToMany(List<string> emails, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.Sender = new MailboxAddress(_sender.DisplayName, _sender.Mail);
            message.From.Add(MailboxAddress.Parse(_sender.Mail));
            List<MailboxAddress> addresses = new List<MailboxAddress>();
            foreach (var email in emails)
            {
                addresses.Add(MailboxAddress.Parse(email));
            }
            message.To.AddRange(addresses);
            message.Subject = subject;

            var builder = new BodyBuilder();
            builder.HtmlBody = htmlMessage;

            message.Body = builder.ToMessageBody();

            using (var stmpClient = new SmtpClient())
            {
                try
                {
                    await stmpClient.ConnectAsync(_sender.Host, _sender.Port, SecureSocketOptions.StartTls);
                    await stmpClient.AuthenticateAsync(_sender.Mail, _sender.Password);
                    await stmpClient.SendAsync(message);
                }
                catch (Exception ex)
                {
                    var path = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, "MyStore.Infrastructure", "Email", "ErrorMail");
                    Directory.CreateDirectory(path);

                    var savefile = Path.Combine(path, "List_" + emails.Count.ToString() + '_' + Guid.NewGuid());
                    await message.WriteToAsync(savefile);
                    await File.AppendAllTextAsync(savefile, '\n' + ex.Message);
                }
                await stmpClient.DisconnectAsync(true);
            }
        }
    }
}
