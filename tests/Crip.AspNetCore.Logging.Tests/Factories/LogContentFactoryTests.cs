using System.Text;
using Microsoft.Extensions.Options;

namespace Crip.AspNetCore.Logging.Tests.Factories;

public class LogContentFactoryTests
{
    [Fact, Trait("Category", "Unit")]
    public async Task PrepareBody_ProperlyHandlesMiddleware()
    {
        // Arrange
        var jsonBuilder = new JsonStreamModifier();
        var longJsonContentOptions = new LongJsonContentOptions { MaxCharCountInField = 16 };
        var options = Options.Create(new RequestLoggingOptions { LongJsonContent = longJsonContentOptions });
        var middlewares = new List<IRequestContentLogMiddleware> { new LongJsonContentMiddleware(jsonBuilder, options) };
        var sut = new LogContentFactory(middlewares);
        var content = """{"trimOn14":"123456789012345","trimOn16":"12345678901234567"}""";
        var contentBytes = Encoding.UTF8.GetBytes(content);
        Stream stream = new MemoryStream(contentBytes);

        // Act
        var result = await sut.PrepareBody("application/json", stream);

        // Assert
        result.Should()
            .NotBeNullOrEmpty()
            .And.BeEquivalentTo(
                $"{Environment.NewLine}{{\"trimOn14\":\"123456789012345\",\"trimOn16\":\"1234567890...\"}}");
    }

    [Fact, Trait("Category", "Unit")]
    public async Task PrepareBody_ProperlyHandlesEmptyMiddlewares()
    {
        // Arrange
        var sut = new LogContentFactory(new List<IRequestContentLogMiddleware>());
        var content = "{\"trimOn14\":\"123456789012345\",\"trimOn16\":\"12345678901234567\"}";
        var contentBytes = Encoding.UTF8.GetBytes(content);
        Stream stream = new MemoryStream(contentBytes);

        // Act
        var result = await sut.PrepareBody("application/json", stream);

        // Assert
        result.Should()
            .NotBeNullOrEmpty()
            .And.BeEquivalentTo(
                $"{Environment.NewLine}{{\"trimOn14\":\"123456789012345\",\"trimOn16\":\"12345678901234567\"}}");
    }
}