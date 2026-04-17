using AutoMapper;
using InvoiceTrackerAPI2.DTOs;
using InvoiceTrackerAPI2.Models;
using InvoiceTrackerAPI2.Repositories.Interfaces;
using InvoiceTrackerAPI2.Services.Interfaces;

namespace InvoiceTrackerAPI2.Services;

// main job is orchestration
// map DTO to model 
// set fields that client shouldn't control
// await repo
// call repository
// return DTO

public class InvoiceService(IInvoiceRepository repo, IMapper mapper) : IInvoiceService
{
    public async Task<IEnumerable<InvoiceDto>> GetAllAsync(int userId, InvoiceFilterDto filter)
    {
        var invoices = await repo.GetAllAsync(userId, filter);
        return mapper.Map<IEnumerable<InvoiceDto>>(invoices);
    }

    public async Task<InvoiceDto?> GetByIdAsync(int id, int userId)
    {
        var invoice = await repo.GetByIdAsync(id, userId);
        return invoice is null ? null : mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<InvoiceDto> CreateAsync(int userId, CreateInvoiceDto dto)
    {
        var invoice = mapper.Map<Invoice>(dto);
        invoice.UserId        = userId;
        invoice.InvoiceNumber = await GenerateInvoiceNumberAsync();

        var created = await repo.CreateAsync(invoice);
        return mapper.Map<InvoiceDto>(created);
    }

    public async Task<InvoiceDto?> UpdateAsync(int id, int userId, CreateInvoiceDto dto)
    {
        var existing = await repo.GetByIdAsync(id, userId);
        if (existing is null) return null;

        mapper.Map(dto, existing);
        var updated = await repo.UpdateAsync(existing);
        return mapper.Map<InvoiceDto>(updated);
    }

    public Task<bool> DeleteAsync(int id, int userId) => repo.DeleteAsync(id, userId);

    private async Task<string> GenerateInvoiceNumberAsync()
    {
        var count = await Task.FromResult(0); // TODO: replace with actual count query if needed
        return $"INV-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
    }
}
