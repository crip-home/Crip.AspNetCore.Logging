using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// HTTP request/response detail logger.
    /// </summary>
    public class HttpLogger : IHttpLogger
    {
        private readonly ILogger _logger;
        private readonly IRequestLogger _requestLogger;
        private readonly IResponseLogger _responseLogger;
        private readonly IBasicInfoLogger _basicInfoLogger;
        private readonly IEnumerable<IHttpRequestPredicate> _requestPredicates;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpLogger"/> class.
        /// </summary>
        /// <param name="logger">Actual logger instance to use as writer.</param>
        /// <param name="requestLogger">Request detail logger.</param>
        /// <param name="responseLogger">Response detail logger.</param>
        /// <param name="basicInfoLogger">Basic information logger.</param>
        /// <param name="requestPredicates">Request path predicates to determine is the log message required.</param>
        public HttpLogger(
            ILogger logger,
            IRequestLogger requestLogger,
            IResponseLogger responseLogger,
            IBasicInfoLogger basicInfoLogger,
            IEnumerable<IHttpRequestPredicate> requestPredicates)
        {
            _logger = logger;
            _requestLogger = requestLogger;
            _responseLogger = responseLogger;
            _basicInfoLogger = basicInfoLogger;
            _requestPredicates = requestPredicates;
        }

        /// <inheritdoc />
        public async Task LogRequest(RequestDetails request)
        {
            if (ShouldSkip(request))
            {
                return;
            }

            using var requestScope = RequestScope(request);
            await _requestLogger.LogRequest(_logger, request);
        }

        /// <inheritdoc />
        public async Task LogResponse(RequestDetails request, ResponseDetails response)
        {
            if (ShouldSkip(request))
            {
                return;
            }

            using var requestScope = RequestScope(request);
            using var responseScope = ResponseScope(response);
            await _responseLogger.LogResponse(_logger, request, response);
        }

        /// <inheritdoc />
        public Task LogInfo(RequestDetails request, ResponseDetails response)
        {
            if (ShouldSkip(request))
            {
                return Task.CompletedTask;
            }

            using var requestScope = RequestScope(request);
            using var responseScope = ResponseScope(response);

            _basicInfoLogger.LogBasicInfo(_logger, request, response);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void LogError(
            Exception exception,
            RequestDetails? request,
            ResponseDetails? response)
        {
            var statusCode = ((int?)response?.StatusCode) ?? 500;
            ResponseScope scope = new(statusCode, response?.Stopwatch);

            using var requestScope = RequestScope(request);
            using var responseScope = _logger.BeginScope(scope);

            _logger.LogError(exception, "Error during HTTP request processing");
        }

        private IDisposable RequestScope(RequestDetails? request)
        {
            var scope = new RequestScope();
            if (request is not null)
            {
                scope = new RequestScope(request.Url ?? string.Empty, request.Method ?? string.Empty);
            }

            return _logger.BeginScope(scope);
        }

        private IDisposable ResponseScope(ResponseDetails response)
        {
            ResponseScope scope = new((int?)response.StatusCode, response.Stopwatch);

            return _logger.BeginScope(scope);
        }

        private bool ShouldSkip(RequestDetails request)
        {
            return _requestPredicates.Any(predicate => predicate.Filter(request));
        }
    }
}