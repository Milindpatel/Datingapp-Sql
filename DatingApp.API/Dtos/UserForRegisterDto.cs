using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos;

public class UserForRegisterDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = "";

    [Required]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 64 characters")]
    public string Password { get; set; } = "";
}
