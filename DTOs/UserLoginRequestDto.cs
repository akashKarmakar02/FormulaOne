using System.ComponentModel.DataAnnotations;

public class UserLoginRequestDto
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}