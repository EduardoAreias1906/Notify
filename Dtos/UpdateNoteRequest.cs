namespace Notify.Dtos;

// PUT substitui a nota inteira — todos os campos são obrigatórios (não é PATCH parcial)
// Tags são editáveis pelo utilizador; Title e Content substituem os anteriores
public class UpdateNoteRequest
{
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required List<string> Tags { get; set; }
}
