namespace ExpenseTrackerAPI.DTOs;

public class CreateExpenseDto
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public DateTime Date { get; set; }
    public string[]? Tags { get; set; }
}

public class UpdateExpenseDto
{
    public decimal? Amount { get; set; }
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTime? Date { get; set; }
    public string[]? Tags { get; set; }
}

public class ExpenseDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = string.Empty;
    public string CategoryColor { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string? ReceiptUrl { get; set; }
    public string[]? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
}
