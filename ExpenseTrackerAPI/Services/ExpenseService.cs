using Microsoft.EntityFrameworkCore;
using ExpenseTrackerAPI.Data;
using ExpenseTrackerAPI.DTOs;
using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.Services;

public class ExpenseService : IExpenseService
{
    private readonly ApplicationDbContext _context;

    public ExpenseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ExpenseDto>> GetUserExpenses(Guid userId)
    {
        return await _context.Expenses
            .Where(e => e.UserId == userId)
            .Include(e => e.Category)
            .OrderByDescending(e => e.Date)
            .Select(e => new ExpenseDto
            {
                Id = e.Id,
                Amount = e.Amount,
                Currency = e.Currency,
                Description = e.Description,
                CategoryId = e.CategoryId,
                CategoryName = e.Category.Name,
                Date = e.Date,
                ReceiptUrl = e.ReceiptUrl,
                Tags = e.Tags,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ExpenseDto?> GetExpenseById(Guid expenseId, Guid userId)
    {
        var expense = await _context.Expenses
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId);

        if (expense == null) return null;

        return new ExpenseDto
        {
            Id = expense.Id,
            Amount = expense.Amount,
            Currency = expense.Currency,
            Description = expense.Description,
            CategoryId = expense.CategoryId,
            CategoryName = expense.Category.Name,
            Date = expense.Date,
            ReceiptUrl = expense.ReceiptUrl,
            Tags = expense.Tags,
            CreatedAt = expense.CreatedAt
        };
    }

    public async Task<ExpenseDto> CreateExpense(Guid userId, CreateExpenseDto dto)
    {
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

        // Load category for response
        await _context.Entry(expense).Reference(e => e.Category).LoadAsync();

        return new ExpenseDto
        {
            Id = expense.Id,
            Amount = expense.Amount,
            Currency = expense.Currency,
            Description = expense.Description,
            CategoryId = expense.CategoryId,
            CategoryName = expense.Category.Name,
            Date = expense.Date,
            ReceiptUrl = expense.ReceiptUrl,
            Tags = expense.Tags,
            CreatedAt = expense.CreatedAt
        };
    }

    public async Task<ExpenseDto?> UpdateExpense(Guid expenseId, Guid userId, UpdateExpenseDto dto)
    {
        var expense = await _context.Expenses
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId);

        if (expense == null) return null;

        if (dto.Amount.HasValue) expense.Amount = dto.Amount.Value;
        if (dto.Description != null) expense.Description = dto.Description;
        if (dto.CategoryId.HasValue) expense.CategoryId = dto.CategoryId.Value;
        if (dto.Date.HasValue) expense.Date = dto.Date.Value;
        if (dto.Tags != null) expense.Tags = dto.Tags;

        expense.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Reload category if changed
        if (dto.CategoryId.HasValue)
        {
            await _context.Entry(expense).Reference(e => e.Category).LoadAsync();
        }

        return new ExpenseDto
        {
            Id = expense.Id,
            Amount = expense.Amount,
            Currency = expense.Currency,
            Description = expense.Description,
            CategoryId = expense.CategoryId,
            CategoryName = expense.Category.Name,
            Date = expense.Date,
            ReceiptUrl = expense.ReceiptUrl,
            Tags = expense.Tags,
            CreatedAt = expense.CreatedAt
        };
    }

    public async Task<bool> DeleteExpense(Guid expenseId, Guid userId)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.UserId == userId);

        if (expense == null) return false;

        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync();
        return true;
    }
}
