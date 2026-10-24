using System.Security.Claims;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController(IAuthRepository repo, TokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserForRegisterDto dto)
    {
        var username = dto.Username.Trim().ToLowerInvariant();
        if (await repo.UserExists(username))
            return BadRequest("Username already exists");

        await repo.Register(new User { Username = username }, dto.Password);
        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserForLoginDto dto)
    {
        var user = await repo.Login(dto.Username.Trim().ToLowerInvariant(), dto.Password);
        if (user == null)
            return Unauthorized();

        return Ok(new { token = tokens.CreateToken(user) });
    }

    /// <summary>Returns the logged-in user's name; handy for checking a token.</summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new { username = User.FindFirstValue(ClaimTypes.Name) });
}
