using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
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
        /// <param name="config">Application configuration.</param>
        public static void AddRequestLogging(this IServiceCollection services, IConfiguration config)
        {
            services.AddTransient<IContextLoggerFactory, ContextLoggerFactory>();
            services.AddTransient<IHttpLoggerFactory, HttpLoggerFactory>();
            services.AddTransient<IRequestLogger, RequestLogger>();
            services.AddTransient<IResponseLogger, ResponseLogger>();
            services.AddTransient<IBasicInfoLogger, BasicInfoLogger>();

            services.AddTransient<LogContentFactory>();
            services.AddTransient<IRequestContentLogMiddleware, LongJsonContentMiddleware>();
            services.AddTransient<IJsonStreamModifier, JsonStreamModifier>();

            services.AddTransient<LogHeaderFactory>();
            services.AddTransient<IHeaderLogMiddleware, AuthorizationHeaderLoggingMiddleware>();
        }

        /// <summary>
        /// Adds endpoint patterns to ignore them from logging handler.
        /// </summary>
        /// <param name="services">DI service.</param>
        /// <param name="ignore">Collection of the endpoints to be ignored.</param>
        /// <example>
        /// <code>
        ///    services.AddLogEndpointIgnorePredicate(new [] { "/api/swagger*", "/api/healthchecks*" })
        /// </code>
        /// Will exclude all request logs to /api/swagger* and /api/healthchecks*.
        /// </example>
        public static void AddRequestLoggingIgnorePredicate(
            this IServiceCollection services,
            IEnumerable<string> ignore)
        {
            services.AddSingleton<IHttpRequestPredicate>(provider =>
                new EndpointPredicate(false, ignore));
        }

        /// <summary>
        /// Adds custom HTTP request logging middleware.
        /// </summary>
        /// <param name="app">The application builder.</param>
        public static void UseRequestLoggingMiddleware(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestLoggingMiddleware>();
        }
    }
}
