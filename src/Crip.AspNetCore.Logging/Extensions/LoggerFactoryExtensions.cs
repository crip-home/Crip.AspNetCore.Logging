using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// Logger factory extension methods.
/// </summary>
internal static class LoggerFactoryExtensions
{
    /// <summary>
    /// Creates a new <see cref="ILogger"/> instance using the full name of
    /// the given type appending controller name at the end.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="context">Executed HTTP context.</param>
    /// <typeparam name="T">Type of the default logger name.</typeparam>
    /// <returns>The <see cref="ILogger"/> that was created.</returns>
    public static ILogger ControllerLogger<T>(this ILoggerFactory loggerFactory, HttpContext context)
    {
        var controllerName = context.ControllerName();
        if (controllerName is null)
        {
            return loggerFactory.CreateLogger<T>();
        }

        var service = typeof(T).FullName ?? typeof(T).Name;
        return loggerFactory.CreateLogger($"{service}.{controllerName}");
    }
}