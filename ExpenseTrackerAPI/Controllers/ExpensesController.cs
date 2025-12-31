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
public class ExpensesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExpensesController> _logger;

    public ExpensesController(ApplicationDbContext context, ILogger<ExpensesController> logger)
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
    public async Task<ActionResult<IEnumerable<ExpenseDto>>> GetExpenses(
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] Guid? categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            var userId = GetUserId();
            var query = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(e => e.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.Date <= endDate.Value);

            if (categoryId.HasValue)
                query = query.Where(e => e.CategoryId == categoryId.Value);

            var expenses = await query
                .OrderByDescending(e => e.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Amount = e.Amount,
                    Currency = e.Currency,
                    Description = e.Description,
                    CategoryId = e.CategoryId,
                    CategoryName = e.Category.Name,
                    CategoryIcon = e.Category.Icon,
                    CategoryColor = e.Category.Color,
                    Date = e.Date,
                    ReceiptUrl = e.ReceiptUrl,
                    Tags = e.Tags,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return Ok(expenses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expenses");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExpenseDto>> GetExpense(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var expense = await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
                return NotFound();

            return Ok(new ExpenseDto
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Currency = expense.Currency,
                Description = expense.Description,
                CategoryId = expense.CategoryId,
                CategoryName = expense.Category.Name,
                CategoryIcon = expense.Category.Icon,
                CategoryColor = expense.Category.Color,
                Date = expense.Date,
                ReceiptUrl = expense.ReceiptUrl,
                Tags = expense.Tags,
                CreatedAt = expense.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expense");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> CreateExpense([FromBody] CreateExpenseDto dto)
    {
        try
        {
            var userId = GetUserId();
            
            // Verify category exists and belongs to user or is system
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId && 
                    (c.UserId == userId || c.IsSystem));

            if (category == null)
                return BadRequest(new { message = "Invalid category" });

            var expense = new Expense
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Amount = dto.Amount,
                Currency = dto.Currency,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                Date = dto.Date,
                Tags = dto.Tags,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetExpense), new { id = expense.Id }, new ExpenseDto
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Currency = expense.Currency,
                Description = expense.Description,
                CategoryId = expense.CategoryId,
                CategoryName = category.Name,
                CategoryIcon = category.Icon,
                CategoryColor = category.Color,
                Date = expense.Date,
                ReceiptUrl = expense.ReceiptUrl,
                Tags = expense.Tags,
                CreatedAt = expense.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ExpenseDto>> UpdateExpense(Guid id, [FromBody] UpdateExpenseDto dto)
    {
        try
        {
            var userId = GetUserId();
            var expense = await _context.Expenses
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
                return NotFound();

            if (dto.Amount.HasValue)
                expense.Amount = dto.Amount.Value;

            if (!string.IsNullOrEmpty(dto.Description))
                expense.Description = dto.Description;

            if (dto.CategoryId.HasValue)
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == dto.CategoryId.Value && 
                        (c.UserId == userId || c.IsSystem));
                
                if (category == null)
                    return BadRequest(new { message = "Invalid category" });
                
                expense.CategoryId = dto.CategoryId.Value;
            }

            if (dto.Date.HasValue)
                expense.Date = dto.Date.Value;

            if (dto.Tags != null)
                expense.Tags = dto.Tags;

            expense.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new ExpenseDto
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Currency = expense.Currency,
                Description = expense.Description,
                CategoryId = expense.CategoryId,
                CategoryName = expense.Category.Name,
                CategoryIcon = expense.Category.Icon,
                CategoryColor = expense.Category.Color,
                Date = expense.Date,
                ReceiptUrl = expense.ReceiptUrl,
                Tags = expense.Tags,
                CreatedAt = expense.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating expense");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var expense = await _context.Expenses
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

            if (expense == null)
                return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expense");
            return StatusCode(500, new { message = "An error occurred" });
        }
    }
}
