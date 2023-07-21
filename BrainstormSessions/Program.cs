using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Email;
using Serilog.Sinks.InMemory;
using System;
using System.Net;

namespace BrainstormSessions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
                /*.WriteTo.Email(new EmailConnectionInfo
                {
                    FromEmail = "sender@example.com",
                    ToEmail = "recipient@example.com",
                    MailServer = "smtp.example.com",
                    EnableSsl = true,
                    NetworkCredentials = new NetworkCredential("username", "password")
                }, outputTemplate: "[{Level}] {Message}{NewLine}{Exception}")*/
                .WriteTo.Log4Net()
                .WriteTo.InMemory()
                .CreateLogger();
            
            try
            {
                Log.Information("Starting application");
                CreateHostBuilder(args).Build().Run();
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
