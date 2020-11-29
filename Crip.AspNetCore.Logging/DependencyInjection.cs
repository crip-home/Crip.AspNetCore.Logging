using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Logging service DI extensions.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds logging DI.
        /// </summary>
        /// <param name="services">DI service.</param>
        /// <returns>Updated service collection.</returns>
        public static IServiceCollection AddRequestLogging(this IServiceCollection services)
        {
            return services
                .AddSingleton<IMeasurable, TimeMeasurable>()
                .AddTransient<IContextLoggerFactory, ContextLoggerFactory>()
                .AddTransient<IHttpLoggerFactory, HttpLoggerFactory>()
                .AddTransient<IRequestLogger, RequestLogger>()
                .AddTransient<IResponseLogger, ResponseLogger>()
                .AddTransient<IBasicInfoLogger, BasicInfoLogger>()
                .AddTransient<LogContentFactory>()
                .AddTransient<IRequestContentLogMiddleware, LongJsonContentMiddleware>()
                .AddTransient<IJsonStreamModifier, JsonStreamModifier>()
                .AddTransient<LogHeaderFactory>()
                .AddTransient<IHeaderLogMiddleware, AuthorizationHeaderLoggingMiddleware>();
        }

        /// <summary>
        /// Adds endpoint patterns to ignore them from logging handler.
        /// </summary>
        /// <param name="services">DI service.</param>
        /// <param name="ignore">Collection of the endpoints to be ignored.</param>
        /// <returns>Updated service collection.</returns>
        /// <example>
        /// <code>
        ///    services.AddRequestLoggingExclude(new [] { "/api/swagger*", "/api/healthchecks*" })
        /// </code>
        /// Will exclude all request logs to /api/swagger* and /api/healthchecks*.
        /// </example>
        public static IServiceCollection AddRequestLoggingExclude(
            this IServiceCollection services,
            IEnumerable<string> ignore)
        {
            return services.AddSingleton<IHttpRequestPredicate>(provider =>
                new EndpointPredicate(true, ignore));
        }

        /// <summary>
        /// Adds custom HTTP request logging middleware.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>Updated application builder.</returns>
        public static IApplicationBuilder UseRequestLoggingMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}