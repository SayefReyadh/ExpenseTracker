namespace ExpenseTrackerAPI.Models;

public class Expense
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public DateTime Date { get; set; }
    public string? ReceiptUrl { get; set; }
    public string[]? Tags { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
