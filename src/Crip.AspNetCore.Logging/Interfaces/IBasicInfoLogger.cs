using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// HTTP request basic information logger contract.
/// </summary>
public interface IBasicInfoLogger
{
    /// <summary>
    /// Logs request basic information.
    /// </summary>
    /// <param name="logger">The actual logger to write.</param>
    /// <param name="request">The HTTP request details.</param>
    /// <param name="response">The HTTP response details.</param>
    void LogBasicInfo(ILogger logger, RequestDetails request, ResponseDetails response);
}