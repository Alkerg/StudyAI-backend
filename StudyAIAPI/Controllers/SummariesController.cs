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
        private readonly ILogger<SummariesController> _logger;

        public SummariesController(LLMService llmService, ILogger<SummariesController> logger)
        {
            _llmService = llmService;
            _logger = logger;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
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



            try
            {
                var summary = await _llmService.GetSummaryFromLLM(request.Files);
                return Content(summary, "application/json; charset=utf-8");
            }
            catch (PdfWithoutTextException exception)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, new
                {
                    title = "PDF sin texto",
                    detail = exception.Message,
                    status = StatusCodes.Status422UnprocessableEntity
                });
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error al generar el resumen de los archivos enviados.");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    title = "No se pudo generar el resumen",
                    detail = "Ocurrió un error inesperado al procesar los archivos. Inténtalo nuevamente más tarde.",
                    status = StatusCodes.Status500InternalServerError
                });
            }
        }
    }
}
