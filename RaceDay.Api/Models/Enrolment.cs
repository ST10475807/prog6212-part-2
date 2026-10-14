namespace RaceDay.Api.Models;

public class Enrolment
{
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public EventCategory? EventCategory { get; set; }
    public int ParticipantId { get; set; }
    public User? Participant { get; set; }
    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Active";
    public RaceResult? Result { get; set; }
}
