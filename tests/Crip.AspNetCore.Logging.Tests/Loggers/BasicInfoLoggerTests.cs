using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging.Tests
{
    public class BasicInfoLoggerTests
    {
        [Theory, Trait("Category", "Unit")]
        [InlineData(LogLevel.None)]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Warning)]
        public void BasicInfoLogger_LogBasicInfo_DoesNotWritesLogIfLevelIsNotSufficient(LogLevel level)
        {
            // Arrange
            Mock<ILogger> loggerMock = CreateFor(level);
            Mock<IStopwatch> stopwatchMock = new();
            HttpContext context = new FakeHttpContextBuilder().SetHost("localhost").Create();
            var request = RequestDetails.From(context.Request);
            var response = ResponseDetails.From(context.Response, stopwatchMock.Object);
            BasicInfoLogger sut = new();

            // Act
            sut.LogBasicInfo(loggerMock.Object, request, response);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never());
        }

        [Theory, Trait("Category", "Unit")]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Trace)]
        public void BasicInfoLogger_LogBasicInfo_WritesLogIfLevelIsSufficient(LogLevel level)
        {
            // Arrange
            Mock<ILogger> loggerMock = CreateFor(level);
            Mock<IStopwatch> stopwatchMock = new();
            HttpContext context = new FakeHttpContextBuilder().SetHost("localhost").Create();
            var request = RequestDetails.From(context.Request);
            var response = ResponseDetails.From(context.Response, stopwatchMock.Object);
            BasicInfoLogger sut = new();

            // Act
            sut.LogBasicInfo(loggerMock.Object, request, response);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact, Trait("Category", "Unit")]
        public void BasicInfoLogger_LogBasicInfo_WritesSuccessfulPostRequestDetails()
        {
            // Arrange
            Mock<ILogger> loggerMock = CreateFor(LogLevel.Trace);
            Mock<IStopwatch> stopwatchMock = new();
            stopwatchMock.SetupGet(stopwatch => stopwatch.Elapsed).Returns(TimeSpan.FromSeconds(2));

            HttpContext context = new FakeHttpContextBuilder()
                .SetRequest(
                    HttpMethod.Post,
                    "https",
                    new() { { "cat", "221" } })
                .Create();

            var request = RequestDetails.From(context.Request);
            var response = ResponseDetails.From(context.Response, stopwatchMock.Object);
            BasicInfoLogger sut = new();

            // Act
            sut.LogBasicInfo(loggerMock.Object, request, response);

            // Assert
            loggerMock.VerifyLogging(
                "POST https://localhost/master/slave?cat=221 at 00:00:02.000 with 200 OK",
                LogLevel.Information);
        }

        [Fact, Trait("Category", "Unit")]
        public void BasicInfoLogger_LogBasicInfo_WritesErrorPostRequestDetails()
        {
            // Arrange
            Mock<ILogger> loggerMock = CreateFor(LogLevel.Trace);
            Mock<IStopwatch> stopwatchMock = new();
            stopwatchMock.SetupGet(stopwatch => stopwatch.Elapsed).Returns(TimeSpan.FromSeconds(3));

            HttpContext context = new FakeHttpContextBuilder()
                .SetMethod(HttpMethod.Post)
                .SetScheme("http")
                .SetHost("example.com")
                .SetPathBase("/primary")
                .SetPath("/secondary")
                .SetQuery(new() { { "cats", "1" } })
                .SetStatus(HttpStatusCode.InternalServerError)
                .Create();

            var request = RequestDetails.From(context.Request);
            var response = ResponseDetails.From(context.Response, stopwatchMock.Object);
            BasicInfoLogger sut = new();

            // Act
            sut.LogBasicInfo(loggerMock.Object, request, response);

            // Assert
            loggerMock.VerifyLogging(
                "POST http://example.com/primary/secondary?cats=1 at 00:00:03.000 with 500 InternalServerError",
                LogLevel.Information);
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
