using TriadIdentity.BLL.Models.DTOs.Common;

namespace TriadIdentity.BLL.Interfaces.Common
{
	public interface ILogService
    {
        Task LogActionAsync(string action, string userId, string details);
        Task<IEnumerable<LogEntryDto>> GetUserLogsAsync(string userId);
    }
}
