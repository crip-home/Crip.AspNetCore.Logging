namespace Crip.AspNetCore.Logging;

/// <summary>
/// Predicate to be applied on request to test if it should be excluded from log.
/// </summary>
public interface IHttpRequestPredicate
{
    /// <summary>
    /// Test if request should be excluded/included in logging.
    /// </summary>
    /// <param name="request">HTTP request object.</param>
    /// <returns>True if request should not be logged.</returns>
    bool Filter(RequestDetails request);
}