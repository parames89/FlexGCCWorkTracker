
namespace WorkRequest.Api.DTOs;

public class WorkRequestNoteResponseDto
{
    public int Id { get; set; }

    public int WorkRequestId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
}
