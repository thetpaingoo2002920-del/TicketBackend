using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TicketBackend.DTOs;
using TicketBackend.Models;

namespace TicketBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IMongoCollection<Event> _eventsCollection;

    public EventsController(IMongoDatabase database)
    {
        _eventsCollection = database.GetCollection<Event>("Events");
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Event>>> GetEvents()
    {
        try
        {
            // MongoDB တွင် _id ဖြင့် Descending စီရန် (သို့မဟုတ် Date ဖြင့် စီနိုင်ပါသည်)
            var events = await _eventsCollection.Find(_ => true)
                .SortByDescending(e => e.Id)
                .ToListAsync();
            return Ok(events);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<Event>> CreateEvent([FromBody] CreateEventDto dto)
    {
        try
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

            await _eventsCollection.InsertOneAsync(newEvent);

            return CreatedAtAction(nameof(GetEvents), new { id = newEvent.Id }, newEvent);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEvent(string id, [FromBody] CreateEventDto dto)
    {
        try
        {
            var existing = await _eventsCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
            if (existing == null)
            {
                return NotFound(new { message = "ပွဲ ရှာမတွေ့ပါ။" });
            }

            if (!DateTime.TryParse(dto.EventDate, out var parsedDate))
            {
                parsedDate = existing.EventDate;
            }

            existing.Title = dto.Title ?? existing.Title;
            existing.Category = string.IsNullOrWhiteSpace(dto.Category) ? existing.Category : dto.Category;
            existing.Venue = string.IsNullOrWhiteSpace(dto.Venue) ? "တောင်ကြီးမြို့" : dto.Venue;
            existing.EventDate = parsedDate;
            existing.TicketPrice = dto.TicketPrice;
            existing.TotalTickets = dto.AvailableTickets;
            existing.AvailableTickets = dto.AvailableTickets;

            await _eventsCollection.ReplaceOneAsync(e => e.Id == id, existing);
            return Ok(new { message = "ပွဲစဉ် အောင်မြင်စွာ ပြင်ဆင်ပြီးပါပြီ!", eventItem = existing });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(string id)
    {
        try
        {
            var result = await _eventsCollection.DeleteOneAsync(e => e.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound(new { message = "ဖျက်လိုသော ပွဲ ရှာမတွေ့ပါ။" });
            }

            return Ok(new { message = "ပွဲကို အောင်မြင်စွာ ဖျက်ထုတ်ပြီးပါပြီ။" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }
}