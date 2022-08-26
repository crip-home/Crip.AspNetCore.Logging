namespace Crip.AspNetCore.Logging.Tests
{
    public class AuthorizationHeaderLoggingMiddlewareTests
    {
        [Theory, Trait("Category", "Unit")]
        [InlineData("Authorization", "Basic *secret value*", "Basic *****")]
        [InlineData("Authorization", "Bearer *secret value*", "Bearer *****")]
        [InlineData("Authorization", "Digest *secret value*", "Digest *secret value*")]
        [InlineData("X-Authorization", "Basic *secret value*", "Basic *secret value*")]
        public void AuthorizationHeaderLoggingMiddleware_Modify(string key, string value, string expected)
        {
            // Arrange
            AuthorizationHeaderLoggingMiddleware sut = new();

            // Act
            string result = sut.Modify(key, value);

            // Assert
            result.Should().Be(expected);
        }
    }
}
