using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Display;
using Serilog.Sinks.TestCorrelator;
using Xunit;

namespace Crip.AspNetCore.Logging.Tests
{
    public class RequestLoggingMiddlewareTests
    {
        private HttpContext Context =>
            new FakeHttpContextBuilder("HTTP/1.1")
                .SetMethod(HttpMethod.Get)
                .SetScheme(HttpScheme.Http)
                .SetHost(new("localhost"))
                .SetPathBase("/master")
                .SetPath("/slave")
                .SetRequestHeaders(new()
                {
                    { "Authorization", "Basic RE9NQUlOXHVzZXJuYW1lOnBhc3N3b3JkCg==" },
                    { "Foo", new StringValues("bar, baz") }
                })
                .SetRequestBody("Request body")
                .SetEndpoint("Fake")
                .Create();

        [Fact, Trait("Category", "Integration")]
        public async Task RequestLoggingMiddleware_Invoke_InformationLevel()
        {
            // Arrange
            IServiceProvider provider = ServiceProvider(LogEventLevel.Information);
            IContextLoggerFactory contextLoggerFactory = provider.GetService<IContextLoggerFactory>();
            IMeasurable measurable = provider.GetService<IMeasurable>();
            ILoggerFactory factory = provider.GetService<ILoggerFactory>();
            ILogger<RequestLoggingMiddleware> logger = factory.CreateLogger<RequestLoggingMiddleware>();

            logger.IsEnabled(LogLevel.Error).Should().BeTrue();
            logger.IsEnabled(LogLevel.Information).Should().BeTrue();
            logger.IsEnabled(LogLevel.Debug).Should().BeFalse();
            logger.IsEnabled(LogLevel.Trace).Should().BeFalse();

            RequestLoggingMiddleware handler = new(
                async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    await ctx.Response.WriteAsync("Response body");
                },
                contextLoggerFactory,
                measurable);

            // Act
            using var _ = TestCorrelator.CreateContext();
            logger.LogInformation("Before");
            await handler.Invoke(Context);
            logger.LogInformation("After");

            // Assert
            List<string> actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
            actual.Should().BeEquivalentTo(
                "Information: Before { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware\" }",
                "Information: GET http://localhost/master/slave at 00:00:00:100 with 200 OK { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 200, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }",
                "Information: After { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware\" }");
        }

        [Fact, Trait("Category", "Integration")]
        public async Task RequestLoggingMiddleware_Invoke_DebugLevel()
        {
            // Arrange
            IServiceProvider provider = ServiceProvider(LogEventLevel.Debug);
            IContextLoggerFactory contextLoggerFactory = provider.GetService<IContextLoggerFactory>();
            IMeasurable measurable = provider.GetService<IMeasurable>();
            RequestLoggingMiddleware handler = new(
                async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.Headers["Foo"] = "Bar";
                    await ctx.Response.WriteAsync("Response body");
                },
                contextLoggerFactory,
                measurable);

            // Act
            using var _ = TestCorrelator.CreateContext();
            await handler.Invoke(Context);

