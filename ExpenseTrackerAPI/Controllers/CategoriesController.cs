using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExpenseTrackerAPI.Data;
using ExpenseTrackerAPI.DTOs;
using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ApplicationDbContext context, ILogger<CategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim!.Value);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        try
        {
            var userId = GetUserId();
            
            // Get system categories and user's custom categories
            var categories = await _context.Categories
                .Where(c => c.IsSystem || c.UserId == userId)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Icon = c.Icon,
                    Color = c.Color,
                    IsSystem = c.IsSystem
                })
                .ToListAsync();

            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        try
        {
            var userId = GetUserId();

            var category = new Category
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = dto.Name,
                Icon = dto.Icon,
                Color = dto.Color,
                IsSystem = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Icon = category.Icon,
                Color = category.Color,
                IsSystem = category.IsSystem
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        try
        {
            var userId = GetUserId();
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsSystem);

            if (category == null)
                return NotFound(new { message = "Category not found or cannot be modified" });

            if (!string.IsNullOrEmpty(dto.Name))
                category.Name = dto.Name;

            if (!string.IsNullOrEmpty(dto.Icon))
                category.Icon = dto.Icon;

            if (!string.IsNullOrEmpty(dto.Color))
                category.Color = dto.Color;

            await _context.SaveChangesAsync();

            return Ok(new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Icon = category.Icon,
                Color = category.Color,
                IsSystem = category.IsSystem
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId && !c.IsSystem);

            if (category == null)
                return NotFound(new { message = "Category not found or cannot be deleted" });

            // Check if category is used in any expenses or budgets
            var hasExpenses = await _context.Expenses.AnyAsync(e => e.CategoryId == id);
            var hasBudgets = await _context.Budgets.AnyAsync(b => b.CategoryId == id);

            if (hasExpenses || hasBudgets)
                return BadRequest(new { message = "Cannot delete category that is being used" });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
