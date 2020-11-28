using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Request logger contract.
    /// </summary>
    public interface IRequestLogger
    {
        /// <summary>
        /// Writes HTTP request log.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="request">The HTTP request.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task LogRequest(ILogger logger, RequestDetails request);
    }
}
