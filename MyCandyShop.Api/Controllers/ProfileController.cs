using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCandyShop.Api.Data;
using System.Security.Claims;

namespace MyCandyShop.Api.Controllers
{
    [ApiController]
    [Route("profile")]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProfileController(AppDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpGet("points")]
        public async Task<IActionResult> GetPoints()
        {
            var userIdStr = User.FindFirstValue("uid");
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid token");

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return NotFound("User not found");

            return Ok(new
            {
                Points = user.Points
            });
        }
    }
}