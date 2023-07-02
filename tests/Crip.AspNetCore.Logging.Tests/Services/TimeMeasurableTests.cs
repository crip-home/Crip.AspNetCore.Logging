using Microsoft.Extensions.DependencyInjection;

namespace Crip.AspNetCore.Logging.Tests.Services;

public class TimeMeasurableTests
{
    private readonly Mock<IStopwatch> _stopwatch = new();
    private readonly IServiceProvider _provider;
    private readonly IMeasurable _measurable;

    public TimeMeasurableTests()
    {
        _provider = new ServiceCollection()
            .AddSingleton(_stopwatch.Object)
            .BuildServiceProvider();

        _measurable = new TimeMeasurable(_provider);
    }

    [Fact, Trait("Category", "Unit")]
    public void StartMeasure_CreatesNewInstance()
    {
        var result = _measurable.StartMeasure();

        result
            .Should().NotBeNull()
            .And.NotBe(_measurable);
    }

    [Fact, Trait("Category", "Unit")]
    public void StopMeasure_ThrowsErrorIfNotStarted()
    {
        Action act = () => _measurable.StopMeasure();

        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("Could not stop not started time measurement. (Parameter '_stopwatch')");
    }

    [Fact, Trait("Category", "Unit")]
    public void StopMeasure_StopsStopwatch()
    {
        var measure = _measurable.StartMeasure();
        var result = measure.StopMeasure();

        _stopwatch.Verify(stopwatch => stopwatch.Stop(), Times.Once());
        result.Should().Be(_stopwatch.Object);
    }
}