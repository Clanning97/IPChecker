using IPChecker.Service;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace IPChecker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IIPService _ipService;
        private SmtpClient _smtp;
        private string _currentIP;

        public Worker(ILogger<Worker> logger, IIPService ipService)
        {
            _logger = logger;
            _ipService = ipService;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _smtp = new SmtpClient()
            {
                Host = "smtp.verizon.net",
                Port = 587,
                EnableSsl = true,
                Credentials = new NetworkCredential("clanning97@verizon.net", "mcybviqbcenppcsh")
            };

            if (File.Exists("ip.txt"))
            {
                try
                {
                    _currentIP = File.ReadAllText("ip.txt");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not read the saved IP address from file but the program will continue...");
                    _currentIP = null;
                }
            }

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrEmpty(_currentIP))
            {
                try
                {
                    _currentIP = await _ipService.GetIP();

                    _smtp.Send("clanning97@verizon.net", "clanning97@verizon.net", "IP Change", $"{_currentIP}");

                    _logger.LogInformation($"Sent email: {_currentIP}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    return;
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var responseIP = await _ipService.GetIP();

                    if (_currentIP != responseIP)
                    {
                        _currentIP = responseIP;

                        _smtp.Send("clanning97@verizon.net", "clanning97@verizon.net", "IP Change", $"{_currentIP}");

                        _logger.LogInformation($"IP has changed, email sent: {_currentIP}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    _smtp.Send("clanning97@verizon.net", "clanning97@verizon.net", "Error Occured: IPChecker", $"{ex}\n{_currentIP}");
                    return;
                }

                await Task.Delay(1000 * 60 * 60);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _smtp.Dispose();

            File.WriteAllText("ip.txt", _currentIP);

            await base.StopAsync(cancellationToken);
        }
    }
}