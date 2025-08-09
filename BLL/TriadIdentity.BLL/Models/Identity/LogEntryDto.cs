namespace TriadIdentity.BLL.Models.Identity;

public class LogEntryDto
{
    public Guid Id { get; set; }
    public string Action { get; set; }
    public string UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Details { get; set; }
}