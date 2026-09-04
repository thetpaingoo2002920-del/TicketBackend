namespace TicketBackend.Models;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; 
    public string Venue { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public decimal TicketPrice { get; set; }
    public int TotalTickets { get; set; }
    public int AvailableTickets { get; set; }
}