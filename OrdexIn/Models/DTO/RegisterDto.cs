namespace OrdexIn.Models.DTO;

public class RegisterDto
{
    public required string Email { get; set; }
    public bool IsAdmin { get; set; } = false;
}