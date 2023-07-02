using Crip.AspNetCore.Logging.LongJsonContent.Configuration;
using Crip.AspNetCore.Logging.LongJsonContent.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Crip.AspNetCore.Logging.LongJsonContent.Tests;

public class DependencyInjectionShould
{
    [Fact, Trait("Category", "Unit")]
    public void AddRequestLoggingLongJson_RegistersRequiredServices()
    {
        var provider = new ServiceCollection()
            .AddOptions()
            .AddRequestLoggingLongJson()
            .BuildServiceProvider();

        provider.GetService<IEnumerable<IRequestContentLogMiddleware>>().Should().NotBeNull().And.HaveCount(1);
        provider.GetService<IJsonStreamModifier>().Should().NotBeNull();
        provider.GetService<IOptions<LongJsonContentOptions>>().Should().NotBeNull();
    }
}