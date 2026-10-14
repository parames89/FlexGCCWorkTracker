
using System.ComponentModel.DataAnnotations;

namespace WorkRequest.Api.Models;

public class WorkRequestItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 3,
        ErrorMessage = "Title must be between 3 and 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Client name is required.")]
    [StringLength(150,
        ErrorMessage = "Client name cannot exceed 150 characters.")]
    public string ClientName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000,
        ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string Description { get; set; } = string.Empty;

    [EnumDataType(typeof(WorkRequestPriority))]
    public WorkRequestPriority Priority { get; set; }
        = WorkRequestPriority.Medium;

    [EnumDataType(typeof(WorkRequestStatus))]
    public WorkRequestStatus Status { get; set; }
        = WorkRequestStatus.New;

    [Required(ErrorMessage = "Due date is required.")]
    public DateTime DueDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public ICollection<WorkRequestNote> Notes { get; set; }
        = new List<WorkRequestNote>();
}
