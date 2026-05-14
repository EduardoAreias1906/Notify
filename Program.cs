using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Notify.Data;
using Notify.Dtos;
using Notify.Models;
using Notify.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Registo de serviços ---
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Regista o AppDbContext com SQLite — injetado automaticamente nos endpoints
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=notes.db"));

// Regista o GroqLlmService como implementação de ILlmService
// AddHttpClient configura um HttpClient dedicado com a base URL e a chave da API
// A chave é lida dos user-secrets (nunca do código ou de ficheiros versionados)
builder.Services.AddHttpClient<ILlmService, GroqLlmService>(client =>
{
    client.BaseAddress = new Uri("https://api.groq.com/openai/v1/");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
        "Bearer", builder.Configuration["Groq:ApiKey"]
    );
});

var app = builder.Build();

// --- Pipeline HTTP ---
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// --- Endpoints ---

// Cria uma nota — as tags são geradas automaticamente pelo LLM
app.MapPost("/notes", async (CreateNoteRequest request, AppDbContext db, ILlmService llm) =>
{
    // Chama o LLM antes de criar a nota para gerar as tags a partir do título e conteúdo
    var tags = await llm.GenerateTagsAsync(request.Title, request.Content);

    var note = new Note
    {
        Title = request.Title,
        Content = request.Content,
        Tags = tags,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.Notes.Add(note);
    await db.SaveChangesAsync();

    return Results.Created($"/notes/{note.Id}", note);
});

// Devolve todas as notas
app.MapGet("/notes", async (AppDbContext db) =>
    await db.Notes.ToListAsync());

// Devolve uma nota pelo id, ou 404
app.MapGet("/notes/{id}", async (int id, AppDbContext db) =>
    await db.Notes.FindAsync(id) is Note note
        ? Results.Ok(note)
        : Results.NotFound());

// Substitui todos os campos de uma nota existente
app.MapPut("/notes/{id}", async (int id, UpdateNoteRequest request, AppDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();

    note.Title = request.Title;
    note.Content = request.Content;
    note.Tags = request.Tags;
    note.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(note);
});

// Gera e guarda um summary para a nota via LLM — sob demanda, não automático
app.MapPost("/notes/{id}/summary", async (int id, AppDbContext db, ILlmService llm) =>
{
    var note = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();

    // Gera o summary com o LLM e guarda-o na nota — sobrescreve se já existia
    note.Summary = await llm.GenerateSummaryAsync(note.Title, note.Content);
    note.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(note);
});

// Remove uma nota, ou 404
app.MapDelete("/notes/{id}", async (int id, AppDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();

    db.Notes.Remove(note);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
