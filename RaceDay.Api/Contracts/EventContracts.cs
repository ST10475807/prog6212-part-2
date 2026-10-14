using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Contracts;

public record EventRequest(
    [Required, StringLength(150, MinimumLength = 2)] string EventName,
    [MaxLength(500)] string? Description,
    DateTime EventDate,
    TimeSpan StartTime,
    [Required, StringLength(200, MinimumLength = 2)] string Venue,
    [Required, StringLength(200, MinimumLength = 2)] string Location,
    [Range(1, 100000)] int MaxParticipants,
    [Required] string Status,
    [Required] string EventType);

public record CategoryRequest(
    [Required, StringLength(100, MinimumLength = 2)] string CategoryName,
    [MaxLength(300)] string? Description,
    [Range(0, 120)] int MinAge = 0,
    [Range(0, 120)] int MaxAge = 120,
    [Range(typeof(decimal), "0.01", "10000")] decimal? DistanceKm = null);

public record AssignCategoryRequest(
    [Range(1, 100000)] int MaxParticipants,
    [Range(typeof(decimal), "0", "1000000")] decimal EntryFee);

public record ResultRequest(int? FinishPosition, TimeSpan? FinishTime, string ResultStatus);
