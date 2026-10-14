
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkRequest.Api.Data;
using WorkRequest.Api.DTOs;
using WorkRequest.Api.Models;

namespace WorkRequest.Api.Controllers;

[ApiController]
[Route("api/WorkRequests/{workRequestId:int}/Notes")]
public class WorkRequestNotesController : ControllerBase
{
    private readonly AppDbContext _context;

    public WorkRequestNotesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/WorkRequests/2/Notes
    [HttpGet]
    public async Task<ActionResult<List<WorkRequestNoteResponseDto>>> GetNotes(
        int workRequestId)
    {
        if (workRequestId <= 0)
            return BadRequest("Work request ID must be greater than zero.");

        var exists = await _context.WorkRequests
            .AnyAsync(w => w.Id == workRequestId);

        if (!exists)
            return NotFound("Work request not found.");

        var notes = await _context.WorkRequestNotes
            .AsNoTracking()
            .Where(n => n.WorkRequestId == workRequestId)
            .OrderByDescending(n => n.CreatedDate)
            .Select(n => new WorkRequestNoteResponseDto
            {
                Id = n.Id,
                WorkRequestId = n.WorkRequestId,
                Content = n.Content,
                CreatedDate = n.CreatedDate
            })
            .ToListAsync();

        return Ok(notes);
    }

    // POST: api/WorkRequests/2/Notes
    [HttpPost]
    public async Task<ActionResult<WorkRequestNoteResponseDto>> AddNote(
        int workRequestId,
        [FromBody] CreateWorkRequestNoteDto dto)
    {
        if (workRequestId <= 0)
            return BadRequest("Work request ID must be greater than zero.");

        var workRequestExists = await _context.WorkRequests
            .AnyAsync(w => w.Id == workRequestId);

        if (!workRequestExists)
            return NotFound("Work request not found.");

        var note = new WorkRequestNote
        {
            WorkRequestId = workRequestId,
            Content = dto.Content.Trim(),
            CreatedDate = DateTime.UtcNow
        };

        _context.WorkRequestNotes.Add(note);
        await _context.SaveChangesAsync();

        var response = new WorkRequestNoteResponseDto
        {
            Id = note.Id,
            WorkRequestId = note.WorkRequestId,
            Content = note.Content,
            CreatedDate = note.CreatedDate
        };

        return CreatedAtAction(
            nameof(GetNotes),
            new { workRequestId },
            response);
    }
}
