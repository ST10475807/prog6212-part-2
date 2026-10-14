using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(RaceDayContext db, IPasswordService passwords, ITokenService tokens) : ControllerBase
{
    /// <summary>Registers an Organizer or Participant. Passwords are securely hashed before storage.</summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var role = request.Role.Trim();
        if (role is not ("Organizer" or "Participant")) return BadRequest(new { message = "Role must be Organizer or Participant." });
        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Users.AnyAsync(u => u.Email == email)) return Conflict(new { message = "Email already exists." });
        var user = new User { FirstName = request.FirstName.Trim(), LastName = request.LastName.Trim(), Email = email, Role = role, Phone = request.Phone };
        user.PasswordHash = passwords.Hash(user, request.Password);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return StatusCode(StatusCodes.Status201Created, ToResponse(user));
    }

    /// <summary>Authenticates credentials and returns a short-lived JWT access token for subsequent requests.</summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null || !passwords.Verify(user, user.PasswordHash, request.Password))
            return Unauthorized(new { message = "Invalid email or password." });
        var (token, expires) = tokens.Create(user);
        return Ok(new LoginResponse(token, expires, ToResponse(user)));
    }

    public static UserResponse ToResponse(User u) => new(u.UserId, u.FirstName, u.LastName, u.Email, u.Role, u.Phone);
}
