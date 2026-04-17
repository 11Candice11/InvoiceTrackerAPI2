using System.Security.Claims;
using InvoiceTrackerAPI2.DTOs;
using InvoiceTrackerAPI2.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceTrackerAPI2.Controllers;

// API is fully stateless

[ApiController]
[Route("api/invoices")]
[Authorize] // requires valid JWT
public class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    // extract userId from JWT - this is how we know which user is making the request
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] InvoiceFilterDto filter) =>
        Ok(await invoiceService.GetAllAsync(UserId, filter));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var invoice = await invoiceService.GetByIdAsync(id, UserId);
        return invoice is null ? NotFound() : Ok(invoice);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateInvoiceDto dto)
    {
        // rather than just returning a 200 when created, we return a 201 with location header pointing to new resource
        var created = await invoiceService.CreateAsync(UserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateInvoiceDto dto)
    {
        var updated = await invoiceService.UpdateAsync(id, UserId, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await invoiceService.DeleteAsync(id, UserId);
        return deleted ? NoContent() : NotFound();
    }
}
