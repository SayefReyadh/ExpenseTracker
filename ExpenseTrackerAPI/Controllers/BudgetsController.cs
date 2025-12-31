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
public class BudgetsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BudgetsController> _logger;

    public BudgetsController(ApplicationDbContext context, ILogger<BudgetsController> logger)
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
    public async Task<ActionResult<IEnumerable<BudgetDto>>> GetBudgets()
    {
        try
        {
            var userId = GetUserId();
            var budgets = await _context.Budgets
                .Include(b => b.Category)
                .Where(b => b.UserId == userId)
                .ToListAsync();

            var budgetDtos = new List<BudgetDto>();

            foreach (var budget in budgets)
            {
                var spent = await CalculateSpent(userId, budget);
                
                budgetDtos.Add(new BudgetDto
                {
                    Id = budget.Id,
                    CategoryId = budget.CategoryId,
                    CategoryName = budget.Category.Name,
                    Amount = budget.Amount,
                    Period = budget.Period,
                    StartDate = budget.StartDate,
                    EndDate = budget.EndDate,
                    Spent = spent,
                    Remaining = budget.Amount - spent,
                    PercentageUsed = budget.Amount > 0 ? (spent / budget.Amount) * 100 : 0
                });
            }

            return Ok(budgetDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budgets");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BudgetDto>> GetBudget(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var budget = await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
                return NotFound();

            var spent = await CalculateSpent(userId, budget);

            return Ok(new BudgetDto
            {
                Id = budget.Id,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category.Name,
                Amount = budget.Amount,
                Period = budget.Period,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate,
                Spent = spent,
                Remaining = budget.Amount - spent,
                PercentageUsed = budget.Amount > 0 ? (spent / budget.Amount) * 100 : 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budget");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<BudgetDto>> CreateBudget([FromBody] CreateBudgetDto dto)
    {
        try
        {
            var userId = GetUserId();

            // Verify category exists
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && 
                    (c.UserId == userId || c.IsSystem));

            if (category == null)
                return BadRequest(new { message = "Invalid category" });

            var budget = new Budget
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CategoryId = dto.CategoryId,
                Amount = dto.Amount,
                Period = dto.Period,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            var spent = await CalculateSpent(userId, budget);

            return CreatedAtAction(nameof(GetBudget), new { id = budget.Id }, new BudgetDto
            {
                Id = budget.Id,
                CategoryId = budget.CategoryId,
                CategoryName = category.Name,
                Amount = budget.Amount,
                Period = budget.Period,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate,
                Spent = spent,
                Remaining = budget.Amount - spent,
                PercentageUsed = budget.Amount > 0 ? (spent / budget.Amount) * 100 : 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating budget");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<BudgetDto>> UpdateBudget(Guid id, [FromBody] UpdateBudgetDto dto)
    {
        try
        {
            var userId = GetUserId();
            var budget = await _context.Budgets
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
                return NotFound();

            if (dto.Amount.HasValue)
                budget.Amount = dto.Amount.Value;

            if (dto.EndDate.HasValue)
                budget.EndDate = dto.EndDate.Value;

            budget.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var spent = await CalculateSpent(userId, budget);

            return Ok(new BudgetDto
            {
                Id = budget.Id,
                CategoryId = budget.CategoryId,
                CategoryName = budget.Category.Name,
                Amount = budget.Amount,
                Period = budget.Period,
                StartDate = budget.StartDate,
                EndDate = budget.EndDate,
                Spent = spent,
                Remaining = budget.Amount - spent,
                PercentageUsed = budget.Amount > 0 ? (spent / budget.Amount) * 100 : 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating budget");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBudget(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (budget == null)
                return NotFound();

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting budget");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    private async Task<decimal> CalculateSpent(Guid userId, Budget budget)
    {
        var endDate = budget.EndDate ?? DateTime.UtcNow;
        
        return await _context.Expenses
            .Where(e => e.UserId == userId && 
                        e.CategoryId == budget.CategoryId &&
                        e.Date >= budget.StartDate &&
                        e.Date <= endDate)
            .SumAsync(e => e.Amount);
    }
}
