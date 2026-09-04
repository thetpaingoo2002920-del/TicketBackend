using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TicketBackend.Models;

public class Ticket
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } 

    [BsonElement("ticketId")]
    public string? TicketId { get; set; }

    [BsonElement("ticketType")]
    public string? TicketType { get; set; }

    [BsonElement("eventId")]
    public string EventId { get; set; } = string.Empty; 

    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty; // 👈 ဤနေရာတွင် ရှိရပါမည်

    [BsonElement("quantity")]
    public int Quantity { get; set; }  

    [BsonElement("fullName")]
    public string FullName { get; set; } = string.Empty;

    [BsonElement("userName")]
    public string UserName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("purchasedAt")]
    public DateTime PurchasedAt { get; set; } = DateTime.Now;
}