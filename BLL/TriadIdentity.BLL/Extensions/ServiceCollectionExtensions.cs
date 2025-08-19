using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TriadIdentity.BLL.Interfaces.Common;
using TriadIdentity.BLL.Interfaces.Identity;
using TriadIdentity.BLL.Profiles;
using TriadIdentity.BLL.Services.Common;
using TriadIdentity.BLL.Services.Identity;
using TriadIdentity.DAL.Entities.Identity;
using TriadIdentity.DAL.Extensions;

namespace TriadIdentity.BLL.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, string connectionString = null)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                services.AddDataAccessLayer(connectionString);
            }

            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<ILogService, LogService>();
            services.AddScoped<CustomUserManager>();
            services.AddScoped<UserManager<User>, CustomUserManager>();
            services.AddScoped<CustomRoleManager>();
            services.AddScoped<RoleManager<Role>, CustomRoleManager>();

            services.AddAutoMapper(config => config.AddProfile<MappingProfile>());

            return services;
        }
    }
}
