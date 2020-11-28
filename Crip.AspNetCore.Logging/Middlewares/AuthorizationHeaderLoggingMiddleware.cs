using System;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Authorization header value middleware.
    /// </summary>
    /// <remarks>
    /// Replaces Basic and Bearer authorization values with asterisk.
    /// </remarks>
    public class AuthorizationHeaderLoggingMiddleware : IHeaderLogMiddleware
    {
        private const StringComparison Comparison = StringComparison.Ordinal;

        /// <inheritdoc/>
        public string Modify(string key, string value)
        {
            switch (key)
            {
                case "Authorization" when value.StartsWith("Basic ", Comparison):
                    return "Basic *****";

                case "Authorization" when value.StartsWith("Bearer ", Comparison):
                    return "Bearer *****";

                default:
                    return value;
            }
        }
    }
}
