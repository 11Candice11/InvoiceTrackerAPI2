using InvoiceTrackerAPI2.Data;
using InvoiceTrackerAPI2.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace InvoiceTrackerAPI2.Services;

public class OverdueInvoiceJob(IServiceScopeFactory scopeFactory, ILogger<OverdueInvoiceJob> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await WaitUntilMidnightUtcAsync(stoppingToken);

            if (stoppingToken.IsCancellationRequested) break;

            await MarkOverdueAsync(stoppingToken);
        }
    }

    internal async Task RunMarkOverdueAsync(CancellationToken ct) => await MarkOverdueAsync(ct);

    private async Task MarkOverdueAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var today = DateTime.UtcNow.Date;
            var count = await db.Invoices
                .Where(i => i.Status == InvoiceStatus.Sent && i.DueDate < today)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.Status, InvoiceStatus.Overdue), ct);

            if (count > 0)
                logger.LogInformation("Marked {Count} invoice(s) as Overdue.", count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to mark overdue invoices.");
        }
    }

    private static async Task WaitUntilMidnightUtcAsync(CancellationToken ct)
    {
        var now   = DateTime.UtcNow;
        var delay = now.Date.AddDays(1) - now;
        await Task.Delay(delay, ct).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
    }
}
