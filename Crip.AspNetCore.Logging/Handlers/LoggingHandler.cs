﻿using System;
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
        private readonly IMeasurable _measure;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingHandler{T}"/> class.
        /// </summary>
        /// <param name="loggerFactory">The actual log writer.</param>
        /// <param name="measure">The time measure service.</param>
        /// <param name="httpLoggerFactory">The HTTP request/response detail logger.</param>
        public LoggingHandler(ILoggerFactory loggerFactory, IHttpLoggerFactory httpLoggerFactory, IMeasurable measure)
        {
            _httpLoggerFactory = httpLoggerFactory;
            _measure = measure;
            _logger = loggerFactory.CreateLogger($"{typeof(LoggingHandler).FullName}.{typeof(T).Name}");
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            var httpLogger = _httpLoggerFactory.Create(_logger);
            var measure = _measure.StartMeasure();
            HttpResponseMessage? response = null;
            RequestDetails requestDetails = RequestDetails.From(request);
            ResponseDetails? responseDetails = null;
            try
            {
                await httpLogger.LogRequest(requestDetails);

                response = await base.SendAsync(request, ct);
                var stopwatch = measure.StopMeasure();

                responseDetails = ResponseDetails.From(response, stopwatch);
                await httpLogger.LogResponse(requestDetails, responseDetails);

                return response;
            }
            catch (Exception exception)
            {
                var stopwatch = measure.StopMeasure();
                httpLogger.LogError(
                    exception,
                    requestDetails,
                    responseDetails ?? ResponseDetails.From(response, stopwatch));

                throw;
            }
            finally
            {
                var stopwatch = measure.StopMeasure();
                httpLogger.LogInfo(
                    requestDetails,
                    responseDetails ?? ResponseDetails.From(response, stopwatch)).Wait(ct);
            }
        }
    }

    /// <summary>
    /// Non generic logging handler abstraction.
    /// </summary>
    public abstract class LoggingHandler : DelegatingHandler
    {
    }
}