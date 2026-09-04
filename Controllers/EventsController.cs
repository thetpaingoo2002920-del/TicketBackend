using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketBackend.Data;
using TicketBackend.DTOs;
using TicketBackend.Models;

namespace TicketBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EventsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
    {
        return await _context.Events.OrderByDescending(e => e.Id).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Event>> CreateEvent([FromBody] CreateEventDto dto)
    {
        if (!DateTime.TryParse(dto.EventDate, out var parsedDate))
        {
            parsedDate = DateTime.UtcNow;
        }

        var newEvent = new Event
        {
            Title = dto.Title,
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "General" : dto.Category,
            Venue = string.IsNullOrWhiteSpace(dto.Venue) ? "တောင်ကြီးမြို့" : dto.Venue,
            EventDate = parsedDate,
            TicketPrice = dto.TicketPrice,
            TotalTickets = dto.AvailableTickets,
            AvailableTickets = dto.AvailableTickets
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetEvents), new { id = newEvent.Id }, newEvent);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(int id, [FromBody] CreateEventDto dto)
    {
        var existing = await _context.Events.FindAsync(id);
        if (existing == null)
        {
            return NotFound(new { message = "ပွဲ ရှာမတွေ့ပါ။" });
        }

        if (!DateTime.TryParse(dto.EventDate, out var parsedDate))
        {
            parsedDate = existing.EventDate;
        }

        existing.Title = dto.Title;
        existing.Category = string.IsNullOrWhiteSpace(dto.Category) ? existing.Category : dto.Category;
        existing.Venue = string.IsNullOrWhiteSpace(dto.Venue) ? "တောင်ကြီးမြို့" : dto.Venue;
        existing.EventDate = parsedDate;
        existing.TicketPrice = dto.TicketPrice;
        existing.TotalTickets = dto.AvailableTickets;
        existing.AvailableTickets = dto.AvailableTickets;

        await _context.SaveChangesAsync();
        return Ok(new { message = "ပွဲစဉ် အောင်မြင်စွာ ပြင်ဆင်ပြီးပါပြီ!", eventItem = existing });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var item = await _context.Events.FindAsync(id);
        if (item == null)
        {
            return NotFound(new { message = "ဖျက်လိုသော ပွဲ ရှာမတွေ့ပါ။" });
        }

        _context.Events.Remove(item);
        await _context.SaveChangesAsync();

        return Ok(new { message = "ပွဲကို အောင်မြင်စွာ ဖျက်ထုတ်ပြီးပါပြီ။" });
    }
}