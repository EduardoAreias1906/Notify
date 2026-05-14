namespace Notify.Services;

// Abstração do serviço de LLM — permite trocar o provider (Groq, OpenAI, etc.) sem tocar nos endpoints
public interface ILlmService
{
    // Recebe o título e conteúdo da nota e devolve uma lista de tags relevantes
    Task<List<string>> GenerateTagsAsync(string title, string content);

    // Recebe o título e conteúdo da nota e devolve um resumo em texto
    Task<string> GenerateSummaryAsync(string title, string content);
}
