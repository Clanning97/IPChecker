using IPChecker.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

namespace IPChecker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Starting IPChecker");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                GracefulExit(ex);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHttpClient<IIPService, IPService>();

                    services.AddTransient<ISmtpService, SmtpService>();

                    services.AddHostedService<Worker>();
                })
                .UseSerilog();

        public static void GracefulExit(Exception ex)
        {
            Log.Fatal(ex, "Could not build or start host");
            Log.CloseAndFlush();
            Environment.Exit(1);
        }
    }
}