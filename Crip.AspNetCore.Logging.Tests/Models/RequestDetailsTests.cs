using System;
using System.Collections.Generic;
using System.Net.Http;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Primitives;
using Xunit;
using HttpMethod = Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpMethod;

namespace Crip.AspNetCore.Logging.Tests
{
    public class RequestDetailsTests
    {
        [Fact, Trait("Category", "Unit")]
        public void RequestDetails_From_CreatesFromHttpRequest()
        {
            // Arrange
            HttpRequest request = new FakeHttpContextBuilder()
                .SetMethod(HttpMethod.Put)
                .SetScheme(HttpScheme.Http)
                .SetPathBase("/api")
                .SetPath("/v1")
                .SetQuery(new() { { "cat", "1" } })
                .SetHost(new("localhost"))
                .Create()
                .Request;

            // Act
            var result = RequestDetails.From(request);

            // Assert
            result.Should().NotBeNull()
                .And.BeEquivalentTo(new RequestDetails
                {
                    Method = "PUT",
                    Scheme = "http",
                    Host = "localhost",
                    Path = "/api/v1",
                    QueryString = "?cat=1",
                    Url = "http://localhost/api/v1?cat=1",
                    Protocol = "HTTP/1.1",
                    Headers = new Dictionary<string, StringValues> { { "Host", "localhost" } }
                }, options => options.Excluding(r => r.Content));
        }

        [Fact, Trait("Category", "Unit")]
        public void RequestDetails_From_CreatesFromHttpRequestEmptyBasePath()
        {
            // Arrange
            HttpRequest request = new FakeHttpContextBuilder()
                .SetMethod(HttpMethod.Post)
                .SetScheme(HttpScheme.Https)
                .SetPath("/api/v2")
                .SetHost(new("example.com"))
                .Create()
                .Request;

            // Act
            var result = RequestDetails.From(request);

            // Assert
            result.Should().NotBeNull()
                .And.BeEquivalentTo(new RequestDetails
                {
                    Method = "POST",
                    Scheme = "https",
                    Host = "example.com",
                    Path = "/api/v2",
                    QueryString = "",
                    Url = "https://example.com/api/v2",
                    Protocol = "HTTP/1.1",
                    Headers = new Dictionary<string, StringValues> { { "Host", "example.com" } }
                }, options => options.Excluding(r => r.Content));
        }

        [Fact, Trait("Category", "Unit")]
        public void RequestDetails_From_CreatesFromHttpRequestMessage()
        {
            // Arrange
            Uri uri = new("http://localhost/api/v1?cat=1");
            HttpRequestMessage request = new(System.Net.Http.HttpMethod.Put, uri);

            // Act
            var result = RequestDetails.From(request);

            // Assert
            result.Should().NotBeNull()
                .And.BeEquivalentTo(new RequestDetails
                {
                    Method = "PUT",
                    Scheme = "http",
                    Host = "localhost",
                    Path = "/api/v1",
                    QueryString = "?cat=1",
                    Url = "http://localhost/api/v1?cat=1",
                    Protocol = "HTTP/1.1",
                    Headers = new Dictionary<string, StringValues>(),
                }, options => options.Excluding(r => r.Content));
        }
    }
}
