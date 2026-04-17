using InvoiceTrackerAPI2.Models.Enums;

namespace InvoiceTrackerAPI2.Models;

public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string ClientEmail { get; set; } = string.Empty;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal VatRate { get; set; } = 0.15m;
    public string? Notes { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    // navigation property
    public List<LineItem> LineItems { get; set; } = [];
}
