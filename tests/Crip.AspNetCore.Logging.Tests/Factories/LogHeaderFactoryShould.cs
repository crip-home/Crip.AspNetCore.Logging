using Microsoft.Extensions.Primitives;

namespace Crip.AspNetCore.Logging.Tests.Factories;

public class LogHeaderFactoryShould
{
    private readonly Mock<IHeaderLogMiddleware> _middleware1 = new();
    private readonly Mock<IHeaderLogMiddleware> _middleware2 = new();

    [Fact, Trait("Category", "Unit")]
    public void Constructor_DoesNotFailIfNoMiddlewaresProvided()
    {
        var act = () => new LogHeaderFactory(null);

        act.Should().NotThrow();
    }

    [Fact, Trait("Category", "Unit")]
    public void Constructor_DoesNotFailIfMiddlewaresProvided()
    {
        var act = () => new LogHeaderFactory(new[] { _middleware1.Object, _middleware2.Object });

        act.Should().NotThrow();
    }

    [Fact, Trait("Category", "Unit")]
    public void PrepareHeader_ExecutesAllMiddlewares()
    {
        LogHeaderFactory factory = new(new[] { _middleware1.Object, _middleware2.Object });

        factory.PrepareHeader(new KeyValuePair<string, StringValues>("key", new StringValues("value")));

        _middleware1.Verify(middleware => middleware.Modify("key", It.IsAny<string>()), Times.Once);
        _middleware2.Verify(middleware => middleware.Modify("key", It.IsAny<string>()), Times.Once);
    }

    [Fact, Trait("Category", "Unit")]
    public void PrepareHeader_SecondMiddlewareGetsOutputFromFirstOne()
    {
        LogHeaderFactory factory = new(new[] { _middleware1.Object, _middleware2.Object });

        _middleware1
            .Setup(middleware => middleware.Modify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("modified");

        factory.PrepareHeader(new KeyValuePair<string, StringValues>("key", new StringValues("value")));

        _middleware1.Verify(middleware => middleware.Modify("key", "value"), Times.Once);
        _middleware2.Verify(middleware => middleware.Modify("key", "modified"), Times.Once);
    }
}