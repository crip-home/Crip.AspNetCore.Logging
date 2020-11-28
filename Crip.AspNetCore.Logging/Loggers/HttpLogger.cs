using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging
{
    public class HttpLogger : IHttpLogger
    {
        private readonly ILogger _logger;
        private readonly IRequestLogger _requestLogger;
        private readonly IResponseLogger _responseLogger;
        private readonly IBasicInfoLogger _basicInfoLogger;
        private readonly IEnumerable<IHttpRequestPredicate> _requestPredicates;

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

        public async Task LogRequest(RequestDetails request)
        {
            if (ShouldSkip(request))
            {
                return;
            }

            using var requestScope = RequestScope(request);
            await _requestLogger.LogRequest(_logger, request);
        }

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

        private IDisposable RequestScope(RequestDetails request)
        {
            RequestScope scope = new(request.Url, request.Method);

            return _logger.BeginScope(scope);
        }

        private IDisposable ResponseScope(ResponseDetails response)
        {
            ResponseScope scope = new((int)response.StatusCode, response.Stopwatch);

            return _logger.BeginScope(scope);
        }

        private bool ShouldSkip(RequestDetails request)
        {
            return _requestPredicates.Any(predicate => predicate.Filter(request));
        }
    }
}
