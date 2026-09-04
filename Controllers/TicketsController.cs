using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketBackend.Data;
using TicketBackend.Models;

namespace TicketBackend.Controllers
{
    [Route("api/tickets")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTickets()
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Event) 
                    .OrderByDescending(t => t.Id)
                    .Select(t => new {
                        id = t.Id,
                        ticketId = t.TicketId,
                        ticketType = t.TicketType,
                        quantity = t.Quantity,
                        purchasedAt = t.PurchasedAt,
                        fullName = t.FullName,
                        userName = t.UserName,
                        email = t.Email,
                        eventId = t.EventId,
                        eventTitle = t.Event != null ? t.Event.Title : "ပွဲအမည်မရှိပါ",
                        venue = t.Event != null ? t.Event.Venue : "တောင်ကြီးမြို့",
                        ticketPrice = t.Event != null ? t.Event.TicketPrice : 0,
                        totalPrice = (t.Event != null ? t.Event.TicketPrice : 0) * t.Quantity
                    })
                    .ToListAsync();

                return Ok(tickets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("user/{email}")]
        public async Task<IActionResult> GetUserTickets(string email)
        {
            try
            {
                var tickets = await _context.Tickets
                    .Include(t => t.Event)
                    .Where(t => t.Email == email)
                    .OrderByDescending(t => t.PurchasedAt)
                    .Select(t => new {
                        id = t.Id,
                        ticketId = t.TicketId,
                        ticketType = t.TicketType,
                        quantity = t.Quantity,
                        purchasedAt = t.PurchasedAt,
                        fullName = t.FullName,
                        userName = t.UserName,
                        email = t.Email,
                        eventId = t.EventId,
                        eventTitle = t.Event != null ? t.Event.Title : "ပွဲအမည်မရှိပါ",
                        venue = t.Event != null ? t.Event.Venue : "တောင်ကြီးမြို့",
                        ticketPrice = t.Event != null ? t.Event.TicketPrice : 0,
                        totalPrice = (t.Event != null ? t.Event.TicketPrice : 0) * t.Quantity
                    })
                    .ToListAsync();

                return Ok(tickets);
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

                var ticket = await _context.Tickets
                    .Include(t => t.Event)
                    .FirstOrDefaultAsync(t => t.TicketId == model.TicketId);

                if (ticket == null)
                {
                    return NotFound(new { success = false, message = "ဤ Ticket ID မှာ မှားယွင်းနေပါသည် သို့မဟုတ် မရှိပါ။" });
                }

                return Ok(new
                {
                    success = true,
                    message = "လက်မှတ်အချက်အလက် မှန်ကန်ပါသည်!",
                    ticket = new
                    {
                        ticketId = ticket.TicketId,
                        eventTitle = ticket.Event != null ? ticket.Event.Title : "ပွဲအမည်မရှိပါ",
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

                int parsedEventId = 0;
                if (model.EventId != null)
                {
                    int.TryParse(model.EventId.ToString(), out parsedEventId);
                }

                var eventsList = await _context.Events.ToListAsync();
                var targetEvent = eventsList.FirstOrDefault(e => e.Id == parsedEventId);

                if (targetEvent == null)
                {
                    return BadRequest(new { success = false, message = $"ID {model.EventId} ရှိသော ပွဲစဉ်ကို ရှာမတွေ့ပါ။" });
                }

                if (targetEvent.AvailableTickets < model.Quantity)
                {
                    return BadRequest(new { success = false, message = $"လက်မှတ် မလုံလောက်တော့ပါ။ ကျန်ရှိမှု: {targetEvent.AvailableTickets}" });
                }

                targetEvent.AvailableTickets -= model.Quantity;
                _context.Entry(targetEvent).State = EntityState.Modified;

                var ticket = new Ticket
                {
                    TicketId = model.TicketId ?? "TKT-" + new Random().Next(100000, 999999),
                    EventId = targetEvent.Id.ToString(), 
                    TicketType = model.TicketType ?? "Normal",
                    Quantity = model.Quantity > 0 ? model.Quantity : 1,
                    FullName = model.FullName ?? "User",
                    UserName = model.UserName ?? "User",
                    Email = model.Email ?? "user@github.com",
                    PurchasedAt = DateTime.Now,
                    Event = null 
                };

                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();

                return Ok(new 
                { 
                    success = true, 
                    message = "Ticket saved successfully", 
                    data = ticket 
                });
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error saving ticket", 
                    error = ex.Message, 
                    innerError = innerMsg 
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(string id)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(id);
                if (ticket == null)
                {
                    return NotFound(new { success = false, message = "Ticket not found" });
                }

                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();

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
        public object? EventId { get; set; } 
        public string? TicketType { get; set; }
        public int Quantity { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
}