using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TriadIdentity.DAL.Entities.Common;
using TriadIdentity.DAL.Entities.Identity;
using TriadIdentity.DAL.Interfaces;
 
namespace TriadIdentity.DAL.Contexts
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, Guid, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>, IApplicationDbContext
    {
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        public DbSet<LogEntry> LogEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(entity => entity.ToTable("Users"));
            builder.Entity<Role>(entity => entity.ToTable("Roles"));
            builder.Entity<UserRole>(entity => entity.ToTable("UserRoles"));
            builder.Entity<UserClaim>(entity => entity.ToTable("UserClaims"));
            builder.Entity<UserLogin>(entity => entity.ToTable("UserLogins"));
            builder.Entity<UserToken>(entity => entity.ToTable("UserTokens"));
            builder.Entity<RoleClaim>(entity => entity.ToTable("RoleClaims"));
            builder.Entity<LogEntry>(entity => entity.ToTable("Logs"));

            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        }

        private void ApplyAuditAndLogging()
        {
            var logsToAdd = new List<LogEntry>();

            foreach (var entry in ChangeTracker.Entries<IAuditEntity>().ToList())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var entityType = entry.Entity.GetType().Name;
                    var userId = entry.Entity is User user ? user.Id.ToString() : "System";
                    var details = entry.Entity is User u
                        ? $"User {u.Email} {entry.State} at {DateTime.UtcNow}"
                        : $"{entityType} {entry.State} at {DateTime.UtcNow}";

                    logsToAdd.Add(new LogEntry
                    {
                        Action = $"{entityType}_{entry.State}",
                        UserId = userId,
                        Details = details
                    });

                    _logger.LogInformation("Entity {EntityType} changed with state {State} at {Time}",
                        entityType, entry.State, DateTime.UtcNow);
                }
            }

            if (logsToAdd.Any())
            {
                LogEntries.AddRange(logsToAdd);
            }
        }


        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditAndLogging();

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to database at {Time}", DateTime.UtcNow);
                throw;
            }
        }

        public override int SaveChanges()
        {
            ApplyAuditAndLogging();

            try
            {
                return base.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving changes to database at {Time}", DateTime.UtcNow);
                throw;
            }
        }
    }
}
