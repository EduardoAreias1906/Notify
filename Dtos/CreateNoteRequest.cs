namespace Notify.Dtos;

// O cliente só fornece Title e Content — o servidor é responsável pelo resto (Id, datas, Tags, Summary)
public class CreateNoteRequest
{
    public required string Title { get; set; }
    public required string Content { get; set; }
}