using InvoiceTrackerAPI2.Models;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTrackerAPI2.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // entry points for querying tables
    public DbSet<User> Users => Set<User>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<LineItem> LineItems => Set<LineItem>();

// relational rules in code rather than attributes
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // make sure email is unique - enforced at DB lvl
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.User)
            .WithMany()
            .HasForeignKey(i => i.UserId)
            // cascading delete - deleting a user deletes the invoice
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Invoice>()
            .Property(i => i.Status)
            // store enum as string 
            .HasConversion<string>();

        // all queries filter by UserId first — most selective index
        modelBuilder.Entity<Invoice>()
            .HasIndex(i => i.UserId);

        // covers ORDER BY IssueDate and date range filters (From/To)
        modelBuilder.Entity<Invoice>()
            .HasIndex(i => new { i.UserId, i.IssueDate });

        // covers status filter tab/dropdown queries
        modelBuilder.Entity<Invoice>()
            .HasIndex(i => new { i.UserId, i.Status });

        modelBuilder.Entity<LineItem>()
            .HasOne(l => l.Invoice)
            .WithMany(i => i.LineItems)
            .HasForeignKey(l => l.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
