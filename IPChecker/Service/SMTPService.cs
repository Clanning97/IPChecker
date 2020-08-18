using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;

namespace IPChecker.Service
{
    public interface ISmtpService
    {
        void SendEmail(MailMessage message);
    }

    public class SmtpService : ISmtpService, IDisposable
    {
        private readonly SmtpClient _smtpClient;
        private readonly IConfiguration _configuration;

        public SmtpService(IConfiguration configuration)
        {
            _configuration = configuration;

            _smtpClient = new SmtpClient()
            {
                Host = _configuration.GetValue<string>("SmtpClient:Host"),
                Port = _configuration.GetValue<int>("SmtpClient:Port"),
                EnableSsl = _configuration.GetValue<bool>("SmtpClient:Ssl"),
                Credentials = new NetworkCredential(
                    _configuration.GetValue<string>("SmtpClient:Username"),
                    _configuration.GetValue<string>("SmtpClient:Password")
                )
            };
        }

        public void SendEmail(MailMessage message)
        {
            _smtpClient.Send(message);
        }

        public void Dispose()
        {
            _smtpClient.Dispose();
        }
    }
}