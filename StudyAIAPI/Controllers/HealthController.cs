using Microsoft.AspNetCore.Mvc;

namespace StudyAIAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController: ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok("API is working");
        }


    }
}
