
using System.ComponentModel.DataAnnotations;
using WorkRequest.Api.Models;

namespace WorkRequest.Api.DTOs;

public class UpdateWorkRequestStatusDto
{
    [EnumDataType(typeof(WorkRequestStatus))]
    public WorkRequestStatus Status { get; set; }
}
