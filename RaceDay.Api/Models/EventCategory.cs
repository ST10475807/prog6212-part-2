namespace RaceDay.Api.Models;

public class EventCategory
{
    public int EventId { get; set; }
    public RaceEvent? Event { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int MaxParticipants { get; set; } = 100;
    public decimal EntryFee { get; set; }
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}
