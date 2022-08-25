using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// HTTP context extension methods.
/// </summary>
internal static class HttpContextExtensions
{
    /// <summary>
    /// Gets invoked controller name and action or route name.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>Controller or route if available.</returns>
    public static string? InvocationName(this HttpContext context)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
        var descriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();
        var name = descriptor is not null ? ControllerNameOf(descriptor) : string.Empty;

        return string.IsNullOrWhiteSpace(name) ? RouteNameOf(endpoint) : name;
    }

    private static string? RouteNameOf(Endpoint? endpoint) =>
        endpoint?.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName;

    private static string ControllerNameOf(ControllerActionDescriptor descriptor)
    {
        var nameParts = new[] { descriptor.ControllerName, descriptor.ActionName };
        var cleanParts = nameParts.Where(part => !string.IsNullOrWhiteSpace(part));

        return string.Join(".", cleanParts);
    }
}