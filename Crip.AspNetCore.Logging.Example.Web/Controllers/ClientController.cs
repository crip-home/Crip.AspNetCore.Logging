using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Crip.AspNetCore.Logging.Example.Web.Controllers
{
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly NamedHttpClient _client;

        public ClientController(NamedHttpClient client)
        {
            _client = client;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _client.Post();
            return Ok(result);
        }
    }
}