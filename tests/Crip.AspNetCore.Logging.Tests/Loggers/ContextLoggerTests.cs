using Crip.AspNetCore.Logging.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crip.AspNetCore.Logging.Tests.Loggers;

public class ContextLoggerTests
{
    private readonly Mock<ILoggerFactory> _loggerFactory = new();
    private readonly Mock<IHttpLoggerFactory> _httpLoggerFactory = new();
    private readonly Mock<IOptions<RequestLoggingOptions>> _options = new();
    private readonly Mock<IStopwatch> _stopwatch = new();
    private readonly Mock<IHttpLogger> _httpLogger = new();

    public ContextLoggerTests()
    {
        _httpLoggerFactory
            .Setup(factory => factory.Create(It.IsAny<ILogger>()))
            .Returns(_httpLogger.Object);
    }

    [Fact, Trait("Category", "Unit")]
    public void Constructor_ProperlyCreatesControllerLogger()
    {
        var context = new FakeHttpContextBuilder().SetEndpoint("Name").Create();

        ContextLogger<RequestLoggingMiddleware> _ = new(
            _loggerFactory.Object,
            _httpLoggerFactory.Object,
            _options.Object,
            context);

        _loggerFactory.Verify(
            factory => factory.CreateLogger("Crip.AspNetCore.Logging.RequestLoggingMiddleware.Name"),
            Times.Once);
    }

    [Fact, Trait("Category", "Unit")]
    public void Constructor_ProperlyCreatesControllerLoggerWithActionName()
    {
        var context = new FakeHttpContextBuilder().SetEndpoint("Name", "Method").Create();

        ContextLogger<RequestLoggingMiddleware> _ = new(
            _loggerFactory.Object,
            _httpLoggerFactory.Object,
            _options.Object,
            context);

        _loggerFactory.Verify(
            factory => factory.CreateLogger("Crip.AspNetCore.Logging.RequestLoggingMiddleware.Name.Method"),
            Times.Once);
    }

    [Fact, Trait("Category", "Unit")]
    public void Constructor_ProperlyCreatesControllerLoggerWithRouteName()
    {
        var context = new FakeHttpContextBuilder().SetEndpoint(endpointName: "EndpointName").Create();

        ContextLogger<RequestLoggingMiddleware> _ = new(
            _loggerFactory.Object,
            _httpLoggerFactory.Object,
            _options.Object,
            context);

        _loggerFactory.Verify(
            factory => factory.CreateLogger("Crip.AspNetCore.Logging.RequestLoggingMiddleware.EndpointName"),
            Times.Once);
    }

    [Fact, Trait("Category", "Unit")]
    public void Constructor_ProperlyCreatesMiddlewareLogger()
    {
        var context = new FakeHttpContextBuilder().Create();

        ContextLogger<RequestLoggingMiddleware> _ = new(
            _loggerFactory.Object,
            _httpLoggerFactory.Object,
            _options.Object,
            context);

        _loggerFactory.Verify(
            factory => factory.CreateLogger("Crip.AspNetCore.Logging.RequestLoggingMiddleware"),
            Times.Once);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task LogResponse_WritesResponseLogMessage()
    {
        var context = new FakeHttpContextBuilder()
            .SetMethod(HttpMethod.Head)
            .SetScheme("https")
            .SetHost("example.com")
            .SetPathBase("/master/api/v1")
            .Create();

        ContextLogger<RequestLoggingMiddleware> contextLogger = new(
            _loggerFactory.Object,
            _httpLoggerFactory.Object,
            Options.Create(new RequestLoggingOptions { LogResponse = true }),
            context);

        await contextLogger.LogResponse(_stopwatch.Object);

        _httpLogger.Verify(
            logger => logger.LogResponse(It.IsAny<RequestDetails>(), It.IsAny<ResponseDetails>()),
            Times.Once);
    }

    [Fact, Trait("Category", "Unit")]
    public async Task LogResponse_DoesNotLogsIfConfiguredSo()
    {
        var context = new FakeHttpContextBuilder().Create();

        ContextLogger<RequestLoggingMiddleware> contextLogger = new(
            _loggerFactory.Object,
            _httpLoggerFactory.Object,
            Options.Create(new RequestLoggingOptions { LogResponse = false }),
            context);

        await contextLogger.LogResponse(_stopwatch.Object);

        _httpLogger.Verify(
            logger => logger.LogResponse(It.IsAny<RequestDetails>(), It.IsAny<ResponseDetails>()),
            Times.Never);
    }
}