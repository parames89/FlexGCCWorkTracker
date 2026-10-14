
using System.ComponentModel.DataAnnotations;

namespace WorkRequest.Api.DTOs;

public class CreateWorkRequestNoteDto
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public string Content { get; set; } = string.Empty;
}
