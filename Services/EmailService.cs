using InvoiceTrackerAPI2.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace InvoiceTrackerAPI2.Services;

public class EmailService(IConfiguration config) : IEmailService
{
    public async Task SendInvoiceAsync(string toEmail, string toName, string invoiceNumber, string htmlBody)
    {
        var smtp    = config["Email:Smtp"]!;
        var port    = int.Parse(config["Email:Port"] ?? "587");
        var user    = config["Email:Username"]!;
        var pass    = config["Email:Password"]!;
        var from    = config["Email:From"] ?? user;
        var fromName = config["Email:FromName"] ?? "TALLY ";

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, from));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = $"Invoice {invoiceNumber}";

        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = $"Please find your invoice {invoiceNumber} attached. View it online or contact us for details."
        };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(smtp, port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(user, pass);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
