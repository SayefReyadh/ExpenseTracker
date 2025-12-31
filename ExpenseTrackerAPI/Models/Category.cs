namespace ExpenseTrackerAPI.Models;

public class Category
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; } // Null for system categories
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = "#000000";
    public bool IsSystem { get; set; } // True for predefined categories
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
}
