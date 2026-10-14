
using System.ComponentModel.DataAnnotations;
using WorkRequest.Api.Models;

namespace WorkRequest.Api.DTOs;

public class UpdateWorkRequestDto
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 5)]
    public string Description { get; set; } = string.Empty;

    [EnumDataType(typeof(WorkRequestPriority))]
    public WorkRequestPriority Priority { get; set; }

    [EnumDataType(typeof(WorkRequestStatus))]
    public WorkRequestStatus Status { get; set; }

    [Required]
    public DateTime? DueDate { get; set; }
}
