using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api/events")]
public class EventsController(RaceDayContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Lists public events; optional filters can be added by clients.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? status, [FromQuery] string? eventType,
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        if (fromDate.HasValue && toDate.HasValue && fromDate.Value.Date > toDate.Value.Date)
            return BadRequest(new { message = "fromDate must be on or before toDate." });

        var query = db.Events.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(e => e.Status == status.Trim());
        if (!string.IsNullOrWhiteSpace(eventType)) query = query.Where(e => e.EventType == eventType.Trim());
        if (fromDate.HasValue) query = query.Where(e => e.EventDate >= fromDate.Value.Date);
        if (toDate.HasValue) query = query.Where(e => e.EventDate < toDate.Value.Date.AddDays(1));

        return Ok(await query.OrderBy(e => e.EventDate).ThenBy(e => e.StartTime).Select(e => new {
            e.EventId, e.EventName, e.Description, e.EventDate, e.StartTime, e.Venue, e.Location,
            e.MaxParticipants, e.Status, e.EventType, e.OrganizerId
        }).ToListAsync());
    }

    /// <summary>Returns one event and its available categories.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var e = await db.Events.Include(x => x.EventCategories).ThenInclude(x => x.Category).AsNoTracking().FirstOrDefaultAsync(x => x.EventId == id);
        return e is null ? NotFound() : Ok(new { e.EventId, e.EventName, e.Description, e.EventDate, e.StartTime, e.Venue, e.Location, e.MaxParticipants, e.Status, e.EventType, Categories = e.EventCategories.Select(ec => new { ec.CategoryId, ec.Category!.CategoryName, ec.Category.Description, ec.Category.MinAge, ec.Category.MaxAge, ec.Category.DistanceKm, ec.MaxParticipants, ec.EntryFee }) });
    }

    /// <summary>Creates an event owned by the signed-in Organizer.</summary>
    [HttpPost, Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Create(EventRequest request)
    {
        if (!ValidEvent(request, out var error)) return BadRequest(new { message = error });
        var e = new RaceEvent { OrganizerId = UserId, EventName = request.EventName.Trim(), Description = request.Description, EventDate = request.EventDate.Date, StartTime = request.StartTime, Venue = request.Venue.Trim(), Location = request.Location.Trim(), MaxParticipants = request.MaxParticipants, Status = request.Status, EventType = request.EventType };
        db.Events.Add(e); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = e.EventId }, e);
    }

    /// <summary>Updates an event only when it belongs to the signed-in Organizer.</summary>
    [HttpPut("{id:int}"), Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Update(int id, EventRequest request)
    {
        if (!ValidEvent(request, out var error)) return BadRequest(new { message = error });
        var e = await db.Events.FindAsync(id);
        if (e is null) return NotFound();
        if (e.OrganizerId != UserId) return Forbid();
        var activeEnrolments = await db.Enrolments.CountAsync(x => x.EventId == id && x.Status == "Active");
        if (request.MaxParticipants < activeEnrolments)
            return Conflict(new { message = $"MaxParticipants cannot be less than the {activeEnrolments} active enrolments already recorded." });
        e.EventName = request.EventName.Trim(); e.Description = request.Description; e.EventDate = request.EventDate.Date;
        e.StartTime = request.StartTime; e.Venue = request.Venue.Trim(); e.Location = request.Location.Trim();
        e.MaxParticipants = request.MaxParticipants; e.Status = request.Status; e.EventType = request.EventType;
        await db.SaveChangesAsync(); return Ok(e);
    }

    /// <summary>Deletes an event only if it belongs to the Organizer and has no enrolments.</summary>
    [HttpDelete("{id:int}"), Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Delete(int id)
    {
        var e = await db.Events.FindAsync(id);
        if (e is null) return NotFound();
        if (e.OrganizerId != UserId) return Forbid();
        if (await db.Enrolments.AnyAsync(x => x.EventId == id)) return Conflict(new { message = "Event has enrolments and cannot be deleted." });
        db.Events.Remove(e); await db.SaveChangesAsync(); return NoContent();
    }

    private static bool ValidEvent(EventRequest r, out string? error)
    {
        if (r.EventDate == default) { error = "A valid event date is required."; return false; }
        if (r.StartTime < TimeSpan.Zero || r.StartTime >= TimeSpan.FromDays(1)) { error = "StartTime must be a valid time of day."; return false; }
        if (string.IsNullOrWhiteSpace(r.EventName) || string.IsNullOrWhiteSpace(r.Venue) || string.IsNullOrWhiteSpace(r.Location)) { error = "Event name, venue, and location are required."; return false; }
        if (r.MaxParticipants < 1) { error = "MaxParticipants must be at least 1."; return false; }
        if (r.Status is not ("Draft" or "Open" or "Closed" or "Cancelled" or "Completed")) { error = "Status must be Draft, Open, Closed, Cancelled, or Completed."; return false; }
        if (r.EventType is not ("Run" or "Walk" or "Cycle")) { error = "EventType must be Run, Walk, or Cycle."; return false; }
        error = null; return true;
    }
}
