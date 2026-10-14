
using System.ComponentModel.DataAnnotations;

namespace WorkRequest.Api.Models;

public class WorkRequestNote
{
    public int Id { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "WorkRequestId must be greater than zero.")]
    public int WorkRequestId { get; set; }

    [Required(ErrorMessage = "Note content is required.")]
    [StringLength(1000, MinimumLength = 1,
        ErrorMessage = "Note content cannot exceed 1000 characters.")]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public WorkRequestItem WorkRequestItem { get; set; } = null!;
}
