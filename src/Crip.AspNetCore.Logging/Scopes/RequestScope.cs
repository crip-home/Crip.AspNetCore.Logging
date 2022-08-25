using System.Collections.Generic;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// HTTP request logging scope.
/// </summary>
internal class RequestScope : Dictionary<string, object>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequestScope"/> class.
    /// </summary>
    public RequestScope()
        : base(1)
    {
        Add("EventName", "HttpRequest");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestScope"/> class.
    /// </summary>
    /// <param name="endpoint">The request endpoint address.</param>
    /// <param name="method">HTTP request method.</param>
    public RequestScope(string endpoint, string method)
        : base(3)
    {
        Add("EventName", "HttpRequest");
        Add("Endpoint", endpoint);
        Add("HttpMethod", method);
    }
}