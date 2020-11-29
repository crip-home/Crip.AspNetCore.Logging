using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace Crip.AspNetCore.Logging.Example.Web.Controllers
{
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/[controller]")]
    public class TestDebugController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new
            {
                request = true,
                headers = true,
                body = false,
            });
        }
    }
}