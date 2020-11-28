using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// HTTP client logging handler.
    /// </summary>
    /// <typeparam name="T">The type of the client.</typeparam>
    public class LoggingHandler<T> : LoggingHandler
    {
        private readonly IHttpLoggerFactory _httpLoggerFactory;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingHandler{T}"/> class.
        /// </summary>
        /// <param name="loggerFactory">The actual log writer.</param>
        /// <param name="httpLoggerFactory">The HTTP request/response detail logger.</param>
        public LoggingHandler(ILoggerFactory loggerFactory, IHttpLoggerFactory httpLoggerFactory)
        {
            _httpLoggerFactory = httpLoggerFactory;
            _logger = loggerFactory.CreateLogger($"{typeof(LoggingHandler).FullName}.{typeof(T).Name}");
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var httpLogger = _httpLoggerFactory.Create(_logger);
            var stopwatch = CreateStopwatch();
            stopwatch.Start();
            HttpResponseMessage? response = null;
            try
            {
                await httpLogger.LogRequest(RequestDetails.From(request));

                response = await base.SendAsync(request, ct);

                await httpLogger.LogResponse(
                    RequestDetails.From(request),
                    ResponseDetails.From(response, stopwatch));
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                httpLogger.LogError(
                    exception,
                    RequestDetails.From(request),
                    ResponseDetails.From(response, stopwatch));

                throw;
            }
            finally
            {
                httpLogger.LogInfo(
                    RequestDetails.From(request),
                    ResponseDetails.From(response, stopwatch)).Wait(ct);
            }

            return response;
        }

        /// <summary>
        /// Factory method to create Stopwatch wrapper instance.
        /// </summary>
        /// <returns>New SystemStopwatch instance.</returns>
        protected virtual IStopwatch CreateStopwatch()
        {
            return new LoggingStopwatch();
        }
    }

    /// <summary>
    /// Non generic logging handler abstraction.
    /// </summary>
    public abstract class LoggingHandler : DelegatingHandler
    {
    }
}