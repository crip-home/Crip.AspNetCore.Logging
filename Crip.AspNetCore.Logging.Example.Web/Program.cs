using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Crip.AspNetCore.Logging.Example.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
                .MinimumLevel.Override("Crip.AspNetCore.Logging.LoggingHandler.NamedHttpClient", LogEventLevel.Verbose)
                /* Set all request logs to Information level. To DISABLE - set Warning level */
                .MinimumLevel.Override("Crip.AspNetCore.Logging.RequestLoggingMiddleware", LogEventLevel.Information)
                /* Set 'Test' controller logs to Verbose level (will write request metrics, request/response headers and entire body to log message) */
                .MinimumLevel.Override("Crip.AspNetCore.Logging.RequestLoggingMiddleware.Test", LogEventLevel.Verbose)
                /* Set 'TestVerbose' controller logs to Verbose level (will write request metrics, request/response headers and entire body to log message) */
                .MinimumLevel.Override("Crip.AspNetCore.Logging.RequestLoggingMiddleware.TestVerbose", LogEventLevel.Verbose)
                /* Set 'TestDebug' controller logs to Debug level (will write request metrics and request/response headers to log message) */
                .MinimumLevel.Override("Crip.AspNetCore.Logging.RequestLoggingMiddleware.TestDebug", LogEventLevel.Debug)
                /* Set 'TestInfo' controller logs to Information level (will write only request metrics to log message) */
                .MinimumLevel.Override("Crip.AspNetCore.Logging.RequestLoggingMiddleware.TestInfo", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Properties:j}{NewLine}{Exception}")
                .CreateLogger();

            CreateHostBuilder(args).Build().Run();
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