using Microsoft.EntityFrameworkCore;
using TriadIdentity.DAL.Entities.Common;

namespace TriadIdentity.DAL.Interfaces;

public interface IApplicationDbContext
{
    DbSet<LogEntry> LogEntries { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}