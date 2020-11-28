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
using Moq;
using Moq.Language.Flow;
using Serilog;
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

        [Fact, Trait("Category", "Unit")]
        public async Task RequestLoggingMiddleware_Invoke_InformationLevel()
        {
            // Arrange
            var provider = CreateServiceProvider(config => config.MinimumLevel.Information());
            ILoggerFactory factory = provider.GetService<ILoggerFactory>();
            ILogger<RequestLoggingMiddleware> logger = factory.CreateLogger<RequestLoggingMiddleware>();

            logger.IsEnabled(LogLevel.Error).Should().BeTrue();
            logger.IsEnabled(LogLevel.Information).Should().BeTrue();
            logger.IsEnabled(LogLevel.Debug).Should().BeFalse();
            logger.IsEnabled(LogLevel.Trace).Should().BeFalse();

            var handler = new RequestLoggingMiddlewareUnderTest(
                async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    await ctx.Response.WriteAsync("Response body");
                },
                factory);

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

        [Fact, Trait("Category", "Unit")]
        public async Task RequestLoggingMiddleware_Invoke_DebugLevel()
        {
            // Arrange
            var provider = CreateServiceProvider(config => config.MinimumLevel.Debug());
            ILoggerFactory factory = provider.GetService<ILoggerFactory>();
            ILogger<RequestLoggingMiddleware> logger = factory.CreateLogger<RequestLoggingMiddleware>();

            logger.IsEnabled(LogLevel.Error).Should().BeTrue();
            logger.IsEnabled(LogLevel.Information).Should().BeTrue();
            logger.IsEnabled(LogLevel.Debug).Should().BeTrue();
            logger.IsEnabled(LogLevel.Trace).Should().BeFalse();

            RequestLoggingMiddlewareUnderTest handler = new(
                async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.Headers["Foo"] = "Bar";
                    await ctx.Response.WriteAsync("Response body");
                },
                factory);

            // Act
            using var _ = TestCorrelator.CreateContext();
            logger.LogInformation("Before");
            await handler.Invoke(Context);
            logger.LogInformation("After");

            // Assert
            List<string> actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
            actual.Should().BeEquivalentTo(
                "Information: Before { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware\" }",
                @"Debug: GET http://localhost/master/slave HTTP/1.1
Host: localhost
Authorization: Basic RE9NQUlOXHVzZXJuYW1lOnBhc3N3b3JkCg==
Foo: bar, baz
 { SourceContext: ""Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake"", EventName: ""HttpRequest"", Endpoint: ""http://localhost/master/slave"", HttpMethod: ""GET"" }",
                @"Debug: HTTP/1.1 200 OK
Foo: Bar
 { SourceContext: ""Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake"", EventName: ""HttpResponse"", StatusCode: 200, Elapsed: 100, Endpoint: ""http://localhost/master/slave"", HttpMethod: ""GET"" }",
                "Information: GET http://localhost/master/slave at 00:00:00:100 with 200 OK { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 200, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }",
                "Information: After { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware\" }");
        }

        [Fact, Trait("Category", "Unit")]
        public async Task RequestLoggingMiddleware_Invoke_TraceLevel()
        {
            // Arrange
            var provider = CreateServiceProvider(config => config.MinimumLevel.Verbose());
            ILoggerFactory factory = provider.GetService<ILoggerFactory>();
            ILogger<RequestLoggingMiddleware> logger = factory.CreateLogger<RequestLoggingMiddleware>();

            logger.IsEnabled(LogLevel.Error).Should().BeTrue();
            logger.IsEnabled(LogLevel.Information).Should().BeTrue();
            logger.IsEnabled(LogLevel.Debug).Should().BeTrue();
            logger.IsEnabled(LogLevel.Trace).Should().BeTrue();

            RequestLoggingMiddlewareUnderTest handler = new(
                async ctx =>
                {
                    ctx.Response.StatusCode = 200;
                    ctx.Response.Headers["Foo"] = "Bar";
                    await ctx.Response.WriteAsync("Response body");
                },
                factory);

            // Act
            using var _ = TestCorrelator.CreateContext();
            logger.LogInformation("Before");
            await handler.Invoke(Context);
            logger.LogInformation("After");

            // Assert
            List<string> actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
            actual.Should().BeEquivalentTo(
                "Information: Before { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware\" }",
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
                "Information: GET http://localhost/master/slave at 00:00:00:100 with 200 OK { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 200, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }",
                "Information: After { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware\" }");
        }

        [Fact, Trait("Category", "Unit")]
        public async Task RequestLoggingMiddleware_Invoke_ExceptionLogged()
        {
            // Arrange
            var provider = CreateServiceProvider(config => config.MinimumLevel.Information());
            ILoggerFactory factory = provider.GetService<ILoggerFactory>();
            ILogger<RequestLoggingMiddleware> logger = factory.CreateLogger<RequestLoggingMiddleware>();

            logger.IsEnabled(LogLevel.Error).Should().BeTrue();
            logger.IsEnabled(LogLevel.Information).Should().BeTrue();
            logger.IsEnabled(LogLevel.Debug).Should().BeFalse();
            logger.IsEnabled(LogLevel.Trace).Should().BeFalse();

            RequestLoggingMiddlewareUnderTest handler = new(
                ctx =>
                {
                    ctx.Response.StatusCode = 500;
                    throw new Exception("Exception message");
                },
                factory);

            // Act
            using var _ = TestCorrelator.CreateContext();
            logger.LogInformation("Before");
            Exception ex1 = await Assert.ThrowsAsync<Exception>(async () => { await handler.Invoke(Context); });
            logger.LogInformation("After");

            // Assert
            ex1.Message.Should().Be("Exception message");
            List<string> actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
            actual.Should().BeEquivalentTo(
                "Information: Before { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware\" }",
                "Error: Error during HTTP request processing { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 500, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }",
                "Information: GET http://localhost/master/slave at 00:00:00:100 with 500 InternalServerError { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 500, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }",
                "Information: After { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware\" }");
        }

        [Fact, Trait("Category", "Unit")]
        public async Task RequestLoggingMiddleware_Invoke_FatalNothingLogged()
        {
            // Arrange
            var provider = CreateServiceProvider(config => config.MinimumLevel.Fatal());
            ILoggerFactory factory = provider.GetService<ILoggerFactory>();
            RequestLoggingMiddlewareUnderTest handler = new(
                async ctx => await ctx.Response.WriteAsync("Response body"),
                factory);

            // Act
            using var _ = TestCorrelator.CreateContext();
            await handler.Invoke(Context);

            // Assert
            List<string> actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
            actual.Should().BeEmpty();
        }

        private static ServiceProvider CreateServiceProvider(
            Func<LoggerConfiguration, LoggerConfiguration> configureLogger)
        {
            var logger = configureLogger(new())
                .WriteTo.TestCorrelator()
                .CreateLogger();

            SerilogLoggerFactory loggerFactory = new(logger);

            return new ServiceCollection()
                .AddSingleton<ILoggerFactory>(_ => loggerFactory)
                .BuildServiceProvider();
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


        private class RequestLoggingMiddlewareUnderTest : RequestLoggingMiddleware
        {
            public RequestLoggingMiddlewareUnderTest(RequestDelegate next, ILoggerFactory factory)
                : base(next, new ContextLoggerFactory(MockedServiceProvider(factory)))
            {
            }

            protected override IStopwatch CreateStopwatch()
            {
                return new MockStopwatch(TimeSpan.FromMilliseconds(100));
            }

            private static IServiceProvider MockedServiceProvider(ILoggerFactory factory)
            {
                var serviceProviderMock = new Mock<IServiceProvider>();

                Setup<IRequestLogger>(serviceProviderMock).Returns(new RequestLogger(new(null), new(null)));
                Setup<IResponseLogger>(serviceProviderMock).Returns(new ResponseLogger(new(null), new(null)));
                Setup<IBasicInfoLogger>(serviceProviderMock).Returns(new BasicInfoLogger());
                Setup<IEnumerable<IHttpRequestPredicate>>(serviceProviderMock).Returns(Enumerable.Empty<IHttpRequestPredicate>());
                Setup<ILoggerFactory>(serviceProviderMock).Returns(factory);
                Setup<IHttpLoggerFactory>(serviceProviderMock).Returns(new HttpLoggerFactory(serviceProviderMock.Object));

                return serviceProviderMock.Object;
            }

            private static ISetup<IServiceProvider, object> Setup<T>(Mock<IServiceProvider> mock)
            {
                return mock.Setup(serviceProvider => serviceProvider.GetService(typeof(T)));
            }
        }
    }
}
