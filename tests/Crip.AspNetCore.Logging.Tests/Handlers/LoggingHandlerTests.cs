﻿using System.Globalization;
using System.Net;
using System.Text;
using Crip.Extensions.Tests;
using Crip.AspNetCore.Logging.Tests.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Formatting.Display;
using Serilog.Sinks.TestCorrelator;

namespace Crip.AspNetCore.Logging.Tests.Handlers;

public class LoggingHandlerTests
{
    [Fact, Trait("Category", "Integration")]
    public async Task SendAsync_WritesInformationLogMessage()
    {
        HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");
        HttpResponseMessage response = new(HttpStatusCode.OK);
        var sut = CreateInvoker(response, LogEventLevel.Information);

        // Act
        using var ctx = TestCorrelator.CreateContext();
        var result = await sut.SendAsync(request, default);

        // Assert
        result
            .Should().NotBeNull()
            .And.Be(response);

        // Assert
        var actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
        actual.Should().BeEquivalentTo("Information: GET http://example.com/ at 00:00:00.100 with 200 OK { SourceContext: \"Crip.AspNetCore.Logging.LoggingHandler.Foo\", EventName: \"HttpResponse\", StatusCode: 200, Elapsed: 100, Endpoint: \"http://example.com/\", HttpMethod: \"GET\" }");
    }

    [Fact, Trait("Category", "Integration")]
    public async Task SendAsync_WritesDebugLogMessages()
    {
        HttpRequestMessage request = new(HttpMethod.Get, "http://example.com");
        request.Headers.Add("Authorization", "Bearer token-value");

        HttpResponseMessage response = new(HttpStatusCode.OK);
        response.Content = new StringContent("{}", Encoding.UTF8, "application/json");
        response.Headers.Location = new Uri("http://example.com");

        var sut = CreateInvoker(response, LogEventLevel.Debug);

        // Act
        using var ctx = TestCorrelator.CreateContext();
        var result = await sut.SendAsync(request, default);

        // Assert
        result
            .Should().NotBeNull()
            .And.Be(response);

        // Assert
        var actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
        actual.Should().BeEquivalentTo(
            """
            Debug: GET http://example.com/ HTTP/1.1
            Authorization: Bearer token-value
             { SourceContext: "Crip.AspNetCore.Logging.LoggingHandler.Foo", EventName: "HttpRequest", Endpoint: "http://example.com/", HttpMethod: "GET" }
            """,
            """
            Debug: HTTP/1.1 200 OK
            Location: http://example.com/
             { SourceContext: "Crip.AspNetCore.Logging.LoggingHandler.Foo", EventName: "HttpResponse", StatusCode: 200, Elapsed: 100, Endpoint: "http://example.com/", HttpMethod: "GET" }
            """,
            "Information: GET http://example.com/ at 00:00:00.100 with 200 OK { SourceContext: \"Crip.AspNetCore.Logging.LoggingHandler.Foo\", EventName: \"HttpResponse\", StatusCode: 200, Elapsed: 100, Endpoint: \"http://example.com/\", HttpMethod: \"GET\" }"
        );
    }

    [Fact, Trait("Category", "Integration")]
    public async Task SendAsync_WritesVerboseLogMessages()
    {
        HttpRequestMessage request = new(HttpMethod.Post, "http://example.com");
        request.Content = new StringContent("{\"request\":1}", Encoding.UTF8, "application/json");
        request.Headers.Add("Authorization", "Bearer token-value");

        HttpResponseMessage response = new(HttpStatusCode.OK);
        response.Content = new StringContent("{\"response\":2}", Encoding.UTF8, "application/json");
        response.Headers.Location = new Uri("http://example.com");

        var sut = CreateInvoker(response, LogEventLevel.Verbose);

        // Act
        using var ctx = TestCorrelator.CreateContext();
        var result = await sut.SendAsync(request, default);

        // Assert
        result
            .Should().NotBeNull()
            .And.Be(response);

        // Assert
        var actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
        actual.Should().BeEquivalentTo(
            """
            Verbose: POST http://example.com/ HTTP/1.1
            Authorization: Bearer token-value

            {"request":1}
             { SourceContext: "Crip.AspNetCore.Logging.LoggingHandler.Foo", EventName: "HttpRequest", Endpoint: "http://example.com/", HttpMethod: "POST" }
            """,
            """
            Verbose: HTTP/1.1 200 OK
            Location: http://example.com/

            {"response":2}
             { SourceContext: "Crip.AspNetCore.Logging.LoggingHandler.Foo", EventName: "HttpResponse", StatusCode: 200, Elapsed: 100, Endpoint: "http://example.com/", HttpMethod: "POST" }
            """,
            "Information: POST http://example.com/ at 00:00:00.100 with 200 OK { SourceContext: \"Crip.AspNetCore.Logging.LoggingHandler.Foo\", EventName: \"HttpResponse\", StatusCode: 200, Elapsed: 100, Endpoint: \"http://example.com/\", HttpMethod: \"POST\" }"
        );
    }

