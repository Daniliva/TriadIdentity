using TriadIdentity.BLL.Models.Identity;

namespace TriadIdentity.BLL.Interfaces.Identity;

public interface ILogService
{
    Task LogActionAsync(string action, string userId, string details);
    Task<IEnumerable<LogEntryDto>> GetLogsByUserIdAsync(string userId);
    Task<IEnumerable<LogEntryDto>> GetAllLogsAsync();
}