using InvoiceTrackerAPI2.Data;
using InvoiceTrackerAPI2.DTOs;
using InvoiceTrackerAPI2.Models;
using InvoiceTrackerAPI2.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTrackerAPI2.Repositories;

// implements interface with EF Core
public class InvoiceRepository(AppDbContext db) : IInvoiceRepository
{
    // builds query incrementally
    public async Task<IEnumerable<Invoice>> GetAllAsync(int userId, InvoiceFilterDto filter)
    {
        var query = db.Invoices
            .Include(i => i.LineItems)
            .Where(i => i.UserId == userId);

        if (filter.Status is not null)
            query = query.Where(i => i.Status == filter.Status);

        if (!string.IsNullOrWhiteSpace(filter.ClientName))
            query = query.Where(i => i.ClientName.Contains(filter.ClientName));

        if (filter.From is not null)
            query = query.Where(i => i.IssueDate >= filter.From);

        if (filter.To is not null)
            query = query.Where(i => i.IssueDate <= filter.To);

        return await query.OrderByDescending(i => i.IssueDate).ToListAsync();
    }

    public Task<Invoice?> GetByIdAsync(int id, int userId) =>
    // evry query that returns invoices will include line items
    // called eager loading
        db.Invoices.Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);

    public async Task<Invoice> CreateAsync(Invoice invoice)
    {
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice?> UpdateAsync(Invoice invoice)
    {
        db.Invoices.Update(invoice);
        await db.SaveChangesAsync();
        return invoice;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        // does a fetch and then delete not just delete
        // less efficient but respects userId ownership check
        // you cant delete someone else's invoice
        var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.Id == id && i.UserId == userId);
        if (invoice is null) return false;
        db.Invoices.Remove(invoice);
        await db.SaveChangesAsync();
        return true;
    }
}