    [Fact, Trait("Category", "Integration")]
    public async Task SendAsync_DoesNotLogsResponseIfConfiguredSo()
    {
        HttpRequestMessage request = new(HttpMethod.Post, "http://example.com");
        request.Content = new StringContent("{\"request\":3}", Encoding.UTF8, "application/json");
        request.Headers.Add("Authorization", "Bearer token-value");

        HttpResponseMessage response = new(HttpStatusCode.OK);
        response.Content = new StringContent("{\"response\":4}", Encoding.UTF8, "application/json");
        response.Headers.Location = new Uri("http://example.com");

        var sut = CreateInvoker(response, LogEventLevel.Verbose, logResponses: false);

        using var ctx = TestCorrelator.CreateContext();
        var result = await sut.SendAsync(request, default);
        
        result
            .Should().NotBeNull()
            .And.Be(response);
        
        var actual = TestCorrelator.GetLogEventsFromCurrentContext().Select(FormatLogEvent).ToList();
        actual.Should().BeEquivalentTo(
            """
            Verbose: POST http://example.com/ HTTP/1.1
            Authorization: Bearer token-value

            {"request":3}
             { SourceContext: "Crip.AspNetCore.Logging.LoggingHandler.Foo", EventName: "HttpRequest", Endpoint: "http://example.com/", HttpMethod: "POST" }
            """,
            "Information: POST http://example.com/ at 00:00:00.100 with 200 OK { SourceContext: \"Crip.AspNetCore.Logging.LoggingHandler.Foo\", EventName: \"HttpResponse\", StatusCode: 200, Elapsed: 100, Endpoint: \"http://example.com/\", HttpMethod: \"POST\" }"
        );
    }

    private HttpMessageInvoker CreateInvoker(HttpResponseMessage response, LogEventLevel logLevel = LogEventLevel.Fatal, bool logResponses = true)
    {
        var serviceProvider = CreateProvider(logLevel, logResponses);
        var handler = serviceProvider.GetRequiredService<LoggingHandler<Foo>>();
        handler.InnerHandler = new TestHttpHandler(response);

        return new HttpMessageInvoker(handler);
    }

    private ServiceProvider CreateProvider(LogEventLevel logLevel, bool logResponses = true)
    {
        return new ServiceCollection()
            .AddSingleton<ILoggerFactory>(i => CreateSerilogLoggerFactory(logLevel))
            .AddSingleton<IRequestLogger>(i => new RequestLogger(new(null), new(null)))
            .AddSingleton<IResponseLogger>(i => new ResponseLogger(new(null), new(null)))
            .AddSingleton<IBasicInfoLogger, BasicInfoLogger>()
            .AddSingleton<IHttpLoggerFactory, HttpLoggerFactory>()
            .AddSingleton<IMeasurable, TimeMeasurable>()
            .AddSingleton<IStopwatch>(i => new MockStopwatch(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.1)))
            .AddSingleton(typeof(LoggingHandler<>))
            .Configure((RequestLoggingOptions options) => options.LogResponse = logResponses)
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
        return configuration(new LoggerConfiguration())
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

    private class Foo
    {
    }
}