using Crip.AspNetCore.Logging.LongJsonContent.Configuration;
using Crip.AspNetCore.Logging.LongJsonContent.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Crip.AspNetCore.Logging.LongJsonContent;

/// <summary>
/// Logging service DI extensions.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds <seealso cref="LongJsonContentMiddleware"/> to the DI service.
    /// </summary>
    /// <param name="services">DI service.</param>
    /// <returns>Updated service collection.</returns>
    public static IServiceCollection AddRequestLoggingLongJson(this IServiceCollection services)
    {
        return services.AddRequestLoggingLongJson(_ => { });
    }

    /// <summary>
    /// Adds <seealso cref="LongJsonContentMiddleware"/> to the DI service.
    /// </summary>
    /// <param name="services">DI service.</param>
    /// <param name="configureOptions">The options configuration callback.</param>
    /// <returns>Updated service collection.</returns>
    public static IServiceCollection AddRequestLoggingLongJson(
        this IServiceCollection services,
        Action<LongJsonContentOptions> configureOptions)
    {
        return services
            .Configure(configureOptions)
            .AddSingleton<IJsonStreamModifier, JsonStreamModifier>()
            .AddSingleton<IRequestContentLogMiddleware, LongJsonContentMiddleware>();
    }
}