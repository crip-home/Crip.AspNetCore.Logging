namespace Crip.AspNetCore.Logging.Tests.Models;

public class ResponseDetailsTests
{
    private readonly Mock<IStopwatch> _stopwatch = new();

    public ResponseDetailsTests()
    {
    }

    [Fact, Trait("Category", "Unit")]
    public void From_NullIfContentIsNull()
    {
        var result = ResponseDetails.From((HttpResponseMessage?)null, null);

        result.Should().NotBeNull().And.BeEquivalentTo(new ResponseDetails
        {
            Stopwatch = null,
            Content = null,
            Headers = null,
            Time = string.Empty,
            ContentType = null,
            StatusCode = null,
        });
    }

    [Fact, Trait("Category", "Unit")]
    public void From_SetsStopwatchInstanceAndGetsItsTime()
    {
        MockStopwatchSeconds(15.1);

        var result = ResponseDetails.From(
            (HttpResponseMessage?)null,
            _stopwatch.Object);

        result.Should().NotBeNull().And.BeEquivalentTo(new ResponseDetails
        {
            Stopwatch = _stopwatch.Object,
            Content = null,
            Headers = null,
            Time = "00:00:15.100",
            ContentType = null,
            StatusCode = null,
        });
    }

    private void MockStopwatchSeconds(double seconds) =>
        _stopwatch
            .SetupGet(stopwatch => stopwatch.Elapsed)
            .Returns(TimeSpan.FromSeconds(seconds));
}