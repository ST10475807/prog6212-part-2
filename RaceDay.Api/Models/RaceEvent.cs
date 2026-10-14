namespace RaceDay.Api.Models;

public class RaceEvent
{
    public int EventId { get; set; }
    public int OrganizerId { get; set; }
    public User? Organizer { get; set; }
    public string EventName { get; set; } = "";
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public string Venue { get; set; } = "";
    public string Location { get; set; } = "";
    public int MaxParticipants { get; set; } = 100;
    public string Status { get; set; } = "Open";
    public string EventType { get; set; } = "Run";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
}
