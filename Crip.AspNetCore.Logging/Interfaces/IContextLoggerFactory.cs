using Microsoft.AspNetCore.Http;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// HTTP context logger factory contract.
/// </summary>
public interface IContextLoggerFactory
{
    /// <summary>
    /// Create HTTP context logger instance for provided <paramref name="context"/>.
    /// </summary>
    /// <param name="context">HTTP context.</param>
    /// <typeparam name="T">Type of the logger instance name.</typeparam>
    /// <returns>New instance of the <seealso cref="IContextLogger"/>.</returns>
    IContextLogger Create<T>(HttpContext context);
}