using TriadIdentity.BLL.Interfaces.Identity;
using TriadIdentity.BLL.Models.DTOs.Identity;
using TriadIdentity.DAL.Entities.Common;
using TriadIdentity.DAL.Interfaces;

namespace TriadIdentity.BLL.Services.Identity
{
    public class LogService : ILogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task LogActionAsync(string action, string userId, string details)
        {
            await _unitOfWork.Repository<LogEntry>().AddAsync(new LogEntry
            {
                Action = action,
                UserId = userId,
                Details = details
            });
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<LogEntryDto>> GetLogsByUserIdAsync(string userId)
        {
            var logs = await _unitOfWork.Repository<LogEntry>().FindAsync(l => l.UserId == userId);
            return logs.AsEnumerable().Select(l => new LogEntryDto
            {
                Id = l.Id,
                Action = l.Action,
                UserId = l.UserId,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
                Details = l.Details
            }).ToList();
        }

        public async Task<IEnumerable<LogEntryDto>> GetAllLogsAsync()
        {
            var logs = await _unitOfWork.Repository<LogEntry>().GetAllAsync();
            return logs.AsEnumerable().Select(l => new LogEntryDto
            {
                Id = l.Id,
                Action = l.Action,
                UserId = l.UserId,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
                Details = l.Details
            }).ToList();
        }
    }
}
