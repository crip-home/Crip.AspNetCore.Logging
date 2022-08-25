using System;
using System.Collections.Generic;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// HTTP request response logging scope.
/// </summary>
internal class ResponseScope : Dictionary<string, object>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResponseScope"/> class.
    /// </summary>
    /// <param name="statusCode">The status response code.</param>
    /// <param name="stopwatch">The elapsed time stopwatch.</param>
    public ResponseScope(int? statusCode, IStopwatch? stopwatch)
        : base(3)
    {
        Add("EventName", "HttpResponse");
        Add("StatusCode", statusCode!);
        Add("Elapsed", Math.Round(stopwatch?.Elapsed.TotalMilliseconds ?? 0d));
    }
}