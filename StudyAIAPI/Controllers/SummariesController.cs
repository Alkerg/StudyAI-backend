using Microsoft.AspNetCore.Mvc;
using StudyAIAPI.Models;
using StudyAIAPI.Services;

namespace StudyAIAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SummariesController : ControllerBase
    {
        private readonly LLMService _llmService;

        public SummariesController(LLMService llmService)
        {
            _llmService = llmService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> GetSummary([FromForm] SummaryRequest request)
        {
            if (request.Files == null || request.Files.Count == 0) 
            {
                return BadRequest("No files provided");
            }

            foreach(var file in request.Files)
            {
                if (!string.Equals(Path.GetExtension(file.FileName), ".pdf", StringComparison.OrdinalIgnoreCase)
                    || file.ContentType != "application/pdf")
                    return BadRequest($"'{file.FileName}' is not a PDF");
            }

            var summary = await _llmService.GetSummaryFromLLM(request.Files);

            return Content(summary, "application/json; charset=utf-8");
        }
    }
}
