using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace Crip.AspNetCore.Logging.Core31.Example.Controllers
{
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/[controller]")]
    public class TestInfoController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new
            {
                request = true,
                headers = false,
                body = false,
            });
        }
    }
}