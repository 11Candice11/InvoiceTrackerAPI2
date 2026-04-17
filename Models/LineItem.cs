namespace InvoiceTrackerAPI2.Models;

public class LineItem
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    // totals are computed never stored
    // calculated in mapping
}
