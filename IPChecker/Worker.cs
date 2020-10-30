using IPChecker.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace IPChecker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IIPService _ipService;
        private readonly ISmtpService _smtpService;
        private string _currentIP;

        public Worker(ILogger<Worker> logger, IIPService ipService, ISmtpService smtpService)
        {
            _logger = logger;
            _ipService = ipService;
            _smtpService = smtpService;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("IPChecker started at: {time}", DateTimeOffset.Now);

            if (File.Exists("ip.txt"))
            {
                try
                {
                    _currentIP = File.ReadAllText("ip.txt");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not read the saved IP address from file but the program will continue...");
                }
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var responseIP = await _ipService.GetIP();

                    if (_currentIP != responseIP)
                    {
                        _currentIP = responseIP;

                        var message = new MailMessage("", "", "The new IP for the house is: ", $"{_currentIP}");
                        _smtpService.SendEmail(message);

                        _logger.LogInformation($"IP has changed, email sent: {_currentIP}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not send or retrieve the ip address");

                    var message = new MailMessage("", "", "Error Occured: IPChecker", $"{ex}\n{_currentIP}");
                    _smtpService.SendEmail(message);

                    break;
                }

                await Task.Delay(1000 * 60 * 60);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            File.WriteAllText("ip.txt", _currentIP);

            _logger.LogInformation("IPChecker stoped at: {time}", DateTimeOffset.Now);

            await base.StopAsync(cancellationToken);
        }
    }
}