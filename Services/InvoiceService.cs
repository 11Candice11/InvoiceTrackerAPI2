using AutoMapper;
using InvoiceTrackerAPI2.DTOs;
using InvoiceTrackerAPI2.Models;
using InvoiceTrackerAPI2.Models.Enums;
using InvoiceTrackerAPI2.Repositories.Interfaces;
using InvoiceTrackerAPI2.Services.Interfaces;

namespace InvoiceTrackerAPI2.Services;

// TODO retry on collision or use sequential counter
public class InvoiceService(IInvoiceRepository repo, IMapper mapper) : IInvoiceService
{
    public async Task<PagedResult<InvoiceDto>> GetAllAsync(int userId, InvoiceFilterDto filter)
    {
        var (items, total) = await repo.GetAllAsync(userId, filter);
        return new PagedResult<InvoiceDto>
        {
            Items    = mapper.Map<IEnumerable<InvoiceDto>>(items),
            Total    = total,
            Page     = filter.Page,
            PageSize = filter.PageSize,
        };
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
        invoice.InvoiceNumber = GenerateInvoiceNumber();

        var created = await repo.CreateAsync(invoice);
        return mapper.Map<InvoiceDto>(created);
    }

    public async Task<InvoiceDto?> UpdateAsync(int id, int userId, UpdateInvoiceDto dto)
    {
        var existing = await repo.GetByIdAsync(id, userId);
        if (existing is null) return null;

        mapper.Map(dto, existing);
        var updated = await repo.UpdateAsync(existing);
        return mapper.Map<InvoiceDto>(updated);
    }

    public async Task<InvoiceDto?> UpdateStatusAsync(int id, int userId, InvoiceStatus status)
    {
        var existing = await repo.GetByIdAsync(id, userId);
        if (existing is null) return null;

        if (!AllowedTransitions.TryGetValue(existing.Status, out var allowed) || !allowed.Contains(status))
            throw new InvalidOperationException(
                $"Cannot transition from {existing.Status} to {status}.");

        existing.Status = status;
        var updated = await repo.UpdateAsync(existing);
        return mapper.Map<InvoiceDto>(updated);
    }

    private static readonly Dictionary<InvoiceStatus, InvoiceStatus[]> AllowedTransitions = new()
    {
        [InvoiceStatus.Draft]     = [InvoiceStatus.Sent,    InvoiceStatus.Cancelled],
        [InvoiceStatus.Sent]      = [InvoiceStatus.Paid,    InvoiceStatus.Overdue,  InvoiceStatus.Cancelled],
        [InvoiceStatus.Overdue]   = [InvoiceStatus.Paid,    InvoiceStatus.Cancelled],
        [InvoiceStatus.Paid]      = [],
        [InvoiceStatus.Cancelled] = [],
    };

    public Task<bool> DeleteAsync(int id, int userId) => repo.DeleteAsync(id, userId);

    private static string GenerateInvoiceNumber() =>
        $"INV-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
}
