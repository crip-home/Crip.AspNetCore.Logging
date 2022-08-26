using System.Linq.Expressions;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging.Tests
{
    public class HttpLoggerTests
    {
        [Fact, Trait("Category", "Unit")]
        public async Task HttpLogger_LogRequest_CallsLoggerWithRequestScope()
        {
            // Arrange
            var (httpLogger, _, loggerMock, requestLoggerMock, _, _) = Mock();
            var context = new FakeHttpContextBuilder()
                .SetMethod(HttpMethod.Head)
                .SetScheme("https")
                .SetHost("example.com")
                .Create();

            var request = RequestDetails.From(context.Request);

            // Act
            await httpLogger.LogRequest(request);

            // Assert
            requestLoggerMock.Verify(
                requestLogger => requestLogger.LogRequest(loggerMock.Object, request),
                Times.Once);

            loggerMock.Verify(
                MatchBeginScope(new()
                {
                    { "EventName", "HttpRequest" },
                    { "Endpoint", "https://example.com" },
                    { "HttpMethod", "HEAD" },
                }),
                Times.Once);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task HttpLogger_LogRequest_SkipsIfPredicatesSaysToSkip()
        {
            // Arrange
            EndpointPredicate predicate = new(exclude: true, patterns: new[] { "/master/*" });
            var (httpLogger, _, loggerMock, requestLoggerMock, _, _) = Mock(predicate: predicate);
            HttpContext context = new FakeHttpContextBuilder()
                .SetMethod(HttpMethod.Head)
                .SetScheme("https")
                .SetHost("example.com")
                .SetPathBase("/master/api/v1")
                .Create();

            var request = RequestDetails.From(context.Request);

            // Act
            await httpLogger.LogRequest(request);

            // Assert
            loggerMock.Verify(
                logger => logger.BeginScope(It.IsAny<It.IsAnyType>()),
                Times.Never);

            requestLoggerMock.Verify(
                logger => logger.LogRequest(It.IsAny<ILogger>(), It.IsAny<RequestDetails>()),
                Times.Never);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task HttpLogger_LogResponse_CallsLoggerWithRequestAndResponseScope()
        {
            // Arrange
            var (httpLogger, stopwatch, loggerMock, _, responseLoggerMock, _) = Mock(15000d);
            HttpContext context = new FakeHttpContextBuilder()
                .SetMethod(HttpMethod.Post)
                .SetScheme("http")
                .SetHost("example.com")
                .SetStatus(HttpStatusCode.Conflict)
                .Create();

            var request = RequestDetails.From(context.Request);
            var response = ResponseDetails.From(context.Response, stopwatch);

            // Act
            await httpLogger.LogResponse(request, response);

            // Assert
            responseLoggerMock.Verify(
                logger => logger.LogResponse(loggerMock.Object, request, response),
                Times.Once);

            loggerMock.Verify(
                MatchBeginScope(new()
                {
                    { "EventName", "HttpRequest" },
                    { "Endpoint", "http://example.com" },
                    { "HttpMethod", "POST" },
                }),
                Times.Once);

            loggerMock.Verify(
                MatchBeginScope(new()
                {
                    { "EventName", "HttpResponse" },
                    { "StatusCode", 409 },
                    { "Elapsed", 15000d },
                }),
                Times.Once);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task HttpLogger_LogResponse_SkipsIfPredicatesSaysToSkip()
        {
            // Arrange
            EndpointPredicate predicate = new(exclude: true, patterns: new[] { "/master/*" });
            var (httpLogger, stopwatch, loggerMock, _, responseLoggerMock, _) = Mock(15000d, predicate);
            HttpContext context = new FakeHttpContextBuilder()
                .SetMethod(HttpMethod.Post)
                .SetScheme("http")
                .SetHost("example.com")
                .SetPathBase("/master/api/v1")
                .SetStatus(HttpStatusCode.Conflict)
                .Create();

            var request = RequestDetails.From(context.Request);
            var response = ResponseDetails.From(context.Response, stopwatch);

            // Act
            await httpLogger.LogResponse(request, response);

            // Assert
            loggerMock.Verify(
                logger => logger.BeginScope(It.IsAny<It.IsAnyType>()),
                Times.Never);

            responseLoggerMock.Verify(
                logger => logger.LogResponse(It.IsAny<ILogger>(), It.IsAny<RequestDetails>(), It.IsAny<ResponseDetails>()),
                Times.Never);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task HttpLogger_LogInfo_CallsLoggerWithRequestAndResponseScope()
        {
            // Arrange
            var (httpLogger, stopwatch, loggerMock, _, _, basicInfoLoggerMock) = Mock(123);
            HttpContext context = new FakeHttpContextBuilder()
                .SetMethod(HttpMethod.Get)
                .SetScheme("http")
                .SetHost("example.com")
                .SetStatus(HttpStatusCode.Ambiguous)
                .Create();

            var request = RequestDetails.From(context.Request);
            var response = ResponseDetails.From(context.Response, stopwatch);

            // Act
            await httpLogger.LogInfo(request, response);

            // Assert
            basicInfoLoggerMock.Verify(
                requestLogger => requestLogger.LogBasicInfo(loggerMock.Object, request, response),
                Times.Once);

            loggerMock.Verify(
                MatchBeginScope(new()
                {
                    { "EventName", "HttpRequest" },
                    { "Endpoint", "http://example.com" },
                    { "HttpMethod", "GET" },
                }),
                Times.Once);

            loggerMock.Verify(
                MatchBeginScope(new()
                {
                    { "EventName", "HttpResponse" },
                    { "StatusCode", 300 },
                    { "Elapsed", 123d },
                }),
                Times.Once);
        }

        [Fact, Trait("Category", "Unit")]
        public async Task HttpLogger_LogInfo_SkipsIfPredicatesSaysToSkip()
        {
            // Arrange
            EndpointPredicate predicate = new(exclude: true, patterns: new[] { "/master/*" });
            var (httpLogger, stopwatch, loggerMock, _, _, basicInfoLoggerMock) = Mock(123, predicate);
            HttpContext context = new FakeHttpContextBuilder()
                .SetMethod(HttpMethod.Get)
                .SetScheme("http")
                .SetHost("example.com")
                .SetPathBase("/master/api/v1")
                .SetStatus(HttpStatusCode.Ambiguous)
                .Create();

            var request = RequestDetails.From(context.Request);
            var response = ResponseDetails.From(context.Response, stopwatch);

            // Act
            await httpLogger.LogInfo(request, response);

            // Assert
            loggerMock.Verify(
                logger => logger.BeginScope(It.IsAny<It.IsAnyType>()),
                Times.Never);

            basicInfoLoggerMock.Verify(
                requestLogger => requestLogger.LogBasicInfo(It.IsAny<ILogger>(), It.IsAny<RequestDetails>(), It.IsAny<ResponseDetails>()),
                Times.Never);
        }

        [Fact, Trait("Category", "Unit")]
        public void HttpLogger_LogError_CallsLoggerWithRequestResponseScope()
        {
            // Arrange
            var (httpLogger, _, loggerMock, _, _, _) = Mock();
            Mock<Exception> exceptionMock = new();
            HttpContext context = new FakeHttpContextBuilder()
                .SetMethod(HttpMethod.Get)
                .SetScheme("http")
                .SetHost("example.com")
                .SetStatus(HttpStatusCode.Forbidden)
                .Create();

            var request = RequestDetails.From(context.Request);
            var response = ResponseDetails.From(context.Response, null);

            // Act
            httpLogger.LogError(exceptionMock.Object, request, response);

            // Assert
            loggerMock.Verify(
                MatchBeginScope(new()
                {
                    { "EventName", "HttpRequest" },
                    { "Endpoint", "http://example.com" },
                    { "HttpMethod", "GET" },
                }),
                Times.Once);

            loggerMock.Verify(
                MatchBeginScope(new()
                {
                    { "EventName", "HttpResponse" },
                    { "StatusCode", 403 },
                    { "Elapsed", 0d },
                }),
                Times.Once);
        }

        [Fact, Trait("Category", "Unit")]
        public void HttpLogger_LogError_CallsLoggerWithRequestAndDefaultStatusScope()
        {
            // Arrange
            var (httpLogger, stopwatch, loggerMock, _, _, _) = Mock(321d);
            Mock<Exception> exceptionMock = new();
            HttpContext context = new FakeHttpContextBuilder()
                .SetMethod(HttpMethod.Get)
                .SetScheme("http")
                .SetHost("example.com")
                .SetStatus(HttpStatusCode.InternalServerError)
                .Create();

            var request = RequestDetails.From(context.Request);
            var response = ResponseDetails.From(context.Response, stopwatch);

            // Act
            httpLogger.LogError(exceptionMock.Object, request, response);

            // Assert
            loggerMock.Verify(
                MatchBeginScope(new()
                {
                    { "EventName", "HttpRequest" },
                    { "Endpoint", "http://example.com" },
                    { "HttpMethod", "GET" },
                }),
                Times.Once);

            loggerMock.Verify(
                MatchBeginScope(new()
                {
                    { "EventName", "HttpResponse" },
                    { "StatusCode", 500 },
                    { "Elapsed", 321d },
                }),
                Times.Once);
        }

        private static Expression<Func<ILogger, IDisposable>> MatchBeginScope(Dictionary<string, object> expected)
        {
            return logger => logger.BeginScope(It.Is<Dictionary<string, object>>(actual =>
                expected.All(actual.Contains)));
        }

        private static (
            HttpLogger httpLogger,
            IStopwatch stopwatch,
            Mock<ILogger> loggerMock,
            Mock<IRequestLogger> requestLoggerMock,
            Mock<IResponseLogger> responseLoggerMock,
            Mock<IBasicInfoLogger> basicInfoLoggerMock) Mock(double elapsed = 100, IHttpRequestPredicate? predicate = null)
        {
            var empty = Enumerable.Empty<IHttpRequestPredicate>();
            var predicates = predicate is null ? empty : new[] { predicate };
            Mock<IStopwatch> stopwatchMock = new();
            Mock<ILogger> loggerMock = new();
            Mock<IRequestLogger> requestLoggerMock = new();
            Mock<IResponseLogger> responseLoggerMock = new();
            Mock<IBasicInfoLogger> basicInfoLoggerMock = new();
            HttpLogger httpLogger = new(
                loggerMock.Object,
                requestLoggerMock.Object,
                responseLoggerMock.Object,
                basicInfoLoggerMock.Object,
                predicates);

            stopwatchMock
                .SetupGet(stopwatch => stopwatch.Elapsed)
                .Returns(TimeSpan.FromMilliseconds(elapsed));

            return (httpLogger, stopwatchMock.Object, loggerMock, requestLoggerMock, responseLoggerMock, basicInfoLoggerMock);
        }
    }
}
