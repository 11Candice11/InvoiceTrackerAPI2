using InvoiceTrackerAPI2.DTOs;
using InvoiceTrackerAPI2.Models;

namespace InvoiceTrackerAPI2.Repositories.Interfaces;

// defines contract, service only ever talks to interface never to AppDbContext
public interface IInvoiceRepository
{
    Task<IEnumerable<Invoice>> GetAllAsync(int userId, InvoiceFilterDto filter);
    Task<Invoice?> GetByIdAsync(int id, int userId);
    Task<Invoice> CreateAsync(Invoice invoice);
    Task<Invoice?> UpdateAsync(Invoice invoice);
    Task<bool> DeleteAsync(int id, int userId);
}
