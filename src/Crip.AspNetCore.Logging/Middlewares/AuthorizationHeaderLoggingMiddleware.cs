using System;
using System.Collections.Generic;
using System.Linq;
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
    private const string Mask = "*****";
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

    private IEnumerable<string> HeaderNames => _options.Value.AuthorizationHeaders.AuthorizationHeaderNames;
    private IEnumerable<string> Masks => _options.Value.AuthorizationHeaders.AuthorizationHeaderMasks;

    /// <inheritdoc/>
    public string Modify(string key, string value) =>
        HeaderNames.Any(authKey => authKey.Equals(key, Comparison))
            ? $"{Masks.FirstOrDefault(mask => value.StartsWith(mask, Comparison))}{Mask}"
            : value;
}