using System.Collections.Generic;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// Authorization header options.
/// </summary>
public record AuthorizationHeaderOptions
{
    /// <summary>
    /// Gets or sets the collection of the authorization header names.
    /// </summary>
    public ICollection<string> AuthorizationHeaderNames { get; set; } = new List<string> { "Authorization" };
}
