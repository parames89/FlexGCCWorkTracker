
using WorkRequest.Api.Models;

namespace WorkRequest.Api.DTOs;

public class WorkRequestResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkRequestPriority Priority { get; set; }
    public WorkRequestStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}
