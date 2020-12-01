#pragma warning disable 1591
#pragma warning disable SA1600

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Primitives;

namespace Crip.AspNetCore.Logging
{
    public class RequestDetails
    {
        public RequestDetails()
        {
        }

        private RequestDetails(HttpRequest? request)
        {
            if (request is null)
            {
                return;
            }

            Protocol = request.Protocol;
            Method = request.Method;
            Scheme = request.Scheme;
            Host = request.Host.Value;
            Path = FluentUrl.Combine(request.PathBase, request.Path);
            QueryString = request.QueryString.Value;
            Url = request.GetDisplayUrl();
            Headers = request.Headers;
            Content = request.Body;
            ContentType = request.ContentType;
        }

        private RequestDetails(HttpRequestMessage? request)
        {
            if (request is null)
            {
                return;
            }

            Protocol = $"HTTP/{request.Version}";
            Method = request.Method.Method;
            Scheme = request.RequestUri?.Scheme;
            Host = request.RequestUri?.Host;
            Path = request.RequestUri?.AbsolutePath;
            QueryString = request.RequestUri?.Query;
            Url = request.RequestUri?.ToString();
            Headers = request.Headers.ToDictionary(
                header => header.Key,
                header => new StringValues(header.Value.ToArray()));

            Content = request.Content?.ReadAsStreamAsync().GetAwaiter().GetResult();
            ContentType = request.Content?.Headers.ContentType?.ToString();
        }

        public string? Protocol { get; init; }

        public string? Method { get; init; }

        public string? Scheme { get; init; }

        public string? Host { get; init; }

        public string? Path { get; init; }

        public string? QueryString { get; init; }

        public string? Url { get; init; }

        public string? ContentType { get; init; }

        public IDictionary<string, StringValues>? Headers { get; init; }

        public Stream? Content { get; init; }

        public static RequestDetails From(HttpRequestMessage? request) =>
            request is null ? new RequestDetails() : new RequestDetails(request);

        public static RequestDetails From(HttpRequest? request) =>
            request is null ? new RequestDetails() : new RequestDetails(request);
    }
}