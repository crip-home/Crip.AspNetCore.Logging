using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Crip.AspNetCore.Logging.Tests
{
    public class RequestDetailsTests
    {
        [Fact, Trait("Category", "Unit")]
        public void RequestDetails_From_CreatesFromHttpRequest()
        {
            // Arrange
            HttpRequest request = new FakeHttpContextBuilder()
                .SetMethod(new HttpMethod("PUT"))
                .SetScheme("http")
                .SetPathBase("/api")
                .SetPath("/v1")
                .SetQuery(new() { { "cat", "1" } })
                .SetHost("localhost")
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
                .SetScheme("https")
                .SetPath("/api/v2")
                .SetHost("example.com")
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
            HttpRequestMessage request = new(HttpMethod.Put, uri);

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
