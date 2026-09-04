using System.ComponentModel.DataAnnotations;

namespace TicketBackend.Models;

public class Ticket
{
    [Key]
    public string? Id { get; set; } 
    public string? TicketId { get; set; }
    public string? TicketType { get; set; }
    public string EventId { get; set; } = string.Empty; 
    public int Quantity { get; set; }  
    public string FullName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime PurchasedAt { get; set; } = DateTime.Now;

    public Event? Event { get; set; }
}