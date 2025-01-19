using Microsoft.AspNetCore.Mvc;
using CodeGenie.Models;

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

    [HttpPost]
    public async Task<IActionResult> CreateUser(User user)
    {
        await _userService.AddUserAsync(user);
        return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, user);
    }
}
