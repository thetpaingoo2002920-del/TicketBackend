using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TicketBackend.Models;

public class Event
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("category")]
    public string Category { get; set; } = string.Empty; 

    [BsonElement("venue")]
    public string Venue { get; set; } = string.Empty;

    [BsonElement("eventDate")]
    public DateTime EventDate { get; set; }

    [BsonElement("ticketPrice")]
    public decimal TicketPrice { get; set; }

    [BsonElement("totalTickets")]
    public int TotalTickets { get; set; }

    [BsonElement("availableTickets")]
    public int AvailableTickets { get; set; }
}