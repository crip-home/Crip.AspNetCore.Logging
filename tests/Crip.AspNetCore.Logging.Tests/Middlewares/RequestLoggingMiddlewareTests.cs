using System.Globalization;
using Crip.AspNetCore.Logging.Tests.Helpers;
using Crip.AspNetCore.Logging.Tests.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Display;
using Serilog.Sinks.TestCorrelator;

namespace Crip.AspNetCore.Logging.Tests.Middlewares;

public class RequestLoggingMiddlewareTests
{
    private HttpContext Context =>
        new FakeHttpContextBuilder("HTTP/1.1")
            .SetMethod(HttpMethod.Get)
            .SetScheme("http")
            .SetHost("localhost")
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
    public async Task Invoke_InformationLevel()
    {
        // Arrange
        var provider = ServiceProvider(LogEventLevel.Information);
        var contextLoggerFactory = provider.GetRequiredService<IContextLoggerFactory>();
        var measurable = provider.GetRequiredService<IMeasurable>();
        var factory = provider.GetRequiredService<ILoggerFactory>();
        var logger = factory.CreateLogger<RequestLoggingMiddleware>();

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
        var actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
        actual.Should().BeEquivalentTo(
            "Information: Before { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware\" }",
            "Information: GET http://localhost/master/slave at 00:00:00.100 with 200 OK { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 200, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }",
            "Information: After { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware\" }");
    }

    [Fact, Trait("Category", "Integration")]
    public async Task Invoke_DebugLevel()
    {
        // Arrange
        var provider = ServiceProvider(LogEventLevel.Debug);
        var contextLoggerFactory = provider.GetRequiredService<IContextLoggerFactory>();
        var measurable = provider.GetRequiredService<IMeasurable>();
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
        var actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
        actual.Should().BeEquivalentTo(
            """
            Debug: GET http://localhost/master/slave HTTP/1.1
            Host: localhost
            Authorization: Basic RE9NQUlOXHVzZXJuYW1lOnBhc3N3b3JkCg==
            Foo: bar, baz
             { SourceContext: "Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake", EventName: "HttpRequest", Endpoint: "http://localhost/master/slave", HttpMethod: "GET" }
            """,
            """
            Debug: HTTP/1.1 200 OK
            Foo: Bar
             { SourceContext: "Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake", EventName: "HttpResponse", StatusCode: 200, Elapsed: 100, Endpoint: "http://localhost/master/slave", HttpMethod: "GET" }
            """,
            "Information: GET http://localhost/master/slave at 00:00:00.100 with 200 OK { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 200, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }"
        );
    }

    [Fact, Trait("Category", "Integration")]
    public async Task Invoke_TraceLevel()
    {
        // Arrange
        var provider = ServiceProvider(LogEventLevel.Verbose);
        var contextLoggerFactory = provider.GetRequiredService<IContextLoggerFactory>();
        var measurable = provider.GetRequiredService<IMeasurable>();
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
        var actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
        actual.Should().BeEquivalentTo(
            """
            Verbose: GET http://localhost/master/slave HTTP/1.1
            Host: localhost
            Authorization: Basic RE9NQUlOXHVzZXJuYW1lOnBhc3N3b3JkCg==
            Foo: bar, baz

            Request body
             { SourceContext: "Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake", EventName: "HttpRequest", Endpoint: "http://localhost/master/slave", HttpMethod: "GET" }
            """,
            """
            Verbose: HTTP/1.1 200 OK
            Foo: Bar

            Response body
             { SourceContext: "Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake", EventName: "HttpResponse", StatusCode: 200, Elapsed: 100, Endpoint: "http://localhost/master/slave", HttpMethod: "GET" }
            """,
            "Information: GET http://localhost/master/slave at 00:00:00.100 with 200 OK { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 200, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }"
        );
    }

    [Fact, Trait("Category", "Integration")]
    public async Task Invoke_ExceptionLogged()
    {
        // Arrange
        var provider = ServiceProvider(LogEventLevel.Information);
        var contextLoggerFactory = provider.GetRequiredService<IContextLoggerFactory>();
        var measurable = provider.GetRequiredService<IMeasurable>();
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
        var ex1 = await Assert.ThrowsAsync<Exception>(async () => { await handler.Invoke(Context); });

        // Assert
        ex1.Message.Should().Be("Exception message");
        var actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
        actual.Should().BeEquivalentTo(
            "Error: Error during HTTP request processing { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 500, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }",
            "Information: GET http://localhost/master/slave at 00:00:00.100 with 500 InternalServerError { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 500, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }"
        );
    }

