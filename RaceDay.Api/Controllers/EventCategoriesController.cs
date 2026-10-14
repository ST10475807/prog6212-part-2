using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api/events/{eventId:int}/categories")]
public class EventCategoriesController(RaceDayContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Lists categories assigned to an event.</summary>
    [HttpGet]
    public async Task<IActionResult> List(int eventId)
    {
        if (!await db.Events.AnyAsync(e => e.EventId == eventId)) return NotFound();
        return Ok(await db.EventCategories.Where(x => x.EventId == eventId).Include(x => x.Category).Select(x => new { x.EventId, x.CategoryId, x.Category!.CategoryName, x.Category.Description, x.Category.MinAge, x.Category.MaxAge, x.Category.DistanceKm, x.MaxParticipants, x.EntryFee }).ToListAsync());
    }

    /// <summary>Assigns a category to an event. Only the event's Organizer can do this.</summary>
    [HttpPost("{categoryId:int}"), Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Assign(int eventId, int categoryId, AssignCategoryRequest request)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });
        if (ev.OrganizerId != UserId) return Forbid();
        if (!await db.Categories.AnyAsync(c => c.CategoryId == categoryId)) return NotFound(new { message = "Category not found." });
        if (await db.EventCategories.AnyAsync(x => x.EventId == eventId && x.CategoryId == categoryId)) return Conflict(new { message = "Category already assigned." });
        var link = new EventCategory { EventId = eventId, CategoryId = categoryId, MaxParticipants = request.MaxParticipants, EntryFee = request.EntryFee };
        db.EventCategories.Add(link); await db.SaveChangesAsync();
        return CreatedAtAction(nameof(List), new { eventId }, link);
    }

    /// <summary>Removes a category from an event when no enrolments use it.</summary>
    [HttpDelete("{categoryId:int}"), Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Remove(int eventId, int categoryId)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();
        if (ev.OrganizerId != UserId) return Forbid();
        var link = await db.EventCategories.FindAsync(eventId, categoryId);
        if (link is null) return NotFound();
        if (await db.Enrolments.AnyAsync(x => x.EventId == eventId && x.CategoryId == categoryId)) return Conflict(new { message = "Category has enrolments." });
        db.EventCategories.Remove(link); await db.SaveChangesAsync(); return NoContent();
    }
}
