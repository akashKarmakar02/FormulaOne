using System.ComponentModel.DataAnnotations;

public class UserRegistrationRequestDto
{
    [Required]
    public required string Name { get; set; }

    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}