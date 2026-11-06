using System.ComponentModel.DataAnnotations;

namespace TriadIdentity.UI.Api.Options
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        [Required(ErrorMessage = "Jwt:Key is missing. Run scripts/init-secrets.ps1 or set the Jwt__Key environment variable.")]
        [MinLength(32)]
        public string Key { get; set; } = string.Empty;

        [Required]
        public string Issuer { get; set; } = string.Empty;

        [Required]
        public string Audience { get; set; } = string.Empty;

        [Range(1, 1440)]
        public int ExpiryMinutes { get; set; } = 60;
    }
}
