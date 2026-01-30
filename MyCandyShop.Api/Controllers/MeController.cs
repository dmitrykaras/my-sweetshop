using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MyCandyShop.Api.Controllers;

[ApiController]
[Route("me")]
public class MeController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public IActionResult GetMe()
    {
        var userId = User.FindFirstValue("uid");
        var email = User.FindFirstValue("email");

        return Ok(new
        {
            userId,
            email
        });
    }
}