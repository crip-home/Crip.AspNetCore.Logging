using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Crip.AspNetCore.Logging.Tests
{
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
            // Arrange
            Mock<HttpRequest> requestMock = new();

            // Mock
            requestMock.SetupGet(r => r.Path).Returns(new PathString(path));
            var request = RequestDetails.From(requestMock.Object);

            EndpointPredicate predicate = new(true, new[] { pattern });

            // Act
            bool exclude = predicate.Filter(request);

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
            // Arrange
            Mock<HttpRequest> requestMock = new();

            // Mock 
            requestMock.SetupGet(r => r.Path).Returns(new PathString(path));
            var request = RequestDetails.From(requestMock.Object);
            EndpointPredicate predicate = new(false, new[] { pattern });

            // Act
            bool exclude = predicate.Filter(request);

            // Assert
            exclude.Should().Be(expected);
        }

        [Fact, Trait("Category", "Unit")]
        public void EndpointPredicate_Filter_MultipleIncludePatterns()
        {
            // Arrange
            Mock<HttpRequest> requestMock = new();

            // Mock
            requestMock.SetupGet(r => r.Path).Returns(new PathString("/api/123/foo"));
            var request = RequestDetails.From(requestMock.Object);
            EndpointPredicate predicate = new(false, new[] { "/api", "/health", "/ping", "/" });

            // Act
            bool skip = predicate.Filter(request);

            // Assert
            skip.Should().BeTrue();
        }
    }
}
