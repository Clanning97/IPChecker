using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace IPChecker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private HttpClient http;
        private SmtpClient smtp;
        private string currentIP;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            http = new HttpClient();
            smtp = new SmtpClient()
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
                    currentIP = File.ReadAllText("ip.txt");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    currentIP = null;
                }
            }

            return base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.IsNullOrEmpty(currentIP))
            {
                try
                {
                    var response = await http.GetFromJsonAsync<IPResponse>("https://api.ipify.org?format=json");
                    currentIP = response.IP;

                    smtp.Send("clanning97@verizon.net", "clanning97@verizon.net", "IP Change", $"{currentIP}");

                    _logger.LogInformation($"Sent email: {currentIP}");
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
                    var response = await http.GetFromJsonAsync<IPResponse>("https://api.ipify.org?format=json");

                    if (currentIP != response.IP)
                    {
                        currentIP = response.IP;

                        smtp.Send("clanning97@verizon.net", "clanning97@verizon.net", "IP Change", $"{currentIP}");

                        _logger.LogInformation($"IP has changed, email sent: {currentIP}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.ToString());
                    smtp.Send("clanning97@verizon.net", "clanning97@verizon.net", "Error Occured: IPChecker", $"{ex}\n{currentIP}");
                    return;
                }

                await Task.Delay(1000 * 60 * 60);
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            http.Dispose();
            smtp.Dispose();

            File.WriteAllText("ip.txt", currentIP);

            return base.StopAsync(cancellationToken);
        }
    }
}