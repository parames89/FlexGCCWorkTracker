
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkRequest.Api.Data;
using WorkRequest.Api.DTOs;
using WorkRequest.Api.Models;

namespace WorkRequest.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkRequestsController : ControllerBase
{
    private readonly AppDbContext _context;

    public WorkRequestsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/WorkRequests
    // Example:
    // ?search=laptop&status=1&priority=2&pageNumber=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? clientName,
        [FromQuery] WorkRequestStatus? status,
        [FromQuery] WorkRequestPriority? priority,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "CreatedDate",
        [FromQuery] bool descending = true)
    {
        if (pageNumber < 1)
            return BadRequest("pageNumber must be at least 1.");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("pageSize must be between 1 and 100.");

        if (fromDate.HasValue && toDate.HasValue &&
            fromDate.Value.Date > toDate.Value.Date)
        {
            return BadRequest("fromDate cannot be after toDate.");
        }

        IQueryable<WorkRequestItem> query =
            _context.WorkRequests.AsNoTracking();

        // Search by title, client name or description.
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();

            query = query.Where(w =>
                w.Title.Contains(term) ||
                w.ClientName.Contains(term) ||
                w.Description.Contains(term));
        }

        // Filter by client name.
        if (!string.IsNullOrWhiteSpace(clientName))
        {
            var client = clientName.Trim();

            query = query.Where(w =>
                w.ClientName.Contains(client));
        }

        // Filter by status.
        if (status.HasValue)
            query = query.Where(w => w.Status == status.Value);

        // Filter by priority.
        if (priority.HasValue)
            query = query.Where(w => w.Priority == priority.Value);

        // Filter by creation date.
        if (fromDate.HasValue)
        {
            query = query.Where(w =>
                w.CreatedDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            var endExclusive = toDate.Value.Date.AddDays(1);

            query = query.Where(w =>
                w.CreatedDate < endExclusive);
        }

        // Count matching records before pagination.
        var totalCount = await query.CountAsync();

        // Sorting.
        IOrderedQueryable<WorkRequestItem> orderedQuery =
            sortBy.Trim().ToLowerInvariant() switch
            {
                "title" => descending
                    ? query.OrderByDescending(w => w.Title)
                    : query.OrderBy(w => w.Title),

                "clientname" => descending
                    ? query.OrderByDescending(w => w.ClientName)
                    : query.OrderBy(w => w.ClientName),

                "priority" => descending
                    ? query.OrderByDescending(w => w.Priority)
                    : query.OrderBy(w => w.Priority),

                "status" => descending
                    ? query.OrderByDescending(w => w.Status)
                    : query.OrderBy(w => w.Status),

                "duedate" => descending
                    ? query.OrderByDescending(w => w.DueDate)
                    : query.OrderBy(w => w.DueDate),

                "updateddate" => descending
                    ? query.OrderByDescending(w => w.UpdatedDate)
                    : query.OrderBy(w => w.UpdatedDate),

                _ => descending
                    ? query.OrderByDescending(w => w.CreatedDate)
                    : query.OrderBy(w => w.CreatedDate)
            };

        // Stable ordering for pagination.
        var records = await orderedQuery
     .Skip((pageNumber - 1) * pageSize)
     .Take(pageSize)
     .AsNoTracking()
     .ToListAsync();

        var items = records.Select(item => new WorkRequestResponseDto
        {
            Id = item.Id,
            Title = item.Title,
            ClientName = item.ClientName,
            Description = item.Description,
            Priority = item.Priority,
            Status = item.Status,
            DueDate = item.DueDate,
            CreatedDate = item.CreatedDate,
            UpdatedDate = item.UpdatedDate
        }).ToList();

        var totalPages =
            (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new
        {
            pageNumber,
            pageSize,
            totalCount,
            totalPages,
            data = items
        });
    }

    // GET: api/WorkRequests/1
    // GET: api/WorkRequests/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<WorkRequestResponseDto>> GetById(int id)
    {
        if (id <= 0)
            return BadRequest("ID must be greater than zero.");

        var item = await _context.WorkRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id);

        if (item == null)
            return NotFound();

        var response = new WorkRequestResponseDto
        {
            Id = item.Id,
            Title = item.Title,
            ClientName = item.ClientName,
            Description = item.Description,
            Priority = item.Priority,
            Status = item.Status,
            DueDate = item.DueDate,
            CreatedDate = item.CreatedDate,
            UpdatedDate = item.UpdatedDate
        };

        return Ok(response);
    }
    // POST: api/WorkRequests

    [HttpPost]
    public async Task<ActionResult<WorkRequestResponseDto>> Create(
        [FromBody] CreateWorkRequestDto dto)
    {
        var item = new WorkRequestItem
        {
            Title = dto.Title.Trim(),
            ClientName = dto.ClientName.Trim(),
            Description = dto.Description.Trim(),
            Priority = dto.Priority,
            Status = dto.Status,
            DueDate = dto.DueDate!.Value,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        _context.WorkRequests.Add(item);
        await _context.SaveChangesAsync();

        var response = new WorkRequestResponseDto
        {
            Id = item.Id,
            Title = item.Title,
            ClientName = item.ClientName,
            Description = item.Description,
            Priority = item.Priority,
            Status = item.Status,
            DueDate = item.DueDate,
            CreatedDate = item.CreatedDate,
            UpdatedDate = item.UpdatedDate
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.Id },
            response);
    }

    // PUT: api/WorkRequests/1

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateWorkRequestDto dto)
    {
        if (id <= 0)
            return BadRequest("ID must be greater than zero.");

        var existingItem = await _context.WorkRequests
            .FirstOrDefaultAsync(w => w.Id == id);

        if (existingItem == null)
            return NotFound();

        existingItem.Title = dto.Title.Trim();
        existingItem.ClientName = dto.ClientName.Trim();
        existingItem.Description = dto.Description.Trim();
        existingItem.Priority = dto.Priority;
        existingItem.Status = dto.Status;
        existingItem.DueDate = dto.DueDate!.Value;
        existingItem.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH: api/WorkRequests/1/status
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateWorkRequestStatusDto dto)
    {
        if (id <= 0)
            return BadRequest("ID must be greater than zero.");

        if (!Enum.IsDefined(typeof(WorkRequestStatus), dto.Status))
            return BadRequest("Invalid status value.");

        var item = await _context.WorkRequests
            .FirstOrDefaultAsync(w => w.Id == id);

        if (item == null)
            return NotFound("Work request not found.");

        item.Status = dto.Status;
        item.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/WorkRequests/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return BadRequest("ID must be greater than zero.");

        var item = await _context.WorkRequests
            .FirstOrDefaultAsync(w => w.Id == id);

        if (item == null)
            return NotFound();

        _context.WorkRequests.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Shared validation for POST and PUT.
    private static string? ValidateItem(WorkRequestItem item)
    {
        if (item == null)
            return "Request body is required.";

        if (string.IsNullOrWhiteSpace(item.Title))
            return "Title is required.";

        if (string.IsNullOrWhiteSpace(item.ClientName))
            return "ClientName is required.";

        if (string.IsNullOrWhiteSpace(item.Description))
            return "Description is required.";

        if (!Enum.IsDefined(typeof(WorkRequestPriority), item.Priority))
            return "Invalid priority value.";

        if (!Enum.IsDefined(typeof(WorkRequestStatus), item.Status))
            return "Invalid status value.";

        if (item.DueDate == default)
            return "A valid DueDate is required.";

        return null;
    }
}
