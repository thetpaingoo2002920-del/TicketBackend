using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TicketBackend.Models;

namespace TicketBackend.Controllers
{
    [Route("api/tickets")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly IMongoCollection<Ticket> _ticketsCollection;
        private readonly IMongoCollection<Event> _eventsCollection;

        public TicketsController(IMongoDatabase database)
        {
            _ticketsCollection = database.GetCollection<Ticket>("Tickets");
            _eventsCollection = database.GetCollection<Event>("Events");
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            try
            {
                var tickets = await _ticketsCollection.Find(_ => true)
                    .SortByDescending(t => t.Id)
                    .ToListAsync();

                var events = await _eventsCollection.Find(_ => true).ToListAsync();
                
                var eventDict = events
                    .Where(e => !string.IsNullOrEmpty(e.Id))
                    .ToDictionary(e => e.Id!, e => e);

                var result = tickets.Select(t => {
                    Event? evt = null;
                    if (!string.IsNullOrEmpty(t.EventId))
                    {
                        eventDict.TryGetValue(t.EventId, out evt);
                    }
                    return new {
                        id = t.Id,
                        ticketId = t.TicketId,
                        ticketType = t.TicketType,
                        quantity = t.Quantity,
                        purchasedAt = t.PurchasedAt,
                        fullName = t.FullName,
                        userName = t.UserName,
                        email = t.Email,
                        userId = t.UserId,
                        eventId = t.EventId,
                        eventTitle = evt != null ? evt.Title : "ပွဲအမည်မရှိပါ",
                        venue = evt != null ? evt.Venue : "တောင်ကြီးမြို့",
                        ticketPrice = evt != null ? evt.TicketPrice : 0,
                        totalPrice = (evt != null ? evt.TicketPrice : 0) * t.Quantity
                    };
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("user/{identifier}")]
        public async Task<IActionResult> GetUserTickets(string identifier)
        {
            try
            {
                var tickets = await _ticketsCollection.Find(t => t.Email == identifier || t.UserId == identifier)
                    .SortByDescending(t => t.PurchasedAt)
                    .ToListAsync();

                var events = await _eventsCollection.Find(_ => true).ToListAsync();
                
                var eventDict = events
                    .Where(e => !string.IsNullOrEmpty(e.Id))
                    .ToDictionary(e => e.Id!, e => e);

                var result = tickets.Select(t => {
                    Event? evt = null;
                    if (!string.IsNullOrEmpty(t.EventId))
                    {
                        eventDict.TryGetValue(t.EventId, out evt);
                    }
                    return new {
                        id = t.Id,
                        ticketId = t.TicketId,
                        ticketType = t.TicketType,
                        quantity = t.Quantity,
                        purchasedAt = t.PurchasedAt,
                        fullName = t.FullName,
                        userName = t.UserName,
                        email = t.Email,
                        userId = t.UserId,
                        eventId = t.EventId,
                        eventTitle = evt != null ? evt.Title : "ပွဲအမည်မရှိပါ",
                        venue = evt != null ? evt.Venue : "တောင်ကြီးမြို့",
                        ticketPrice = evt != null ? evt.TicketPrice : 0,
                        totalPrice = (evt != null ? evt.TicketPrice : 0) * t.Quantity
                    };
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyTicket([FromBody] TicketVerifyModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.TicketId))
                {
                    return BadRequest(new { success = false, message = "Ticket ID လိုအပ်ပါသည်။" });
                }

                var ticket = await _ticketsCollection.Find(t => t.TicketId == model.TicketId).FirstOrDefaultAsync();

                if (ticket == null)
                {
                    return NotFound(new { success = false, message = "ဤ Ticket ID မှာ မှားယွင်းနေပါသည် သို့မဟုတ် မရှိပါ။" });
                }

                Event? evt = null;
                if (!string.IsNullOrEmpty(ticket.EventId))
                {
                    evt = await _eventsCollection.Find(e => e.Id == ticket.EventId).FirstOrDefaultAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = "လက်မှတ်အချက်အလက် မှန်ကန်ပါသည်!",
                    ticket = new
                    {
                        ticketId = ticket.TicketId,
                        eventTitle = evt != null ? evt.Title : "ပွဲအမည်မရှိပါ",
                        userName = ticket.UserName ?? ticket.FullName,
                        email = ticket.Email,
                        ticketType = ticket.TicketType,
                        quantity = ticket.Quantity
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostTicket([FromBody] TicketDto model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new { success = false, message = "Invalid data received" });
                }

                string eventIdStr = model.EventId?.ToString() ?? string.Empty;
                var targetEvent = await _eventsCollection.Find(e => e.Id == eventIdStr).FirstOrDefaultAsync();

                if (targetEvent == null)
                {
                    return BadRequest(new { success = false, message = $"ID {model.EventId} ရှိသော ပွဲစဉ်ကို ရှာမတွေ့ပါ။" });
                }

                if (targetEvent.AvailableTickets < model.Quantity)
                {
                    return BadRequest(new { success = false, message = $"လက်မှတ် မလုံလောက်တော့ပါ။ ကျန်ရှိမှု: {targetEvent.AvailableTickets}" });
                }

                targetEvent.AvailableTickets -= model.Quantity;
                await _eventsCollection.ReplaceOneAsync(e => e.Id == targetEvent.Id, targetEvent);

                var ticket = new Ticket
                {
                    TicketId = model.TicketId ?? "TKT-" + new Random().Next(100000, 999999),
                    EventId = targetEvent.Id ?? string.Empty, 
                    UserId = model.UserId ?? string.Empty, 
                    TicketType = model.TicketType ?? "Normal",
                    Quantity = model.Quantity > 0 ? model.Quantity : 1,
                    FullName = model.FullName ?? "User",
                    UserName = model.UserName ?? "User",
                    Email = model.Email ?? "user@github.com",
                    PurchasedAt = DateTime.Now
                };

                await _ticketsCollection.InsertOneAsync(ticket);

                return Ok(new 
                { 
                    success = true, 
                    message = "Ticket saved successfully", 
                    data = ticket 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error saving ticket", 
                    error = ex.Message 
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(string id)
        {
            try
            {
                var result = await _ticketsCollection.DeleteOneAsync(t => t.Id == id);
                if (result.DeletedCount == 0)
                {
                    return NotFound(new { success = false, message = "Ticket not found" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }

    public class TicketVerifyModel
    {
        public string? TicketId { get; set; } 
    }

    public class TicketDto
    {
        public string? TicketId { get; set; }
        public object? EventId { get; set; } // 👈 ကျသွားသည့် syntax အမှားကို ပြင်ဆင်ပြီး
        public string? UserId { get; set; }
        public string? TicketType { get; set; }
        public int Quantity { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}