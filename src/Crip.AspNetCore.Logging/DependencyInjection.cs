using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// Logging service DI extensions.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds logging to DI.
    /// </summary>
    /// <param name="services">DI service.</param>
    /// <returns>Updated service collection.</returns>
    public static IServiceCollection AddRequestLogging(this IServiceCollection services) =>
        services.AddRequestLogging(_ => { });

    /// <summary>
    /// Adds logging to DI and configure options.
    /// </summary>
    /// <param name="services">DI service.</param>
    /// <param name="configureOptions">The options configuration callback.</param>
    /// <returns>Updated service collection.</returns>
    public static IServiceCollection AddRequestLogging(this IServiceCollection services, Action<RequestLoggingOptions> configureOptions) =>
        services
            .Configure(configureOptions)
            .AddSingleton<IMeasurable, TimeMeasurable>()
            .AddTransient<IStopwatch, LoggingStopwatch>()
            .AddTransient<IContextLoggerFactory, ContextLoggerFactory>()
            .AddTransient<IHttpLoggerFactory, HttpLoggerFactory>()
            .AddTransient<IRequestLogger, RequestLogger>()
            .AddTransient<IResponseLogger, ResponseLogger>()
            .AddTransient<IBasicInfoLogger, BasicInfoLogger>()
            .AddTransient<LogContentFactory>()
            .AddTransient<LogHeaderFactory>()
            .AddTransient<IHeaderLogMiddleware, AuthorizationHeaderLoggingMiddleware>();

    /// <summary>
    /// Adds endpoint patterns to ignore them from logging handler.
    /// </summary>
    /// <param name="services">DI service.</param>
    /// <param name="ignore">Collection of the endpoints to be ignored.</param>
    /// <returns>Updated service collection.</returns>
    /// <example>
    /// <code>
    ///     services.AddRequestLoggingExclude(new [] { "/api/swagger*", "/api/healthchecks*" })
    /// </code>
    /// Will exclude all request logs to /api/swagger* and /api/healthchecks*.
    /// </example>
    public static IServiceCollection AddRequestLoggingExclude(
        this IServiceCollection services,
        params string[] ignore)
    {
        return services.AddSingleton<IHttpRequestPredicate>(_ =>
            new EndpointPredicate(true, ignore));
    }

    /// <summary>
    /// Adds <see cref="CookieHeaderLoggingMiddleware"/> for <see cref="RequestLoggingMiddleware"/> to hide
    /// cookie values in log messages.
    /// </summary>
    /// <param name="services">DI service.</param>
    /// <returns>Updated service collection.</returns>
    /// <example>
    /// <code>
    ///     services
    ///         .AddRequestLogging()
    ///         .AddRequestLoggingCookieValueMiddleware();
    /// </code>
    /// The cookie header will write only first 10 characters.
    /// <code>
    ///     Cookie: .AspNetCore.Identity.Application=CfDJ8NfEhW***
    /// </code>
    /// </example>
    public static IServiceCollection AddRequestLoggingCookieValueMiddleware(this IServiceCollection services)
    {
        return services.AddSingleton<IHeaderLogMiddleware, CookieHeaderLoggingMiddleware>();
    }

    /// <summary>
    /// Adds custom HTTP request logging middleware.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>Updated application builder.</returns>
    public static IApplicationBuilder UseRequestLoggingMiddleware(this IApplicationBuilder app)
    {
        if (!app.IsRegistered<IContextLoggerFactory>() ||
            !app.IsRegistered<IMeasurable>() ||
            !app.IsRegistered<LogContentFactory>() ||
            !app.IsRegistered<LogHeaderFactory>() ||
            !app.IsRegistered<IBasicInfoLogger>() ||
            !app.IsRegistered<IResponseLogger>() ||
            !app.IsRegistered<IRequestLogger>() ||
            !app.IsRegistered<IHttpLoggerFactory>())
        {
            throw new InvalidOperationException(
                $"Unable to find the required services. " +
                $"Please add all the required services by calling " +
                $"{nameof(IServiceCollection)}.{nameof(DependencyInjection.AddRequestLogging)} " +
                $"inside the call to 'ConfigureServices(...)' in the application startup code.");
        }

        return app.UseMiddleware<RequestLoggingMiddleware>();
    }

    /// <summary>
    /// Add request logging handler to the <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>Updated service collection.</returns>
    public static IServiceCollection AddRequestLoggingHandler(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(LoggingHandler<>));

        return services;
    }

    private static bool IsRegistered<T>(this IApplicationBuilder app)
    {
        return app.ApplicationServices.GetService<T>() is not null;
    }
}