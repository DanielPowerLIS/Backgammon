using System.Configuration;

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Backgammon.Server.SupportServices
{
    internal class EmailService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _address;
        private readonly string _password;
        private readonly string _senderName;

        public EmailService()
        {
            _host = GetSetting("EmailHost");
            _port = GetPort();
            _address = GetSetting("EmailAddress");
            _password = GetSetting("EmailPassword");
            _senderName = GetSetting("EmailSenderName");
        }

        public void Send(string recipient, string subject, string body)
        {
            MimeMessage message = CreateMessage(recipient, subject, body);

            using (var client = new SmtpClient())
            {
                client.Connect(_host, _port, SecureSocketOptions.StartTls);
                client.Authenticate(_address, _password);
                client.Send(message);
                client.Disconnect(true);
            }
        }

        private MimeMessage CreateMessage(
            string recipient, string subject, string body)
        {
            var message = new MimeMessage();
            var sender = new MailboxAddress(_senderName, _address);
            MailboxAddress receiver = MailboxAddress.Parse(recipient);

            message.From.Add(sender);
            message.To.Add(receiver);
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Plain)
            {
                Text = body
            };

            return message;
        }

        private string GetSetting(string key)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationErrorsException(
                    "A required email setting is missing: " + key);
            }

            return value;
        }

        private int GetPort()
        {
            string portValue = GetSetting("EmailPort");
            bool validPort = int.TryParse(portValue, out int port);

            if (!validPort || port <= 0 || port > ushort.MaxValue)
            {
                throw new ConfigurationErrorsException(
                    "The email port configuration is invalid.");
            }

            return port;
        }
    }
}