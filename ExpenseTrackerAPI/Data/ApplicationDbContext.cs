using Microsoft.EntityFrameworkCore;
using ExpenseTrackerAPI.Models;

namespace ExpenseTrackerAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Budget> Budgets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        });

        // Expense configuration
        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasOne(e => e.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Category)
                .WithMany(c => c.Expenses)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(7);
            
            entity.HasOne(c => c.User)
                .WithMany(u => u.Categories)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Budget configuration
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            
            entity.HasOne(b => b.User)
                .WithMany(u => u.Budgets)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(b => b.Category)
                .WithMany(c => c.Budgets)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed system categories
        var systemCategories = new[]
        {
            new Category { Id = Guid.NewGuid(), Name = "Food & Dining", Icon = "🍔", Color = "#FF6B6B", IsSystem = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Transportation", Icon = "🚗", Color = "#4ECDC4", IsSystem = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Shopping", Icon = "🛍️", Color = "#45B7D1", IsSystem = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Entertainment", Icon = "🎬", Color = "#FFA07A", IsSystem = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Healthcare", Icon = "⚕️", Color = "#98D8C8", IsSystem = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Bills & Utilities", Icon = "💡", Color = "#F7DC6F", IsSystem = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Education", Icon = "📚", Color = "#BB8FCE", IsSystem = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Travel", Icon = "✈️", Color = "#85C1E2", IsSystem = true, CreatedAt = DateTime.UtcNow },
            new Category { Id = Guid.NewGuid(), Name = "Other", Icon = "📦", Color = "#95A5A6", IsSystem = true, CreatedAt = DateTime.UtcNow },
        };

        modelBuilder.Entity<Category>().HasData(systemCategories);
    }
}
