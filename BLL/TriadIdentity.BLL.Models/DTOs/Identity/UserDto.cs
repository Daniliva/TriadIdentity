using System.ComponentModel.DataAnnotations;

namespace TriadIdentity.BLL.Models.DTOs.Identity
{
    public class UserDto
    {
        [Required(ErrorMessage = "User ID is required.")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string LastName { get; set; }
    }
    public class UserClaimDto
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Claim type is required")]
        [StringLength(100, ErrorMessage = "Claim type cannot exceed 100 characters")]
        public string ClaimType { get; set; }

        [Required(ErrorMessage = "Claim value is required")]
        [StringLength(100, ErrorMessage = "Claim value cannot exceed 100 characters")]
        public string ClaimValue { get; set; }
    }
}