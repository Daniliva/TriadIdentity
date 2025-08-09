using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TriadIdentity.DAL.Entities.Identity;
using TriadIdentity.DAL.Interfaces;

namespace TriadIdentity.BLL.Services.Common
{
    public class CustomUserManager : UserManager<User>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomUserManager(
            IUserStore<User> store,
            IOptions<IdentityOptions> optionsAccessor,
            IPasswordHasher<User> passwordHasher,
            IUserValidator<User>[] userValidators,
            IPasswordValidator<User> passwordValidator,
            ILookupNormalizer keyNormalizer,
            IdentityErrorDescriber errors,
            IServiceProvider services,
            ILogger<UserManager<User>> logger,
            IUnitOfWork unitOfWork)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidator, keyNormalizer, errors, services, logger)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<IdentityResult> CreateAsync(User user, string password)
        {
            var result = await base.CreateAsync(user, password);
            if (result.Succeeded)
            {
                await _unitOfWork.Repository<LogEntry>().AddAsync(new LogEntry
                {
                    Action = "UserCreated",
                    UserId = user.Id.ToString(),
                    Details = $"User {user.Email} created at {DateTime.UtcNow}."
                });
                await _unitOfWork.SaveChangesAsync();
            }
            return result;
        }

        public override async Task<IdentityResult> UpdateAsync(User user)
        {
            var result = await base.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _unitOfWork.Repository<LogEntry>().AddAsync(new LogEntry
                {
                    Action = "UserUpdated",
                    UserId = user.Id.ToString(),
                    Details = $"User {user.Email} updated at {DateTime.UtcNow}."
                });
                await _unitOfWork.SaveChangesAsync();
            }
            return result;
        }
    }
}
