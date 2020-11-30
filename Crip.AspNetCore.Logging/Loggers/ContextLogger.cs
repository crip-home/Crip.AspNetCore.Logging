using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// HTTP context logger.
    /// </summary>
    /// <typeparam name="T">Type of the logger instance name.</typeparam>
    public class ContextLogger<T> : IContextLogger
    {
        private readonly HttpContext _context;
        private readonly IHttpLogger _httpLogger;
        private readonly ILogger _logger;
        private int? _statusCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextLogger{T}"/> class.
        /// </summary>
        /// <param name="loggerFactory">Logger instance creator.</param>
        /// <param name="httpLoggerFactory">HTTP request/response logger factory.</param>
        /// <param name="context">The request context.</param>
        public ContextLogger(
            ILoggerFactory loggerFactory,
            IHttpLoggerFactory httpLoggerFactory,
            HttpContext context)
        {
            _logger = loggerFactory.ControllerLogger<T>(context);
            _httpLogger = httpLoggerFactory.Create(_logger);
            _context = context;
        }

        /// <inheritdoc />
        public LogLevel LogLevel => _logger.GetLogLevel();

        /// <inheritdoc cref="IContextLogger" />
        public Task LogRequest()
        {
            var request = RequestDetails.From(_context.Request);
            return _httpLogger.LogRequest(request);
        }

        /// <inheritdoc cref="IContextLogger" />
        public Task LogResponse(IStopwatch stopwatch)
        {
            var request = RequestDetails.From(_context.Request);
            var response = ResponseDetails.From(_context.Response, stopwatch);
            return _httpLogger.LogResponse(request, response);
        }

        /// <inheritdoc cref="IContextLogger" />
        public Task LogInfo(IStopwatch stopwatch)
        {
            var request = RequestDetails.From(_context.Request);
            var response = ResponseDetails.From(_context.Response, stopwatch, _statusCode);
            return _httpLogger.LogInfo(request, response);
        }

        /// <inheritdoc cref="IContextLogger" />
        public void LogError(Exception exception, IStopwatch? stopwatch)
        {
            _statusCode = 500;
            var request = RequestDetails.From(_context.Request);
            var response = ResponseDetails.From(_context.Response, stopwatch, _statusCode);
            _httpLogger.LogError(exception, request, response);
        }
    }
}
