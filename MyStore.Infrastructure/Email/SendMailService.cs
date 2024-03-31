using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Infrastructure.Email
{
    public class SenderSettings
    {
        public required string Email { get; set; }
        public string? DisplayName { get; set; }
        public required string Password { get; set; }
        public required string Host { get; set; }
        public required int Port { get; set; }
    }

    public class SendMailService : ISendMailService
    {
        private readonly SenderSettings _sender;
        public SendMailService(IOptions<SenderSettings> configuration) => _sender = configuration.Value;

        public async Task SendMailToOne(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.Sender = new MailboxAddress(_sender.DisplayName, _sender.Email);
            message.From.Add(new MailboxAddress(_sender.DisplayName, _sender.Email));
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
                    await stmpClient.AuthenticateAsync(_sender.Email, _sender.Password);
                    await stmpClient.SendAsync(message);
                }
                catch (Exception ex)
                {
                    Directory.CreateDirectory("./ErrorMail");
                    var savefile = string.Format($"./ErrorMail/{0}", email + '_' + Guid.NewGuid());
                    await message.WriteToAsync(savefile);
                    await File.AppendAllTextAsync(savefile, '\n' + ex.Message);
                }
                await stmpClient.DisconnectAsync(true);
            }
        }

        public async Task SendMailToMany(List<string> emails, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.Sender = new MailboxAddress(_sender.DisplayName, _sender.Email);
            message.From.Add(MailboxAddress.Parse(_sender.Email));
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
                    await stmpClient.AuthenticateAsync(_sender.Email, _sender.Password);
                    await stmpClient.SendAsync(message);
                }
                catch (Exception ex)
                {
                    Directory.CreateDirectory("./ErrorMail");
                    var savefile = string.Format("./ErrorMail/{0}", "List_" + emails.Count.ToString() + '_' + Guid.NewGuid());
                    await message.WriteToAsync(savefile);
                    await File.AppendAllTextAsync(savefile, '\n' + ex.Message);
                }
                await stmpClient.DisconnectAsync(true);
            }
        }
    }
}
