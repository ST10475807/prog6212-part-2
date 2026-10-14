namespace RaceDay.Api.Models;

public class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = "";
    public string? Description { get; set; }
    public int MinAge { get; set; }
    public int MaxAge { get; set; } = 120;
    public decimal? DistanceKm { get; set; }
    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
}
