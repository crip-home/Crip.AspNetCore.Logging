using Microsoft.AspNetCore.Http;

namespace Crip.AspNetCore.Logging.Tests.Services;

public class EndpointPredicateTests
{
    [Theory, Trait("Category", "Unit")]
    [InlineData("/api/*", "/api/123/foo", true)]
    [InlineData("/api/*", "/api", false)]
    [InlineData("/api/*", "/health", false)]
    [InlineData("/api/*", "/", false)]
    [InlineData("/api/*", "", false)]
    [InlineData("/api", "/api/123/foo", false)]
    [InlineData("/api", "/api", true)]
    [InlineData("/api", "/health", false)]
    [InlineData("/api", "/", false)]
    [InlineData("/api", "", false)]
    public void EndpointPredicate_Filter_ExcludePatterns(string pattern, string path, bool expected)
    {
        var requestMock = MockRequest(path);
        var request = RequestDetails.From(requestMock.Object);

        EndpointPredicate predicate = new(true, new[] { pattern });

        // Act
        var exclude = predicate.Filter(request);

        // Assert
        exclude.Should().Be(expected);
    }

    [Theory, Trait("Category", "Unit")]
    [InlineData("/health", "/api/123/foo", true)]
    [InlineData("/health", "/health-ui", true)]
    [InlineData("/health", "/health", false)]
    [InlineData("/health", "/health/sub", true)]
    [InlineData("/health", "/", true)]
    [InlineData("/health", "", true)]
    [InlineData("/health/*", "/api/123/foo", true)]
    [InlineData("/health/*", "/health-ui", true)]
    [InlineData("/health/*", "/health/", false)]
    [InlineData("/health/*", "/health/sub", false)]
    [InlineData("/health/*", "/", true)]
    [InlineData("/health/*", "", true)]
    public void EndpointPredicate_Filter_IncludePatterns(string pattern, string path, bool expected)
    {
        var requestMock = MockRequest(path);
        var request = RequestDetails.From(requestMock.Object);
        EndpointPredicate predicate = new(false, new[] { pattern });

        // Act
        var exclude = predicate.Filter(request);

        // Assert
        exclude.Should().Be(expected);
    }

    [Fact, Trait("Category", "Unit")]
    public void EndpointPredicate_Filter_MultipleIncludePatterns()
    {
        var requestMock = MockRequest(new PathString("/api/123/foo"));
        var request = RequestDetails.From(requestMock.Object);
        EndpointPredicate predicate = new(false, new[] { "/api", "/health", "/ping", "/" });

        // Act
        var skip = predicate.Filter(request);

        // Assert
        skip.Should().BeTrue();
    }

    [Fact, Trait("Category", "Unit")]
    public void EndpointPredicate_Filter_FailsOnMissingRequestInput()
    {
        // Arrange
        EndpointPredicate predicate = new(false, new[] { "*" });

        // Act
        Action act = () => predicate.Filter(null!);

        // Assert
        act.Should().ThrowExactly<ArgumentNullException>();
    }

    private static Mock<HttpRequest> MockRequest(string path)
    {
        Mock<HttpRequest> requestMock = new();

        requestMock.SetupGet(r => r.Host).Returns(new HostString("localhost"));
        requestMock.SetupGet(r => r.QueryString).Returns(new QueryString(""));
        requestMock.SetupGet(r => r.PathBase).Returns(new PathString(""));
        requestMock.SetupGet(r => r.Scheme).Returns("http");
        requestMock.SetupGet(r => r.Path).Returns(new PathString(path));

        return requestMock;
    }
}