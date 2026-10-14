using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api/users"), Authorize]
public class UsersController(RaceDayContext db) : ControllerBase
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Returns the authenticated user's own profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var u = await db.Users.FindAsync(CurrentUserId);
        return u is null ? NotFound() : Ok(AuthController.ToResponse(u));
    }

    /// <summary>Updates only the authenticated user's own profile details.</summary>
    [HttpPut("me")]
    public async Task<IActionResult> Update(UpdateProfileRequest request)
    {
        var u = await db.Users.FindAsync(CurrentUserId);
        if (u is null) return NotFound();
        u.FirstName = request.FirstName.Trim(); u.LastName = request.LastName.Trim(); u.Phone = request.Phone;
        await db.SaveChangesAsync();
        return Ok(AuthController.ToResponse(u));
    }
}
