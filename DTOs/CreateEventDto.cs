namespace TicketBackend.DTOs;

public class CreateEventDto
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public string EventDate { get; set; } = string.Empty;
    public decimal TicketPrice { get; set; }
    public int AvailableTickets { get; set; }
}