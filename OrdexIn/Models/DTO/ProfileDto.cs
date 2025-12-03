namespace OrdexIn.Models.DTO;

public class ProfileDto
{
    public Guid UserId { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
}