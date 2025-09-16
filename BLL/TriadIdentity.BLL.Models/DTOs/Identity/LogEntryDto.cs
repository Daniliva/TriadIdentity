namespace TriadIdentity.BLL.Models.DTOs.Identity
{
    public class LogEntryDto
    {
        public Guid Id { get; set; }
        public string Action { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string Details { get; set; } = null!;
    }
}