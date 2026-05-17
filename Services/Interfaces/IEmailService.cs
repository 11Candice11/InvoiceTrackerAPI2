namespace InvoiceTrackerAPI2.Services.Interfaces;

public interface IEmailService
{
    Task SendInvoiceAsync(string toEmail, string toName, string invoiceNumber, string htmlBody);
}
