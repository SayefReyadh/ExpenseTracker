using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.DTOs;

public class CreateBudgetDto
{
    public Guid CategoryId { get; set; }
    public decimal Amount { get; set; }
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class UpdateBudgetDto
{
    public decimal? Amount { get; set; }
    public DateTime? EndDate { get; set; }
}

public class BudgetDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public BudgetPeriod Period { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal Spent { get; set; }
    public decimal Remaining { get; set; }
    public decimal PercentageUsed { get; set; }
}
