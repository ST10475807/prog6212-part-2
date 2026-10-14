using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayContext(DbContextOptions<RaceDayContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RaceEvent> Events => Set<RaceEvent>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<RaceResult> Results => Set<RaceResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(255).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(20);
        });

        modelBuilder.Entity<RaceEvent>(entity =>
        {
            entity.Property(x => x.EventName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.Venue).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Location).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(20).IsRequired();
            entity.HasOne(x => x.Organizer).WithMany(x => x.OrganizedEvents)
                .HasForeignKey(x => x.OrganizerId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Events_MaxParticipants", "[MaxParticipants] > 0");
                t.HasCheckConstraint("CK_Events_EventType", "[EventType] IN ('Run', 'Walk', 'Cycle')");
                t.HasCheckConstraint("CK_Events_Status", "[Status] IN ('Draft', 'Open', 'Closed', 'Cancelled', 'Completed')");
            });
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(x => x.CategoryName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(300);
            entity.HasIndex(x => x.CategoryName).IsUnique();
            entity.ToTable(t => t.HasCheckConstraint("CK_Categories_AgeRange", "[MinAge] >= 0 AND [MaxAge] >= [MinAge] AND [MaxAge] <= 120"));
        });

        modelBuilder.Entity<EventCategory>().HasKey(x => new { x.EventId, x.CategoryId });
        modelBuilder.Entity<EventCategory>().HasOne(x => x.Event).WithMany(x => x.EventCategories).HasForeignKey(x => x.EventId);
        modelBuilder.Entity<EventCategory>().HasOne(x => x.Category).WithMany(x => x.EventCategories).HasForeignKey(x => x.CategoryId);
        modelBuilder.Entity<EventCategory>().Property(x => x.EntryFee).HasPrecision(10, 2);

        modelBuilder.Entity<Enrolment>().HasOne(x => x.EventCategory).WithMany(x => x.Enrolments)
            .HasForeignKey(x => new { x.EventId, x.CategoryId });
        modelBuilder.Entity<Enrolment>().HasOne(x => x.Participant).WithMany(x => x.Enrolments)
            .HasForeignKey(x => x.ParticipantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Enrolment>().HasIndex(x => new { x.ParticipantId, x.EventId, x.CategoryId }).IsUnique();
        modelBuilder.Entity<RaceResult>().HasIndex(x => x.EnrolmentId).IsUnique();
        modelBuilder.Entity<RaceResult>().HasOne(x => x.Enrolment).WithOne(x => x.Result).HasForeignKey<RaceResult>(x => x.EnrolmentId);
        base.OnModelCreating(modelBuilder);
    }
}
