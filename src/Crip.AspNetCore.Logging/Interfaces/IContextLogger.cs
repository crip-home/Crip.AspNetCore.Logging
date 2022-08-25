using Microsoft.Extensions.Logging;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// HTTP context logger contract.
/// </summary>
public interface IContextLogger : IClientLogger
{
    /// <summary>
    /// Gets current logger level.
    /// </summary>
    LogLevel LogLevel { get; }
}