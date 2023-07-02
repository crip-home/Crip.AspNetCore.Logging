using System;
using Crip.AspNetCore.Logging.LongJsonContent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Crip.AspNetCore.Logging.Core31.Example;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Configure exception handling with filters:
        // https://docs.microsoft.com/en-us/aspnet/core/web-api/handle-errors?view=aspnetcore-3.1#use-exceptions-to-modify-the-response
        services.AddControllers(options =>
            options.Filters.Add(new HttpResponseExceptionFilter()));

        services
            // Adds all required services for RequestLoggingMiddleware
            .AddRequestLogging(options =>
            {
                // Allows to disable response content logging for all requests.
                options.LogResponse = true;

                // By default only "authorization" header is masked, but this configuration allows
                // to set any header name to mask secure values of authorization.
                options.AuthorizationHeaders.AuthorizationHeaderNames.Add("x-auth");
            })
            // Adds middleware to reduce log size by removing values from "application/json" request
            // where single property takes more than allowed character count.
            // Very useful when API receives/sends blobs in base64 format.
            .AddRequestLoggingLongJson(options =>
            {
                // Maximum allowed characters in a json string property.
                options.MaxCharCountInField = 500;
                // Character count to leave in a field after it is trimmed.
                options.LeaveOnTrimCharCountInField = 10;
            })
            // Hide cookie header values in log messages
            .AddRequestLoggingCookieValueMiddleware()
            // Do not log if request path matches provided pattern.
            .AddRequestLoggingExclude("/api/test/pattern-exclude*", "/api/test/exact-exclude");

        // Register HTTP client and write all request logs
        // As an alternative, could be used handler `.AddHttpMessageHandler<LoggingHandler<NamedHttpClient>>()`
        // but in such case make sure you register `.AddRequestLoggingHandler()` in DI.
        services.AddLoggableHttpClient<NamedHttpClient>(client =>
        {
            client.BaseAddress = new Uri("http://postman-echo.com/");
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        // Should be called after UseRouting and before UseEndpoints.
        //
        // It still work if added at the top of this method, but in that case
        // logger MinimumLevel overrides will not work as at moment of middleware
        // execution ControllerName will not be available and logger name will fallback
        // to its default value "Crip.AspNetCore.Logging.RequestLoggingMiddleware"
        app.UseRequestLoggingMiddleware();

        // Exception handlers should go after RequestLoggingMiddleware
        // otherwise response could not be prepared for log message.
        //
        // Prefer to use IActionFilter for error handling as shown in ConfigureServices method.
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.StatusCode = 500;
                    context.Response.ContentType = "text/html";

                    await context.Response.WriteAsync("<html lang=\"en\"><body>\r\nSystem ERROR!<br><br>\r\n</body></html>\r\n");
                    await context.Response.WriteAsync(new string(' ', 100));
                });
            });
        }

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapGet("/", async context =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        });
    }
}