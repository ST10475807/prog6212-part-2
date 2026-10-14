using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api")]
public class EnrolmentsController(RaceDayContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Enrols the authenticated Participant in the chosen event category.</summary>
    [HttpPost("events/{eventId:int}/categories/{categoryId:int}/enrolments"), Authorize(Roles = "Participant")]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        var link = await db.EventCategories.Include(x => x.Event).FirstOrDefaultAsync(x => x.EventId == eventId && x.CategoryId == categoryId);
        if (link is null) return NotFound(new { message = "Event category not found." });
        if (link.Event!.Status != "Open") return Conflict(new { message = "This event is not open for enrolment." });
        var existing = await db.Enrolments.FirstOrDefaultAsync(x => x.EventId == eventId && x.CategoryId == categoryId && x.ParticipantId == UserId);
        if (existing is not null && existing.Status != "Cancelled")
            return Conflict(new { message = "You are already enrolled in this category." });
        var categoryCount = await db.Enrolments.CountAsync(x => x.EventId == eventId && x.CategoryId == categoryId && x.Status == "Active");
        if (categoryCount >= link.MaxParticipants) return Conflict(new { message = "This category is full." });
        var eventCount = await db.Enrolments.CountAsync(x => x.EventId == eventId && x.Status == "Active");
        if (eventCount >= link.Event.MaxParticipants) return Conflict(new { message = "This event has reached its maximum participant capacity." });

        Enrolment enrolment;
        if (existing is not null)
        {
            existing.Status = "Active";
            existing.EnrolmentDate = DateTime.UtcNow;
            enrolment = existing;
        }
        else
        {
            enrolment = new Enrolment { EventId = eventId, CategoryId = categoryId, ParticipantId = UserId, Status = "Active" };
            db.Enrolments.Add(enrolment);
        }
        await db.SaveChangesAsync();
        return Created($"/api/enrolments/{enrolment.EnrolmentId}", new { enrolment.EnrolmentId, enrolment.EventId, enrolment.CategoryId, enrolment.ParticipantId, enrolment.Status, enrolment.EnrolmentDate });
    }

    /// <summary>Lists the authenticated Participant's enrolments.</summary>
    [HttpGet("enrolments/me"), Authorize(Roles = "Participant")]
    public async Task<IActionResult> Mine() => Ok(await db.Enrolments.Where(x => x.ParticipantId == UserId).Include(x => x.EventCategory).ThenInclude(x => x!.Event).Include(x => x.EventCategory).ThenInclude(x => x!.Category).Select(x => new { x.EnrolmentId, x.EventId, EventName = x.EventCategory!.Event!.EventName, x.CategoryId, CategoryName = x.EventCategory.Category!.CategoryName, x.Status, x.EnrolmentDate }).ToListAsync());

    /// <summary>Lists enrolments for an event owned by the authenticated Organizer.</summary>
    [HttpGet("events/{eventId:int}/enrolments"), Authorize(Roles = "Organizer")]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();
        if (ev.OrganizerId != UserId) return Forbid();
        return Ok(await db.Enrolments.Where(x => x.EventId == eventId).Include(x => x.Participant).Include(x => x.EventCategory).ThenInclude(x => x!.Category).Select(x => new { x.EnrolmentId, Participant = x.Participant!.FirstName + " " + x.Participant.LastName, x.Participant.Email, x.CategoryId, CategoryName = x.EventCategory!.Category!.CategoryName, x.Status, x.EnrolmentDate }).ToListAsync());
    }

    /// <summary>Cancels an enrolment owned by the authenticated Participant.</summary>
    [HttpDelete("enrolments/{id:int}"), Authorize(Roles = "Participant")]
    public async Task<IActionResult> Cancel(int id)
    {
        var e = await db.Enrolments.FindAsync(id);
        if (e is null) return NotFound();
        if (e.ParticipantId != UserId) return Forbid();
        if (e.Status == "Completed") return Conflict(new { message = "Completed enrolments cannot be cancelled." });
        e.Status = "Cancelled"; await db.SaveChangesAsync(); return NoContent();
    }
}
