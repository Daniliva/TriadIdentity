using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TriadIdentity.DAL.Entities.Common;
using TriadIdentity.DAL.Entities.Identity;
using TriadIdentity.DAL.Interfaces;

namespace TriadIdentity.BLL.Services.Common
{
    public class CustomRoleManager : RoleManager<Role>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomRoleManager(
            IRoleStore<Role> store,
            IRoleValidator<Role>[] roleValidators,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            ILogger<RoleManager<Role>> logger,
            IUnitOfWork unitOfWork)
            : base(store, roleValidators, keyNormalizer, errors, logger)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<IdentityResult> CreateAsync(Role role)
        {
            var result = await base.CreateAsync(role);
            if (result.Succeeded)
            {
                await _unitOfWork.Repository<LogEntry>().AddAsync(new LogEntry
                {
                    Action = "RoleCreated",
                    UserId = null,
                    Details = $"Role {role.Name} created at {DateTime.UtcNow}."
                });
                await _unitOfWork.SaveChangesAsync();
            }
            return result;
        }
    }
}
