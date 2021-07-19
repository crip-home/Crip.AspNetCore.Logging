using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Primitives;

namespace Crip.AspNetCore.Logging.Tests
{
    public class FakeHttpContextBuilder
    {
        private readonly DefaultHttpContext _context;

        public FakeHttpContextBuilder(string protocol = "HTTP/1.1")
        {
            _context = new DefaultHttpContext();
            _context.Request.Protocol = protocol;
        }

        private FakeHttpContextBuilder(DefaultHttpContext context)
        {
            _context = context;
        }

        public FakeHttpContextBuilder SetEndpoint(string controllerName)
        {
            _context.SetEndpoint(CreateEndpoint(controllerName));

            return new(_context);
        }

        public FakeHttpContextBuilder SetMethod(HttpMethod method)
        {
            _context.Request.Method = method.ToString().ToUpper();
            return new(_context);
        }

        public FakeHttpContextBuilder SetScheme(HttpScheme scheme)
        {
            _context.Request.Scheme = scheme.ToString().ToLower();
            return new(_context);
        }

        public FakeHttpContextBuilder SetHost(HostString host)
        {
            _context.Request.Host = host;
            return new(_context);
        }

        public FakeHttpContextBuilder SetPathBase(PathString path)
        {
            _context.Request.PathBase = path;
            return new(_context);
        }

        public FakeHttpContextBuilder SetPath(PathString path)
        {
            _context.Request.Path = path;
            return new(_context);
        }

        public FakeHttpContextBuilder SetQuery(Dictionary<string, string?> query)
        {
            _context.Request.QueryString = QueryString.Create(query);
            return new(_context);
        }

        public FakeHttpContextBuilder SetRequestHeaders(Dictionary<string, StringValues>? headers)
        {
            AddRange(_context.Request.Headers, headers);
            return new(_context);
        }

        public FakeHttpContextBuilder SetResponseHeaders(Dictionary<string, StringValues>? headers)
        {
            AddRange(_context.Response.Headers, headers);
            return new(_context);
        }

        public FakeHttpContextBuilder SetRequestContentType(string contentType)
        {
            _context.Request.ContentType = contentType;
            return new(_context);
        }

        public FakeHttpContextBuilder SetResponseContentType(string contentType)
        {
            _context.Response.ContentType = contentType;
            return new(_context);
        }

        public FakeHttpContextBuilder SetRequestBody(string content)
        {
            _context.Request.Body = new MemoryStream();
            byte[] bodyBytes = Encoding.UTF8.GetBytes(content);
            _context.Request.Body.Write(bodyBytes, 0, content.Length);
            _context.Request.Body.Position = 0;

            return new(_context);
        }

        public FakeHttpContextBuilder SetResponseBody(string content)
        {
            _context.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return new(_context);
        }

        public FakeHttpContextBuilder SetStatus(HttpStatusCode status)
        {
            _context.Response.StatusCode = (int)status;
            return new(_context);
        }

        public FakeHttpContextBuilder SetRequest(
            HttpMethod method = HttpMethod.Get,
            HttpScheme scheme = HttpScheme.Http,
            Dictionary<string, string?>? queryParams = null,
            Dictionary<string, StringValues>? headers = null) =>
            SetMethod(method)
                .SetScheme(scheme)
                .SetHost(new("localhost"))
                .SetPathBase("/master")
                .SetPath("/slave")
                .SetQuery(queryParams ?? new Dictionary<string, string?>())
                .SetRequestHeaders(headers);

        public HttpContext Create() => _context;

        private static Endpoint CreateEndpoint(string controllerName) =>
            new Endpoint(
                _ => Task.CompletedTask,
                EndpointMetadata(controllerName),
                $"{controllerName}Controller");

        private static EndpointMetadataCollection EndpointMetadata(string name) =>
            new(new ControllerActionDescriptor { ControllerName = name });

        private static void AddRange(IHeaderDictionary target, IDictionary<string, StringValues>? values)
        {
            if (values is null) return;

            foreach (var (key, value) in values)
            {
                target[key] = value;
            }
        }
    }
}