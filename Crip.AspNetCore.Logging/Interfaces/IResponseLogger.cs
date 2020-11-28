using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// HTTP request response logger contract.
    /// </summary>
    public interface IResponseLogger
    {
        /// <summary>
        /// Writes HTTP response log.
        /// </summary>
        /// <param name="logger">The actual logger instance.</param>
        /// <param name="request">The HTTP request.</param>
        /// <param name="response">The HTTP response.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task LogResponse(ILogger logger, RequestDetails request, ResponseDetails response);
    }
}
