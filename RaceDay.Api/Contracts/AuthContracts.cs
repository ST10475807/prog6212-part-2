using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Contracts;

public record RegisterRequest(
    [Required, MaxLength(50)] string FirstName,
    [Required, MaxLength(50)] string LastName,
    [Required, EmailAddress, MaxLength(255)] string Email,
    [Required, MinLength(8)] string Password,
    [Required] string Role,
    [Phone, MaxLength(20)] string? Phone);

public record LoginRequest([Required, EmailAddress] string Email, [Required] string Password);
public record UpdateProfileRequest([Required, MaxLength(50)] string FirstName, [Required, MaxLength(50)] string LastName, [Phone, MaxLength(20)] string? Phone);
public record UserResponse(int UserId, string FirstName, string LastName, string Email, string Role, string? Phone);
public record LoginResponse(string AccessToken, DateTime ExpiresAtUtc, UserResponse Profile);
