using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCandyShop.Api.Data;
using MyCandyShop.Api.Entities;

namespace MyCandyShop.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public UsersController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET api/users/{email} - проверить данные пользователя
    [HttpGet("{email}")]
    public async Task<ActionResult<User?>> GetUserByEmail(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    // POST api/users - создать или обновить пользователя
    [HttpPost]
    public async Task<ActionResult<User>> CreateOrUpdateUser([FromBody] User user)
    {
        var existing = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (existing != null)
        {
            // обновляем
            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
        }
        else
        {
            user.Id = Guid.NewGuid();
            user.CreatedAt = DateTimeOffset.UtcNow;
            _dbContext.Users.Add(user);
        }

        await _dbContext.SaveChangesAsync();
        return Ok(user);
    }
}