            // Assert
            List<string> actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
            actual.Should().BeEquivalentTo(
                @"Debug: GET http://localhost/master/slave HTTP/1.1
Host: localhost
Authorization: Basic RE9NQUlOXHVzZXJuYW1lOnBhc3N3b3JkCg==
Foo: bar, baz
 { SourceContext: ""Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake"", EventName: ""HttpRequest"", Endpoint: ""http://localhost/master/slave"", HttpMethod: ""GET"" }",
                @"Debug: HTTP/1.1 200 OK
Foo: Bar
 { SourceContext: ""Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake"", EventName: ""HttpResponse"", StatusCode: 200, Elapsed: 100, Endpoint: ""http://localhost/master/slave"", HttpMethod: ""GET"" }",
                "Information: GET http://localhost/master/slave at 00:00:00:100 with 200 OK { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 200, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }"
            );
        }

        [Fact, Trait("Category", "Integration")]
        public async Task RequestLoggingMiddleware_Invoke_TraceLevel()
        {
            // Arrange
            IServiceProvider provider = ServiceProvider(LogEventLevel.Verbose);
            IContextLoggerFactory contextLoggerFactory = provider.GetService<IContextLoggerFactory>();
            IMeasurable measurable = provider.GetService<IMeasurable>();
            RequestLoggingMiddleware handler = new(
                async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.Headers["Foo"] = "Bar";
                    await ctx.Response.WriteAsync("Response body");
                },
                contextLoggerFactory,
                measurable);

            // Act
            using var _ = TestCorrelator.CreateContext();
            await handler.Invoke(Context);

            // Assert
            List<string> actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
            actual.Should().BeEquivalentTo(
                @"Verbose: GET http://localhost/master/slave HTTP/1.1
Host: localhost
Authorization: Basic RE9NQUlOXHVzZXJuYW1lOnBhc3N3b3JkCg==
Foo: bar, baz

Request body
 { SourceContext: ""Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake"", EventName: ""HttpRequest"", Endpoint: ""http://localhost/master/slave"", HttpMethod: ""GET"" }",
                @"Verbose: HTTP/1.1 200 OK
Foo: Bar

Response body
 { SourceContext: ""Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake"", EventName: ""HttpResponse"", StatusCode: 200, Elapsed: 100, Endpoint: ""http://localhost/master/slave"", HttpMethod: ""GET"" }",
                "Information: GET http://localhost/master/slave at 00:00:00:100 with 200 OK { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 200, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }"
            );
        }

        [Fact, Trait("Category", "Integration")]
        public async Task RequestLoggingMiddleware_Invoke_ExceptionLogged()
        {
            // Arrange
            IServiceProvider provider = ServiceProvider(LogEventLevel.Information);
            IContextLoggerFactory contextLoggerFactory = provider.GetService<IContextLoggerFactory>();
            IMeasurable measurable = provider.GetService<IMeasurable>();
            RequestLoggingMiddleware handler = new(
                ctx =>
                {
                    ctx.Response.StatusCode = 500;
                    throw new Exception("Exception message");
                },
                contextLoggerFactory,
                measurable);

            // Act
            using var _ = TestCorrelator.CreateContext();
            Exception ex1 = await Assert.ThrowsAsync<Exception>(async () => { await handler.Invoke(Context); });

            // Assert
            ex1.Message.Should().Be("Exception message");
            List<string> actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
            actual.Should().BeEquivalentTo(
                "Error: Error during HTTP request processing { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 500, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }",
                "Information: GET http://localhost/master/slave at 00:00:00:100 with 500 InternalServerError { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 500, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }"
            );
        }

        [Fact, Trait("Category", "Integration")]
        public async Task RequestLoggingMiddleware_Invoke_FatalNothingLogged()
        {
            // Arrange
            IServiceProvider provider = ServiceProvider(LogEventLevel.Fatal);
            IContextLoggerFactory contextLoggerFactory = provider.GetService<IContextLoggerFactory>();
            IMeasurable measurable = provider.GetService<IMeasurable>();
            RequestLoggingMiddleware handler = new(
                async ctx => await ctx.Response.WriteAsync("Response body"),
                contextLoggerFactory,
                measurable);

            // Act
            using var _ = TestCorrelator.CreateContext();
            await handler.Invoke(Context);

            // Assert
            List<string> actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
            actual.Should().BeEmpty();
        }

        private static IContextLoggerFactory SetupFactory(LogEventLevel logLevel)
        {
            return ServiceProvider(logLevel).GetService<IContextLoggerFactory>();
        }

        private static IServiceProvider ServiceProvider(LogEventLevel logLevel)
        {
            return new ServiceCollection()
                .AddSingleton<ILoggerFactory>(i => CreateSerilogLoggerFactory(logLevel))
                .AddSingleton<IRequestLogger>(i => new RequestLogger(new(null), new(null)))
                .AddSingleton<IResponseLogger>(i => new ResponseLogger(new(null), new(null)))
                .AddSingleton<IBasicInfoLogger, BasicInfoLogger>()
                .AddSingleton<IHttpLoggerFactory, HttpLoggerFactory>()
                .AddSingleton<IContextLoggerFactory, ContextLoggerFactory>()
                .AddSingleton<IMeasurable, TimeMeasurable>()
                .AddSingleton<IStopwatch>(i => new MockStopwatch(TimeSpan.FromSeconds(0.1)))
                .BuildServiceProvider();
        }

        private static SerilogLoggerFactory CreateSerilogLoggerFactory(LogEventLevel logLevel)
        {
            var configuration = SetLogLevel(logLevel);
            var logger = CreateCorrelatorLogger(configuration);

            return new SerilogLoggerFactory(logger);
        }

        private static Func<LoggerConfiguration, LoggerConfiguration> SetLogLevel(LogEventLevel logLevel)
        {
            return configuration => configuration.MinimumLevel.Is(logLevel);
        }

        private static Logger CreateCorrelatorLogger(Func<LoggerConfiguration, LoggerConfiguration> configuration)
        {
            return configuration(new())
                .WriteTo.TestCorrelator()
                .CreateLogger();
        }

        private string FormatLogEvent(LogEvent evt)
        {
            const string template = "{Level}: {Message:lj} {Properties}";

            var culture = CultureInfo.InvariantCulture;
            MessageTemplateTextFormatter formatter = new(template, culture);
            StringWriter sw = new();
            formatter.Format(evt, sw);

            return sw.ToString();
        }
    }
}