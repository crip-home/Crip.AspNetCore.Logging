using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// HTTP context extension methods.
    /// </summary>
    internal static class HttpContextExtensions
    {
        /// <summary>
        /// Get invoked controller name from HTTP context.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <returns>Controller name if available.</returns>
        public static string? ControllerName(this HttpContext context)
        {
            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            var descriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

            return descriptor?.ControllerName;
        }
    }
}
