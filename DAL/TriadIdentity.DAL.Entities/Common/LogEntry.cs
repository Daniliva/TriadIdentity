using System.ComponentModel.DataAnnotations;

namespace TriadIdentity.DAL.Entities.Common
{
    public class LogEntry : IAuditEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Action { get; set; }

        [MaxLength(50)]
        public string? UserId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [Required]
        [MaxLength(500)]
        public string Details { get; set; }

        public LogEntry()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }
    }
}