    [Fact, Trait("Category", "Integration")]
    public async Task Invoke_FatalNothingLogged()
    {
        // Arrange
        var provider = ServiceProvider(LogEventLevel.Fatal);
        var contextLoggerFactory = provider.GetRequiredService<IContextLoggerFactory>();
        var measurable = provider.GetRequiredService<IMeasurable>();
        RequestLoggingMiddleware handler = new(
            async ctx => await ctx.Response.WriteAsync("Response body"),
            contextLoggerFactory,
            measurable);

        // Act
        using var _ = TestCorrelator.CreateContext();
        await handler.Invoke(Context);

        // Assert
        var actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
        actual.Should().BeEmpty();
    }

    [Fact, Trait("Category", "Integration")]
    public async Task Invoke_TraceLevelResponseNotLoggedIfConfiguredSo()
    {
        var provider = ServiceProvider(LogEventLevel.Verbose, logResponses: false);
        var contextLoggerFactory = provider.GetRequiredService<IContextLoggerFactory>();
        var measurable = provider.GetRequiredService<IMeasurable>();
        RequestLoggingMiddleware handler = new(
            async ctx =>
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.Headers["Foo"] = "Bar";
                await ctx.Response.WriteAsync("Response body");
            },
            contextLoggerFactory,
            measurable);

        using var _ = TestCorrelator.CreateContext();
        await handler.Invoke(Context);

        var actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
        actual.Should().BeEquivalentTo(
            """
            Verbose: GET http://localhost/master/slave HTTP/1.1
            Host: localhost
            Authorization: Basic RE9NQUlOXHVzZXJuYW1lOnBhc3N3b3JkCg==
            Foo: bar, baz

            Request body
             { SourceContext: "Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake", EventName: "HttpRequest", Endpoint: "http://localhost/master/slave", HttpMethod: "GET" }
            """,
            "Information: GET http://localhost/master/slave at 00:00:00.100 with 200 OK { SourceContext: \"Crip.AspNetCore.Logging.RequestLoggingMiddleware.Fake\", EventName: \"HttpResponse\", StatusCode: 200, Elapsed: 100, Endpoint: \"http://localhost/master/slave\", HttpMethod: \"GET\" }"
        );
    }

    [Fact, Trait("Category", "Integration")]
    public async Task Invoke_TraceLevelResponseNotLoggedAndContentIsProvided()
    {
        var context = Context;
        var provider = ServiceProvider(LogEventLevel.Verbose, logResponses: false);
        var contextLoggerFactory = provider.GetRequiredService<IContextLoggerFactory>();
        var measurable = provider.GetRequiredService<IMeasurable>();
        RequestLoggingMiddleware handler = new(
            async ctx =>
            {
                ctx.Response.StatusCode = 201;
                ctx.Response.Headers["Foo"] = "Bar";
                await ctx.Response.WriteAsync("Response body");
            },
            contextLoggerFactory,
            measurable);

        await handler.Invoke(context);

        context.Response.StatusCode.Should().Be(201);
        await VerifyResponseBody(context, "Response body");
    }

    [Fact, Trait("Category", "Integration")]
    public async Task Invoke_TraceLevelResponseContentIsProvided()
    {
        var context = Context;
        var provider = ServiceProvider(LogEventLevel.Verbose);
        var contextLoggerFactory = provider.GetRequiredService<IContextLoggerFactory>();
        var measurable = provider.GetRequiredService<IMeasurable>();
        RequestLoggingMiddleware handler = new(
            async ctx =>
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.Headers["Foo"] = "Bar";
                await ctx.Response.WriteAsync("Response body");
            },
            contextLoggerFactory,
            measurable);

        await handler.Invoke(context);

        context.Response.StatusCode.Should().Be(200);
        await VerifyResponseBody(context, "Response body");
    }

    private static async Task VerifyResponseBody(HttpContext context, string content)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();
        body.Should().Be(content);
    }

    private static IServiceProvider ServiceProvider(LogEventLevel logLevel, bool logResponses = true) =>
        new ServiceCollection()
            .AddSingleton<ILoggerFactory>(_ => CreateSerilogLoggerFactory(logLevel))
            .AddSingleton<IRequestLogger>(_ => new RequestLogger(new(null), new(null)))
            .AddSingleton<IResponseLogger>(_ => new ResponseLogger(new(null), new(null)))
            .AddSingleton<IBasicInfoLogger, BasicInfoLogger>()
            .AddSingleton<IHttpLoggerFactory, HttpLoggerFactory>()
            .AddSingleton<IContextLoggerFactory, ContextLoggerFactory>()
            .AddSingleton<IMeasurable, TimeMeasurable>()
            .AddSingleton<IStopwatch>(_ => new MockStopwatch(TimeSpan.FromSeconds(0.1)))
            .Configure((RequestLoggingOptions options) => options.LogResponse = logResponses)
            .BuildServiceProvider();

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