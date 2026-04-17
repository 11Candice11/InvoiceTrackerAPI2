using InvoiceTrackerAPI2.DTOs;

namespace InvoiceTrackerAPI2.Services.Interfaces;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceDto>> GetAllAsync(int userId, InvoiceFilterDto filter);
    Task<InvoiceDto?> GetByIdAsync(int id, int userId);
    Task<InvoiceDto> CreateAsync(int userId, CreateInvoiceDto dto);
    Task<InvoiceDto?> UpdateAsync(int id, int userId, CreateInvoiceDto dto);
    Task<bool> DeleteAsync(int id, int userId);
}
