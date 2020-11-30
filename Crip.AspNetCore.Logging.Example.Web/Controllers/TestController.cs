using System;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace Crip.AspNetCore.Logging.Example.Web.Controllers
{
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok(new { shouldAppear = true });
        }

        [HttpGet]
        [Route("error")]
        public IActionResult Error()
        {
            throw new Exception("***error***");
        }

        [HttpGet]
        [Route("error/ctrl-filter-handled")]
        public IActionResult Error2()
        {
            throw new HttpResponseException
            {
                Status = 401,
                Value = new { test = 1 },
            };
        }

        [HttpGet]
        [Route("error/exception-handler-lambda")]
        public IActionResult Error3()
        {
            throw new ArgumentNullException("param name");
        }

        [HttpGet]
        [Route("silent")]
        public IActionResult Silent()
        {
            return Ok(new { shouldAppear = false });
        }

        [HttpGet]
        [Route("silent/next")]
        public IActionResult SilentNext()
        {
            return Ok(new { shouldAppear = true });
        }

        [HttpGet]
        [Route("silent-start")]
        public IActionResult SilentStar()
        {
            return Ok(new { shouldAppear = false });
        }

        [HttpGet]
        [Route("silent-start/next")]
        public IActionResult SilentStarNext()
        {
            return Ok(new { shouldAppear = false });
        }
    }
}