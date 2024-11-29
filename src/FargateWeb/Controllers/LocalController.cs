using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FargateWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocalController : ControllerBase
    {
        [HttpGet]
        [Route("health")]
        public IActionResult Get()
        {
            return Ok("Strong like bull!");
        }
    }
}
