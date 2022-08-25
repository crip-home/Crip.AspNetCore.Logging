using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Crip.AspNetCore.Logging.Tests
{
    public class ContextLoggerTests
    {
        private readonly Mock<ILoggerFactory> _loggerFactoryMock = new();
        private readonly Mock<IHttpLoggerFactory> _httpLoggerFactory = new();


        [Fact, Trait("Category", "Unit")]
        public void Constructor_ProperlyCreatesControllerLogger()
        {
            HttpContext context = new FakeHttpContextBuilder().SetEndpoint("Name").Create();

            var _ = new ContextLogger<RequestLoggingMiddleware>(
                _loggerFactoryMock.Object,
                _httpLoggerFactory.Object,
                context);

            _loggerFactoryMock.Verify(
                factory => factory.CreateLogger("Crip.AspNetCore.Logging.RequestLoggingMiddleware.Name"),
                Times.Once);
        }

        [Fact, Trait("Category", "Unit")]
        public void Constructor_ProperlyCreatesControllerLoggerWithActionName()
        {
            HttpContext context = new FakeHttpContextBuilder().SetEndpoint("Name", "Method").Create();

            var _ = new ContextLogger<RequestLoggingMiddleware>(
                _loggerFactoryMock.Object,
                _httpLoggerFactory.Object,
                context);

            _loggerFactoryMock.Verify(
                factory => factory.CreateLogger("Crip.AspNetCore.Logging.RequestLoggingMiddleware.Name.Method"),
                Times.Once);
        }

        [Fact, Trait("Category", "Unit")]
        public void Constructor_ProperlyCreatesControllerLoggerWithRouteName()
        {
            HttpContext context = new FakeHttpContextBuilder().SetEndpoint(endpointName: "EndpointName").Create();

            var _ = new ContextLogger<RequestLoggingMiddleware>(
                _loggerFactoryMock.Object,
                _httpLoggerFactory.Object,
                context);

            _loggerFactoryMock.Verify(
                factory => factory.CreateLogger("Crip.AspNetCore.Logging.RequestLoggingMiddleware.EndpointName"),
                Times.Once);
        }

        [Fact, Trait("Category", "Unit")]
        public void Constructor_ProperlyCreatesMiddlewareLogger()
        {
            Mock<ILoggerFactory> loggerFactoryMock = new();
            Mock<IHttpLoggerFactory> httpLoggerFactoryMock = new();
            HttpContext context = new FakeHttpContextBuilder().Create();

            ContextLogger<RequestLoggingMiddleware> _ = new(
                loggerFactoryMock.Object,
                httpLoggerFactoryMock.Object,
                context);

            loggerFactoryMock.Verify(
                factory => factory.CreateLogger("Crip.AspNetCore.Logging.RequestLoggingMiddleware"),
                Times.Once);
        }
    }
}