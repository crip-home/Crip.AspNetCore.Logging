using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Crip.AspNetCore.Logging.Core31.Example.Controllers
{
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly NamedHttpClient _client;

        public TestController(NamedHttpClient client)
        {
            _client = client;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new { shouldAppear = true });
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
}