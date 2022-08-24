using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Crip.AspNetCore.Logging.Tests
{
    public class RequestLoggerTests
    {
        [Theory, Trait("Category", "Unit")]
        [InlineData(LogLevel.None)]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Information)]
        public async Task RequestLogger_LogRequest_DoesNotWritesLogIfLevelIsNotSufficient(LogLevel level)
        {
            // Arrange
            Mock<ILogger> loggerMock = CreateFor(level);
            HttpContext context = new FakeHttpContextBuilder().SetHost("localhost").Create();
            RequestLogger sut = new(new(null), new(null));

            // Act
            await sut.LogRequest(loggerMock.Object, RequestDetails.From(context.Request));

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }

        [Theory, Trait("Category", "Unit")]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        public async Task RequestLogger_LogRequest_WritesLogIfLevelIsSufficient(LogLevel level)
        {
            // Arrange
            Mock<ILogger> loggerMock = CreateFor(level);
            HttpContext context = new FakeHttpContextBuilder().SetHost("localhost").Create();
            RequestLogger sut = new(new(null), new(null));

            // Act
            await sut.LogRequest(loggerMock.Object, RequestDetails.From(context.Request));

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task RequestLogger_LogRequest_DebugWritesOnlyUrlAndHeaders()
        {
            // Arrange
            Mock<ILogger> loggerMock = CreateFor(LogLevel.Debug);
            HttpContext context = new FakeHttpContextBuilder()
                .SetRequest(
                    HttpMethod.Post,
                    "https",
                    new() { { "foo", "bar" } },
                    new() { { "foo", new(new[] { "bar", "baz" }) } })
                .Create();

            RequestLogger sut = new(new(null), new(null));

            // Act
            await sut.LogRequest(loggerMock.Object, RequestDetails.From(context.Request));

            // Assert
            loggerMock.VerifyLogging(
                $"POST https://localhost/master/slave?foo=bar HTTP/1.1{Environment.NewLine}" +
                $"Host: localhost{Environment.NewLine}" +
                $"foo: bar,baz{Environment.NewLine}",
                LogLevel.Debug);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task RequestLogger_LogRequest_TraceWritesUrlAndHeadersAndBody()
        {
            // Arrange
            Mock<ILogger> loggerMock = CreateFor(LogLevel.Trace);
            HttpContext context = new FakeHttpContextBuilder()
                .SetRequestBody("request content string")
                .SetRequestHeaders(new() { { "foo", new(new[] { "bar", "baz" }) } })
                .SetRequestContentType("text/plain")
                .SetMethod(HttpMethod.Post)
                .SetScheme("https")
                .SetHost("example.org")
                .SetQuery(new() { { "foo", "bar" } })
                .Create();

            RequestLogger sut = new(new(null), new(null));

            // Act
            await sut.LogRequest(loggerMock.Object, RequestDetails.From(context.Request));

            // Assert
            loggerMock.VerifyLogging(
                $"POST https://example.org?foo=bar HTTP/1.1{Environment.NewLine}" +
                $"foo: bar,baz{Environment.NewLine}" +
                $"Content-Type: text/plain{Environment.NewLine}" +
                $"Host: example.org{Environment.NewLine}" +
                $"{Environment.NewLine}" +
                $"request content string{Environment.NewLine}",
                LogLevel.Trace);
        }


        [Fact, Trait("Category", "Unit")]
        public async Task RequestLogger_LogRequest_AppliesHeaderMiddleware()
        {
            // Arrange
            Mock<ILogger> loggerMock = CreateFor(LogLevel.Debug);
            Mock<IHeaderLogMiddleware> headerMiddlewareMock = new();
            var headerMiddlewares = new List<IHeaderLogMiddleware> { headerMiddlewareMock.Object };
            LogHeaderFactory headerFactory = new(headerMiddlewares);
            HttpContext context = new FakeHttpContextBuilder()
                .SetRequest(headers: new() { { "foo", new(new[] { "bar", "baz" }) } })
                .Create();

            RequestLogger sut = new(new(null), headerFactory);

            // Mock
            headerMiddlewareMock
                .Setup(middleware => middleware.Modify(It.IsAny<string>(), It.IsAny<string>()))
                .Returns<string, string>((key, value) => value);

            headerMiddlewareMock
                .Setup(middleware => middleware.Modify("foo", It.IsAny<string>()))
                .Returns("*modified value*");

            // Act
            await sut.LogRequest(loggerMock.Object, RequestDetails.From(context.Request));

            // Assert
            loggerMock.VerifyLogging(
                $"GET http://localhost/master/slave HTTP/1.1{Environment.NewLine}" +
                $"Host: localhost{Environment.NewLine}" +
                $"foo: *modified value*{Environment.NewLine}",
                LogLevel.Debug);
        }

        private Mock<ILogger> CreateFor(LogLevel level)
        {
            Mock<ILogger> loggerMock = new();
            loggerMock
                .Setup(logger => logger.IsEnabled(level))
                .Returns(true);

            return loggerMock;
        }
    }
}