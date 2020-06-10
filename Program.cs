using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole.Themes;
using TestApi.Helpers;

namespace TestApi
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = GetLogger();

            try
            {
                Log.Information("Starting web request");
                CreateHostBuilder(args).Build().Run();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unexpected host termination");
            }
            finally
            {
                Log.CloseAndFlush();
            }

            return -1;
        }

        private static Serilog.ILogger GetLogger()
        {
            var logConfig = GetLoggerConfiguration();
            bool useJson = Environment.GetEnvironmentVariable("ASPNETCORE_JSON_LOGGING") == "1";

            if (useJson)
            {
                logConfig = logConfig.WriteTo.Console(new CompactJsonFormatter());
            }
            else
            {
                logConfig = logConfig.WriteTo.Console();
            }

            return logConfig.CreateLogger();
        }

        private static LoggerConfiguration GetLoggerConfiguration()
        {
            if (!Enum.TryParse(Environment.GetEnvironmentVariable("ASPNETCORE_LOGLEVEL"), true, out LogEventLevel logLevel))
            {
                logLevel = LogEventLevel.Information;
            }
            Console.WriteLine("Loglevel: " + logLevel.ToString());

            return new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.With<CorrelationIdEnricher>();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .UseSerilog();
                });


    }
}
