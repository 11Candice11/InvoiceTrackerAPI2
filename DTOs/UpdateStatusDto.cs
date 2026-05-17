using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using InvoiceTrackerAPI2.Models.Enums;

namespace InvoiceTrackerAPI2.DTOs;

public record UpdateStatusDto
{
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public InvoiceStatus Status { get; init; }
}
