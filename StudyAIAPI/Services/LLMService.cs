using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text;
using System.Text.Encodings.Web;
using UglyToad.PdfPig;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;
using System.Text.Json;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
//using JsonSerializer = System.Text.Json.JsonSerializer;
namespace StudyAIAPI.Services
{
    public sealed class LLMService : IDisposable
    {
        private readonly IChatClient _chatClient;
        private const string SystemPrompt = "Eres un asistente de IA que resume documentos. " +
                                             "Genera un resumen conciso y claro de no más de 200 palabras. " +
                                             "Captura los puntos clave, las ideas principales y los detalles relevantes.";
        private const string ResumeTemplate = "Genera un resumen del siguiente contenido:\n\n{0}";
        public LLMService(string apiKey)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri("https://api.groq.com/openai/v1")
            };

            var chatClient = new ChatClient(
                "openai/gpt-oss-20b",
                new ApiKeyCredential(apiKey),
                options);

            _chatClient = chatClient.AsIChatClient();
        }

        public async Task<string> GetSummaryFromLLM(List<IFormFile> files)
        {
            ArgumentNullException.ThrowIfNull(files);

            //var summaries = new List<string>();
            var summaries = new Dictionary<string, string>();

            foreach (var file in files)
            {
                var text = ExtractText(file);
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException(
                        $"No se pudo extraer texto del PDF '{file.FileName}'"
                    );
                }

                var messages = new List<ChatMessage>
                {
                    new ChatMessage(ChatRole.System, SystemPrompt),
                    new ChatMessage(ChatRole.User, string.Format(ResumeTemplate, text))
                };

                
                var resume = await _chatClient.GetResponseAsync(messages);

                summaries.Add(file.FileName, resume.Text.Trim());
            }

            // Convert summaries to JSON string
            var jsonSummaries = JsonSerializer.Serialize(summaries, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            });

            return jsonSummaries;
        }

        private static string ExtractText(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var pdf = PdfDocument.Open(stream);

            var sb = new StringBuilder();
            foreach (var page in pdf.GetPages())
                sb.AppendLine(ContentOrderTextExtractor.GetText(page));

            return sb.ToString().Trim();
        }

        public void Dispose()
        {
            _chatClient.Dispose();
        }
    }
}
