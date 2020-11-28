using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// HTTP logger factory contract.
    /// </summary>
    public interface IHttpLoggerFactory
    {
        /// <summary>
        /// Creates new instance of the <see cref="IHttpLogger"/>.
        /// </summary>
        /// <param name="logger">The logger instance to be used inside requested service.</param>
        /// <returns>The <see cref="IHttpLogger"/> service instance.</returns>
        IHttpLogger Create(ILogger logger);
    }
}