using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TicketBackend.Models;

namespace TicketBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IMongoCollection<User> _usersCollection;

        public AdminController(IMongoDatabase database)
        {
            _usersCollection = database.GetCollection<User>("Users");
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _usersCollection.Find(_ => true).ToListAsync();
                
            
                var userResponse = users.Select(u => new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    email = u.Email,
                    role = u.Role
                });

                return Ok(userResponse);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Users များကို ထုတ်ယူရာတွင် Error ဖြစ်နေပါသည်။", error = ex.Message });
            }
        }
        [HttpPut("users/{id}/role")]
        public async Task<IActionResult> UpdateUserRole(string id, [FromBody] RoleUpdateDto dto)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(u => u.Id, id);
                var update = Builders<User>.Update.Set(u => u.Role, dto.Role);

                var result = await _usersCollection.UpdateOneAsync(filter, update);

                if (result.MatchedCount == 0)
                {
                    return NotFound(new { message = "ဒီ User ကို ရှာမတွေ့ပါ။" });
                }

                return Ok(new { message = "User ၏ Role အောင်မြင်စွာ ပြောင်းလဲပြီးပါပြီ။" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Role ပြောင်းလဲရာတွင် Error ဖြစ်နေပါသည်။", error = ex.Message });
            }
        }
    }

    public class RoleUpdateDto
    {
        public string Role { get; set; } = "User";
    }
}