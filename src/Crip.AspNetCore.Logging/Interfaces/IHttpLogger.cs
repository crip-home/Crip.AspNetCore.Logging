using System;
using System.Threading.Tasks;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// HTTP request/response detail logger contact.
/// </summary>
public interface IHttpLogger
{
    /// <summary>
    /// Write request log.
    /// </summary>
    /// <param name="request">Request details.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogRequest(RequestDetails request);

    /// <summary>
    /// Write response log.
    /// </summary>
    /// <param name="request">Request details.</param>
    /// <param name="response">Response details.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogResponse(RequestDetails request, ResponseDetails response);

    /// <summary>
    /// Write request basic information log.
    /// </summary>
    /// <param name="request">Request details.</param>
    /// <param name="response">Response details.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task LogInfo(RequestDetails request, ResponseDetails response);

    /// <summary>
    /// Write request execution error log.
    /// </summary>
    /// <param name="exception">Execution instance.</param>
    /// <param name="request">Request details.</param>
    /// <param name="response">Response details.</param>
    void LogError(Exception exception, RequestDetails? request, ResponseDetails? response);
}