using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging.Handlers
{
    /// <summary>
    /// HTTP client logging handler.
    /// </summary>
    /// <typeparam name="T">The type of the client.</typeparam>
    public class LoggingHandler<T> : LoggingHandler
    {
        private readonly ILogger<T> _logger;
        private readonly IHttpLogger _httpLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingHandler{T}"/> class.
        /// </summary>
        public LoggingHandler(
            ILogger<T> logger,
            IHttpLogger httpLogger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpLogger = httpLogger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var stopwatch = CreateStopwatch();
            stopwatch.Start();
            HttpResponseMessage? response = null;
            try
            {
                await _httpLogger.LogRequest(RequestDetails.From(request));

                response = await base.SendAsync(request, ct);

                await _httpLogger.LogResponse(
                    RequestDetails.From(request),
                    ResponseDetails.From(response, stopwatch));
            }
            catch (Exception exception)
            {
                stopwatch.Stop();
                _httpLogger.LogError(
                    exception,
                    RequestDetails.From(request),
                    ResponseDetails.From(response, stopwatch));

                throw;
            }
            finally
            {
                _httpLogger.LogInfo(
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
