using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crip.AspNetCore.Logging.Tests;

public class DependencyInjectionTests
{
    [Fact, Trait("Category", "Unit")]
    public void AddRequestLogging_RegistersRequiredServices()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        var provider = new ServiceCollection()
            .AddScoped<IConfiguration>(_ => configuration)
            .AddRequestLogging()
            .BuildServiceProvider();

        // Act and Assert
        provider.GetService<IMeasurable>().Should().NotBeNull();
        provider.GetService<IStopwatch>().Should().NotBeNull();
        provider.GetService<IContextLoggerFactory>().Should().NotBeNull();
        provider.GetService<IHttpLoggerFactory>().Should().NotBeNull();
        provider.GetService<IRequestLogger>().Should().NotBeNull();
        provider.GetService<IResponseLogger>().Should().NotBeNull();
        provider.GetService<IBasicInfoLogger>().Should().NotBeNull();
        provider.GetService<LogContentFactory>().Should().NotBeNull();
        provider.GetService<IEnumerable<IRequestContentLogMiddleware>>().Should().NotBeNull().And.HaveCount(1);
        provider.GetService<IJsonStreamModifier>().Should().NotBeNull();
        provider.GetService<LogHeaderFactory>().Should().NotBeNull();
        provider.GetService<IEnumerable<IHeaderLogMiddleware>>().Should().NotBeNull().And.HaveCount(1);
    }
}
