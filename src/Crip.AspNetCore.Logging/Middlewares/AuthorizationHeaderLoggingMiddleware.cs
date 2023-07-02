using System;
using Microsoft.Extensions.Options;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// Authorization header value middleware.
/// </summary>
/// <remarks>
/// Replaces Basic and Bearer authorization values with asterisk.
/// </remarks>
public class AuthorizationHeaderLoggingMiddleware : IHeaderLogMiddleware
{
    private const StringComparison Comparison = StringComparison.InvariantCultureIgnoreCase;

    private readonly IOptions<RequestLoggingOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationHeaderLoggingMiddleware"/> class.
    /// </summary>
    /// <param name="options">Authorization header options.</param>
    public AuthorizationHeaderLoggingMiddleware(IOptions<RequestLoggingOptions> options)
    {
        _options = options;
    }

    /// <inheritdoc/>
    public string Modify(string key, string value)
    {
        foreach (var authKey in _options.Value.AuthorizationHeaders.AuthorizationHeaderNames)
        {
            if (key.Equals(authKey, Comparison) && value.StartsWith("Basic ", Comparison))
                return "Basic *****";
            if (key.Equals(authKey, Comparison) && value.StartsWith("Bearer ", Comparison))
                return "Bearer *****";
        }

        return value;
    }
}
