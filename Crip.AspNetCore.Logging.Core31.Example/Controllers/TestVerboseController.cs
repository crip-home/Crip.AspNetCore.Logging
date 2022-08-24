using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace Crip.AspNetCore.Logging.Core31.Example.Controllers
{
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TestVerboseController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new
            {
                request = true,
                headers = true,
                body = true,
            });
        }
        
        [HttpPost]
        public IActionResult Post()
        {
            return Ok(new
            {
                request = true,
                headers = true,
                body = true,
            });
        }
    }
}