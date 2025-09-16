using System.ComponentModel.DataAnnotations;

namespace TriadIdentity.BLL.Models.DTOs.Identity
{
    public class LoginUserDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        [RegularExpression(@"^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z]).*$",
            ErrorMessage = "Password must contain at least one digit, one lowercase letter, and one uppercase letter.")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}