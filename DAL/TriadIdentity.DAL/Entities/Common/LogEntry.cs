namespace TriadIdentity.DAL.Entities.Common;

public class LogEntry :IAuditEntity
{
    public Guid Id { get; set; }
    public string Action { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Details { get; set; }

    public LogEntry()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
}
public interface IAuditEntity
{
    DateTime CreatedAt { get; set; }

    DateTime? UpdatedAt { get; set; }
}