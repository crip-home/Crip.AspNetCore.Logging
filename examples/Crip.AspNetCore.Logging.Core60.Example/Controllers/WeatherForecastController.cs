using Microsoft.AspNetCore.Mvc;

namespace Crip.AspNetCore.Logging.Core60.Example.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries =
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly MyTypedClient _client;

    public WeatherForecastController(MyTypedClient client)
    {
        _client = client;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }

    [HttpGet(template: "client")]
    public async Task<IActionResult> Client()
    {
        var result = await _client.Post();
        return Ok(result);
    }

    [HttpGet(template: "error")]
    public IActionResult Error()
    {
        throw new Exception("***error***");
    }

    [HttpGet(template: "error-with-action-filter-handled")]
    public IActionResult Error2()
    {
        throw new HttpResponseException
        {
            Status = 401,
            Value = new { test = 1 },
        };
    }

    [HttpGet(template: "pattern-exclude")]
    public IActionResult PatternExclude()
    {
        return Ok(new { shouldLog = false });
    }

    [HttpGet(template: "pattern-exclude/sub")]
    public IActionResult PatternExcludeSub()
    {
        return Ok(new { shouldLog = false });
    }

    [HttpGet(template: "PATTERN-EXCLUDE/case-insensitive")]
    public IActionResult PatternExcludeCaseInsensitive()
    {
        return Ok(new { shouldLog = false });
    }

    [HttpGet(template: "exact-exclude")]
    public IActionResult ExactExclude()
    {
        return Ok(new { shouldLog = false });
    }

    [HttpGet(template: "exact-exclude/sub")]
    public IActionResult ExactExcludeSub()
    {
        return Ok(new { shouldLog = true });
    }

    [HttpPost(template: "verbose")]
    public IActionResult Verbose()
    {
        return Ok(new { Level = "Verbose" });
    }

    [HttpPost(template: "debug")]
    public IActionResult Debug()
    {
        return Ok(new { Level = "Debug" });
    }

    [HttpPost(template: "info")]
    public IActionResult Info()
    {
        return Ok(new { Level = "Info" });
    }

    [HttpPost(template: "warning")]
    public IActionResult Warning()
    {
        return Ok(new { Level = "Warning" });
    }
}