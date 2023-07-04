namespace Crip.AspNetCore.Logging;

/// <summary>
/// Request logging configuration.
/// </summary>
public record RequestLoggingOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether request response should be logged.
    /// </summary>
    public bool LogResponse { get; set; } = true;

    /// <summary>
    /// Gets or sets Authorization header middleware options.
    /// </summary>
    public AuthorizationHeaderOptions AuthorizationHeaders { get; set; } = new();
}