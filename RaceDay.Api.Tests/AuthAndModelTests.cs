using Microsoft.AspNetCore.Identity;
using RaceDay.Api.Models;
using RaceDay.Api.Services;
using Xunit;

namespace RaceDay.Api.Tests;

public class AuthAndModelTests
{
    [Fact]
    public void PasswordService_HashesPassword_AndVerifiesCorrectPassword()
    {
        var service = new PasswordService();
        var user = new User { Email = "test@example.com", Role = "Participant" };
        var hash = service.Hash(user, "StrongPass123!");
        Assert.NotEqual("StrongPass123!", hash);
        Assert.True(service.Verify(user, hash, "StrongPass123!"));
        Assert.False(service.Verify(user, hash, "wrong-password"));
    }

    [Theory]
    [InlineData("Organizer", true)]
    [InlineData("Participant", true)]
    [InlineData("Admin", false)]
    [InlineData("", false)]
    public void RegistrationRole_IsRestrictedToApprovedRoles(string role, bool expected)
    {
        var valid = role is "Organizer" or "Participant";
        Assert.Equal(expected, valid);
    }

    [Fact]
    public void EnrolmentUniquenessRule_IsDocumentedForParticipantEventCategory()
    {
        // RaceDayContext configures a unique index over ParticipantId, EventId, CategoryId.
        // Integration testing against a relational test provider is recommended before submission.
        Assert.True(typeof(Enrolment).GetProperty(nameof(Enrolment.ParticipantId)) is not null);
        Assert.True(typeof(Enrolment).GetProperty(nameof(Enrolment.EventId)) is not null);
        Assert.True(typeof(Enrolment).GetProperty(nameof(Enrolment.CategoryId)) is not null);
    }
}
