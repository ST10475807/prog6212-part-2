using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController, Route("api/categories")]
public class CategoriesController(RaceDayContext db) : ControllerBase
{
    /// <summary>Lists all race categories.</summary>
    [HttpGet]
    public async Task<IActionResult> List() => Ok(await db.Categories.AsNoTracking().ToListAsync());

    /// <summary>Returns one category.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var c = await db.Categories.FindAsync(id);
        return c is null ? NotFound() : Ok(c);
    }

    /// <summary>Creates a category. Organizer-only.</summary>
    [HttpPost, Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Create(CategoryRequest request)
    {
        if (!Valid(request, out var validationMessage)) return BadRequest(new { message = validationMessage });
        var categoryName = request.CategoryName.Trim();
        if (await db.Categories.AnyAsync(c => c.CategoryName.ToLower() == categoryName.ToLower())) return Conflict(new { message = "Category name already exists." });
        var c = new Category { CategoryName = categoryName, Description = request.Description?.Trim(), MinAge = request.MinAge, MaxAge = request.MaxAge, DistanceKm = request.DistanceKm };
        db.Categories.Add(c); await db.SaveChangesAsync(); return CreatedAtAction(nameof(Get), new { id = c.CategoryId }, c);
    }

    /// <summary>Updates a category. Organizer-only.</summary>
    [HttpPut("{id:int}"), Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Update(int id, CategoryRequest request)
    {
        var c = await db.Categories.FindAsync(id);
        if (c is null) return NotFound();
        if (!Valid(request, out var validationMessage)) return BadRequest(new { message = validationMessage });
        var categoryName = request.CategoryName.Trim();
        if (await db.Categories.AnyAsync(x => x.CategoryId != id && x.CategoryName.ToLower() == categoryName.ToLower())) return Conflict(new { message = "Category name already exists." });
        c.CategoryName = categoryName; c.Description = request.Description?.Trim(); c.MinAge = request.MinAge; c.MaxAge = request.MaxAge; c.DistanceKm = request.DistanceKm;
        await db.SaveChangesAsync(); return Ok(c);
    }

    /// <summary>Deletes an unused category. Organizer-only.</summary>
    [HttpDelete("{id:int}"), Authorize(Roles = "Organizer")]
    public async Task<IActionResult> Delete(int id)
    {
        var c = await db.Categories.FindAsync(id);
        if (c is null) return NotFound();
        if (await db.EventCategories.AnyAsync(x => x.CategoryId == id)) return Conflict(new { message = "Category is assigned to an event." });
        db.Categories.Remove(c); await db.SaveChangesAsync(); return NoContent();
    }
    private static bool Valid(CategoryRequest request, out string? message)
    {
        if (string.IsNullOrWhiteSpace(request.CategoryName)) { message = "CategoryName is required."; return false; }
        if (request.MinAge < 0 || request.MaxAge < request.MinAge || request.MaxAge > 120) { message = "Age range must be between 0 and 120, and MaxAge cannot be less than MinAge."; return false; }
        if (request.DistanceKm.HasValue && request.DistanceKm.Value <= 0) { message = "DistanceKm must be greater than zero when supplied."; return false; }
        message = null; return true;
    }
}
