using System.ComponentModel.DataAnnotations;

namespace TriadIdentity.BLL.Models.Identity;

public class LoginUserDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; }

    public bool RememberMe { get; set; }
}