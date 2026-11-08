using System.ComponentModel.DataAnnotations;

namespace StudyAIAPI.Models
{
    public class SummaryRequest
    {
        [Required]
        public List<IFormFile> Files { get; set; } = new();
    }
}
