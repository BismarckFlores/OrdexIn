using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdexIn.Models;
using OrdexIn.Models.DTO;
using OrdexIn.Services.Intefaces;

namespace OrdexIn.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IAuthService _authService;
    
    public UsersController(ILogger<UsersController> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }
    
    // GET: api/users
    [HttpGet]
    public async Task<IActionResult> GetUsersAsync()
    {
        var profiles = await _authService.GetAllUsersAsync();

        foreach (var profile in profiles)
        {
            _logger.LogInformation("User: {UserId}, Email: {Email}, IsAdmin: {IsAdmin}", profile.UserId, profile.Email, profile.IsAdmin);
        }
        
        var users = profiles.Select(p => new ProfileDto
            {
                UserId = p.UserId,
                Email = p.Email,
                Role = p.IsAdmin ? "admin" : "user"
            }
        ).ToList();
        
        return Ok(users);
    }
    
    // POST: api/users
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Invalid payload: email are required.");

        var defaultPassword = "66426705.Oi";

        try
        {
            var userModel = new UserModel { Email = dto.Email, Password = defaultPassword };
            var session = await _authService.RegisterUserAsync(userModel, dto.IsAdmin);

            return session?.User == null ? StatusCode(500, "Failed to create user in auth provider.") : Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}