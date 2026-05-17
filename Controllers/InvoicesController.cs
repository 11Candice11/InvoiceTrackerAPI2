using System.Security.Claims;
using InvoiceTrackerAPI2.DTOs;
using InvoiceTrackerAPI2.Models.Enums;
using InvoiceTrackerAPI2.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceTrackerAPI2.Controllers;

// API is fully stateless

[ApiController]
[Route("api/invoices")]
[Authorize] // requires valid JWT
public class InvoicesController(IInvoiceService invoiceService, IEmailService emailService) : ControllerBase
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
        var created = await invoiceService.CreateAsync(UserId, dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateInvoiceDto dto)
    {
        var updated = await invoiceService.UpdateAsync(id, UserId, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
    {
        try
        {
            var updated = await invoiceService.UpdateStatusAsync(id, UserId, dto.Status);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return UnprocessableEntity(new { message = ex.Message });
        }
    }
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await invoiceService.DeleteAsync(id, UserId);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/send")]
    public async Task<IActionResult> Send(int id, [FromBody] SendInvoiceDto dto)
    {
        var invoice = await invoiceService.GetByIdAsync(id, UserId);
        if (invoice is null) return NotFound();

        var html = BuildInvoiceHtml(invoice, dto.ToEmail);
        await emailService.SendInvoiceAsync(dto.ToEmail, invoice.ClientName, invoice.InvoiceNumber, html);

        if (invoice.Status == InvoiceStatus.Draft)
            await invoiceService.UpdateStatusAsync(id, UserId, InvoiceStatus.Sent);

        return Ok(new { message = "Invoice sent successfully." });
    }

    private static string BuildInvoiceHtml(InvoiceDto inv, string toEmail)
    {
        var subtotal = inv.LineItems.Sum(l => l.Qty * l.UnitPrice);
        var vat      = subtotal * inv.VatRate;
        var total    = subtotal + vat;

        var invoiceNumber = Encode(inv.InvoiceNumber);
        var clientName    = Encode(inv.ClientName);
        var clientEmail   = Encode(toEmail);

        var rows = string.Join("", inv.LineItems.Select(l =>
            $"""
            <tr>
              <td style="padding:12px 0;border-bottom:1px solid #f0f0f0;">{Encode(l.Description)}</td>
              <td style="padding:12px 0;border-bottom:1px solid #f0f0f0;text-align:center;">{l.Qty}</td>
              <td style="padding:12px 0;border-bottom:1px solid #f0f0f0;text-align:right;">R {l.UnitPrice:N2}</td>
              <td style="padding:12px 0;border-bottom:1px solid #f0f0f0;text-align:right;font-weight:700;">R {l.Qty * l.UnitPrice:N2}</td>
            </tr>
            """));

        return $"""
        <!DOCTYPE html>
        <html>
        <head><meta charset="utf-8"/></head>
        <body style="font-family:-apple-system,BlinkMacSystemFont,'Segoe UI',sans-serif;background:#f5f5f5;margin:0;padding:32px;">
          <div style="max-width:600px;margin:0 auto;background:#fff;border-radius:12px;overflow:hidden;border:1px solid #e0e0e0;">
            <div style="background:#1e293b;padding:28px 32px;">
              <p style="color:#94a3b8;font-size:11px;letter-spacing:1px;margin:0 0 6px;">INVOICE</p>
              <h1 style="color:#fff;font-size:24px;margin:0;letter-spacing:-0.5px;">{invoiceNumber}</h1>
            </div>
            <div style="padding:32px;">
              <table style="width:100%;margin-bottom:24px;">
                <tr>
                  <td>
                    <p style="font-size:11px;color:#999;letter-spacing:1px;margin:0 0 4px;">BILLED TO</p>
                    <p style="font-size:16px;font-weight:700;color:#1a1a1a;margin:0;">{clientName}</p>
                    <p style="font-size:13px;color:#999;margin:4px 0 0;">{clientEmail}</p>
                  </td>
                  <td style="text-align:right;">
                    <p style="font-size:11px;color:#999;letter-spacing:1px;margin:0 0 4px;">ISSUE DATE</p>
                    <p style="font-size:13px;color:#1a1a1a;margin:0;">{inv.IssueDate:dd MMM yyyy}</p>
                    <p style="font-size:11px;color:#999;letter-spacing:1px;margin:12px 0 4px;">DUE DATE</p>
                    <p style="font-size:13px;color:#1a1a1a;margin:0;">{inv.DueDate:dd MMM yyyy}</p>
                  </td>
                </tr>
              </table>

              <table style="width:100%;border-collapse:collapse;">
                <thead>
                  <tr style="border-bottom:2px solid #e0e0e0;">
                    <th style="font-size:11px;color:#999;padding:0 0 10px;text-align:left;font-weight:600;letter-spacing:.5px;">DESCRIPTION</th>
                    <th style="font-size:11px;color:#999;padding:0 0 10px;text-align:center;font-weight:600;letter-spacing:.5px;">QTY</th>
                    <th style="font-size:11px;color:#999;padding:0 0 10px;text-align:right;font-weight:600;letter-spacing:.5px;">UNIT PRICE</th>
                    <th style="font-size:11px;color:#999;padding:0 0 10px;text-align:right;font-weight:600;letter-spacing:.5px;">TOTAL</th>
                  </tr>
                </thead>
                <tbody>{rows}</tbody>
              </table>

              <div style="margin-top:24px;padding-top:16px;border-top:1px solid #e0e0e0;text-align:right;">
                <p style="font-size:13px;color:#666;margin:0 0 6px;">Subtotal: <strong style="color:#1a1a1a;">R {subtotal:N2}</strong></p>
                <p style="font-size:13px;color:#666;margin:0 0 12px;">VAT ({inv.VatRate * 100:0}%): <strong style="color:#1a1a1a;">R {vat:N2}</strong></p>
                <p style="font-size:20px;font-weight:700;color:#1a1a1a;margin:0;">Total: R {total:N2}</p>
              </div>

              {(inv.Notes is not null ? $"""
              <div style="margin-top:24px;padding:16px;background:#f9f9f9;border-radius:8px;">
                <p style="font-size:11px;color:#999;letter-spacing:1px;margin:0 0 8px;">NOTES</p>
                <p style="font-size:13px;color:#666;margin:0;">{Encode(inv.Notes)}</p>
              </div>
              """ : "")}
            </div>
            <div style="padding:20px 32px;background:#f9f9f9;border-top:1px solid #e0e0e0;text-align:center;">
              <p style="font-size:12px;color:#999;margin:0;">Sent via TALLY </p>
            </div>
          </div>
        </body>
        </html>
        """;
    }

    private static string Encode(string value) => System.Net.WebUtility.HtmlEncode(value);
}
