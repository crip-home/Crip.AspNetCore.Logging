using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// HTTP request logging middleware.
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IContextLoggerFactory _contextLoggerFactory;
        private readonly IMeasurable _measurable;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next request delegate.</param>
        /// <param name="contextLoggerFactory">HTTP context logger factory service.</param>
        /// <param name="measurable">The time measure service.</param>
        public RequestLoggingMiddleware(
            RequestDelegate next,
            IContextLoggerFactory contextLoggerFactory,
            IMeasurable measurable)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _contextLoggerFactory = contextLoggerFactory ?? throw new ArgumentNullException(nameof(contextLoggerFactory));
            _measurable = measurable ?? throw new ArgumentNullException(nameof(measurable));
        }

        /// <summary>
        /// <list type="bullet">
        ///   <item>
        ///     <term>Information</term>
        ///     <description>Single entry per request/response with status and execution time.</description>
        ///   </item>
        ///   <item>
        ///     <term>Debug</term>
        ///     <description>Request/response logged separate entries with headers.</description>
        ///   </item>
        ///   <item>
        ///     <term>Verbose</term>
        ///     <description>Request/response logged separate entries with headers and body.</description>
        ///   </item>
        /// </list>
        /// </summary>
        /// <param name="context">HTTP execution context.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Invoke(HttpContext context)
        {
            var logger = _contextLoggerFactory.Create<RequestLoggingMiddleware>(context);
            var measure = _measurable.StartMeasure();

            try
            {
                context.Request.EnableBuffering();
                await logger.LogRequest();

                Stream originalBodyStream = context.Response.Body;
                await using MemoryStream temp = new();
                if (logger.LogLevel <= LogLevel.Trace)
                {
                    context.Response.Body = temp;
                }

                await _next(context);
                var stopwatch = measure.StopMeasure();

                await logger.LogResponse(stopwatch);
                if (logger.LogLevel <= LogLevel.Trace)
                {
                    await temp.CopyToAsync(originalBodyStream);
                }
            }
            catch (Exception exception)
            {
                var stopwatch = measure.StopMeasure();
                logger.LogError(exception, stopwatch);

                throw;
            }
            finally
            {
                var stopwatch = measure.StopMeasure();
                logger.LogInfo(stopwatch).Wait();
            }
        }
    }
}