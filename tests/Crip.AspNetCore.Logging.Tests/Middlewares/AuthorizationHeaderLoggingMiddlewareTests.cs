using Microsoft.Extensions.Options;

namespace Crip.AspNetCore.Logging.Tests.Middlewares;

public class AuthorizationHeaderLoggingMiddlewareTests
{
    [Theory, Trait("Category", "Unit")]
    [InlineData("Authorization", "Basic *secret value*", "Basic *****")]
    [InlineData("Authorization", "Bearer *secret value*", "Bearer *****")]
    [InlineData("Authorization", "Digest *secret value*", "Digest *secret value*")]
    [InlineData("X-Authorization", "Basic *secret value*", "Basic *secret value*")]
    public void Modify(string key, string value, string expected)
    {
        // Arrange
        AuthorizationHeaderLoggingMiddleware sut = new(Options.Create<RequestLoggingOptions>(new()));

        // Act
        var result = sut.Modify(key, value);

        // Assert
        result.Should().Be(expected);
    }

    [Theory, Trait("Category", "Unit")]
    [InlineData("Authorization", "Basic *secret value*", "Basic *****")]
    [InlineData("authorization", "Basic *secret value*", "Basic *****")]
    [InlineData("Authorization", "Bearer *secret value*", "Bearer *****")]
    [InlineData("authorization", "Bearer *secret value*", "Bearer *****")]
    [InlineData("Authorization", "Digest *secret value*", "Digest *secret value*")]
    [InlineData("X-Authorization", "Basic *secret value*", "Basic *****")]
    [InlineData("x-authorization", "Basic *secret value*", "Basic *****")]
    public void ModifyCustomized(string key, string value, string expected)
    {
        // Arrange
        AuthorizationHeaderLoggingMiddleware sut = new(Options.Create<RequestLoggingOptions>(new()
        {
            AuthorizationHeaders = new()
            {
                AuthorizationHeaderNames = new List<string> { "Authorization", "X-Authorization" },
            },
        }));

        // Act
        var result = sut.Modify(key, value);

        // Assert
        result.Should().Be(expected);
    }
}
