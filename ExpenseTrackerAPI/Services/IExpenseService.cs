using ExpenseTrackerAPI.DTOs;

namespace ExpenseTrackerAPI.Services;

public interface IExpenseService
{
    Task<IEnumerable<ExpenseDto>> GetUserExpenses(Guid userId);
    Task<ExpenseDto?> GetExpenseById(Guid expenseId, Guid userId);
    Task<ExpenseDto> CreateExpense(Guid userId, CreateExpenseDto dto);
    Task<ExpenseDto?> UpdateExpense(Guid expenseId, Guid userId, UpdateExpenseDto dto);
    Task<bool> DeleteExpense(Guid expenseId, Guid userId);
}
