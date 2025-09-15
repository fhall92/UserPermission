using Microsoft.AspNetCore.Mvc;
using UserPermission.Core.DTOs;
using UserPermission.Core.Interfaces;

namespace UserPermission.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    public AuthController(IUserService userService) => _userService = userService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (dto == null)
            return BadRequest(new { Message = "Login data is required." });

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userService.AuthenticateAsync(dto);
        if (user == null)
            return Unauthorized();

        return Ok(user);
    }
}
