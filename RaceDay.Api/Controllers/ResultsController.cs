using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api")]
public class ResultsController(RaceDayContext db) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Returns results belonging to the signed-in Participant.</summary>
    [HttpGet("results/me"), Authorize(Roles = "Participant")]
    public async Task<IActionResult> Mine() => Ok(await db.Results.Where(r => r.Enrolment!.ParticipantId == UserId)
        .Include(r => r.Enrolment).ThenInclude(e => e!.EventCategory).ThenInclude(ec => ec!.Event)
        .Select(r => new { r.ResultId, r.EnrolmentId, EventName = r.Enrolment!.EventCategory!.Event!.EventName, r.FinishPosition, r.FinishTime, r.ResultStatus, r.RecordedAt }).ToListAsync());

    /// <summary>Gets a result for an enrolment; Participants can see their own, Organizers can see their own event.</summary>
    [HttpGet("enrolments/{enrolmentId:int}/result"), Authorize]
    public async Task<IActionResult> Get(int enrolmentId)
    {
        var e = await db.Enrolments.Include(x => x.EventCategory).ThenInclude(x => x!.Event).Include(x => x.Result).FirstOrDefaultAsync(x => x.EnrolmentId == enrolmentId);
        if (e is null) return NotFound();
        if (User.IsInRole("Participant") && e.ParticipantId != UserId) return Forbid();
        if (User.IsInRole("Organizer") && e.EventCategory!.Event!.OrganizerId != UserId) return Forbid();
        return e.Result is null ? NotFound(new { message = "No result recorded yet." }) : Ok(e.Result);
    }

    /// <summary>Records a result. Only the Organizer who owns the event may do this.</summary>
    [HttpPost("enrolments/{enrolmentId:int}/result"), Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Create(int enrolmentId, ResultRequest request)
    {
        var e = await db.Enrolments.Include(x => x.EventCategory).ThenInclude(x => x!.Event).FirstOrDefaultAsync(x => x.EnrolmentId == enrolmentId);
        if (e is null) return NotFound();
        if (e.EventCategory!.Event!.OrganizerId != UserId) return Forbid();
        if (await db.Results.AnyAsync(r => r.EnrolmentId == enrolmentId)) return Conflict(new { message = "A result already exists for this enrolment." });
        if (!Valid(request)) return BadRequest(new { message = "Invalid result status or finishing position." });
        var result = new RaceResult { EnrolmentId = enrolmentId, FinishPosition = request.FinishPosition, FinishTime = request.FinishTime, ResultStatus = request.ResultStatus };
        db.Results.Add(result); await db.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { enrolmentId }, result);
    }

    /// <summary>Updates an existing result. Only the event's Organizer may do this.</summary>
    [HttpPut("enrolments/{enrolmentId:int}/result"), Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Update(int enrolmentId, ResultRequest request)
    {
        var e = await db.Enrolments.Include(x => x.EventCategory).ThenInclude(x => x!.Event).FirstOrDefaultAsync(x => x.EnrolmentId == enrolmentId);
        if (e is null) return NotFound();
        if (e.EventCategory!.Event!.OrganizerId != UserId) return Forbid();
        var r = await db.Results.SingleOrDefaultAsync(x => x.EnrolmentId == enrolmentId);
        if (r is null) return NotFound();
        if (!Valid(request)) return BadRequest(new { message = "Invalid result status or finishing position." });
        r.FinishPosition = request.FinishPosition; r.FinishTime = request.FinishTime; r.ResultStatus = request.ResultStatus; r.RecordedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(); return Ok(r);
    }

    private static bool Valid(ResultRequest r) =>
        (r.FinishPosition is null or > 0) &&
        (r.FinishTime is null || (r.FinishTime.Value >= TimeSpan.Zero && r.FinishTime.Value < TimeSpan.FromDays(1))) &&
        r.ResultStatus is "Pending" or "Finished" or "DNF" or "DNS";
}
