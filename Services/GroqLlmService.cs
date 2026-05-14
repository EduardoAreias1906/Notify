using System.Net.Http.Json;

namespace Notify.Services;

public class GroqLlmService : ILlmService
{
    private readonly HttpClient _http;

    // O modelo Groq que vamos usar — Llama 3.3 70B é o mais capaz disponível no tier gratuito
    private const string Model = "llama-3.3-70b-versatile";

    // O HttpClient é injetado pelo sistema de DI (configurado no Program.cs com a base URL e a chave)
    public GroqLlmService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<string>> GenerateTagsAsync(string title, string content)
    {
        // Pedimos ao LLM que devolva apenas as tags separadas por vírgula, sem texto extra
        var response = await SendRequestAsync(
            systemPrompt: "You are a tagging assistant. Given a note's title and content, return 3 to 5 relevant tags as a comma-separated list. Return only the tags, nothing else. Always respond in European Portuguese (Portugal). Example: produtividade, trabalho, reunião",
            userMessage: $"Title: {title}\nContent: {content}"
        );

        // Dividimos a string "tag1, tag2, tag3" numa lista e normalizamos para lowercase sem espaços
        return response
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim().ToLower())
            .ToList();
    }

    public async Task<string> GenerateSummaryAsync(string title, string content)
    {
        return await SendRequestAsync(
            systemPrompt: "You are a summarization assistant. Given a note's title and content, write a single short sentence that captures the core idea. Be brief and direct. Return only that sentence, nothing else. Always respond in European Portuguese (Portugal).",
            userMessage: $"Title: {title}\nContent: {content}"
        );
    }

    // Método privado partilhado pelos dois métodos públicos — evita repetir a lógica HTTP
    private async Task<string> SendRequestAsync(string systemPrompt, string userMessage)
    {
        // A API do Groq segue o formato OpenAI: enviamos "messages" com roles "system" e "user"
        // "system" define o comportamento do LLM; "user" é a mensagem concreta
        var requestBody = new
        {
            model = Model,
            messages = new[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userMessage }
            },
            // temperature controla a criatividade: 0 = determinístico, 1 = mais criativo
            temperature = 0.3
        };

        // PostAsJsonAsync serializa o objeto para JSON e envia o POST
        var response = await _http.PostAsJsonAsync("chat/completions", requestBody);

        // Lança exceção se a API devolver erro (ex: chave inválida, rate limit)
        response.EnsureSuccessStatusCode();

        // Deserializamos apenas os campos que nos interessam da resposta
        var json = await response.Content.ReadFromJsonAsync<GroqResponse>();

        // A resposta vem em choices[0].message.content — formato padrão OpenAI
        return json!.Choices[0].Message.Content;
    }

    // Records privados para mapear a resposta JSON da API
    // A API devolve uma lista de "choices"; usamos sempre o primeiro (índice 0)
    private record GroqResponse(List<Choice> Choices);
    private record Choice(Message Message);
    private record Message(string Content);
}
