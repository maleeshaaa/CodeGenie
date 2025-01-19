using Microsoft.AspNetCore.Mvc;
using CodeGenie.Models;
using CodeGenie.Services;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    // POST: api/user/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var result = await _userService.RegisterUserAsync(model.Username, model.Email, model.Password);
        if (result == "User registered successfully.")
            return Ok(result);

        return BadRequest(result);
    }

    // POST: api/user/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var result = await _userService.LoginUserAsync(model.Username, model.Password);
        if (result.StartsWith("Bearer"))
            return Ok(new { Token = result });

        return Unauthorized(result);
    }
}
