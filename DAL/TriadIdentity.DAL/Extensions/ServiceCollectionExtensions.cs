using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TriadIdentity.DAL.Contexts;
using TriadIdentity.DAL.Entities.Identity;
using TriadIdentity.DAL.Interfaces;
using TriadIdentity.DAL.Repositories;

namespace TriadIdentity.DAL.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                    sqlOptions.EnableRetryOnFailure()));

            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddUserValidator<UserValidator<User>>()
                .AddPasswordValidator<PasswordValidator<User>>()
                .AddRoleValidator<RoleValidator<Role>>();

            services.AddScoped<IUserValidator<User>, UserValidator<User>>();
            services.AddScoped<IPasswordValidator<User>, PasswordValidator<User>>();
            services.AddScoped<IRoleValidator<Role>, RoleValidator<Role>>();

            services.AddScoped<IUserValidator<User>[]>(provider =>
                new[] { provider.GetService<IUserValidator<User>>() });
            services.AddScoped<IRoleValidator<Role>[]>(provider =>
                new[] { provider.GetService<IRoleValidator<Role>>() });

            return services;
        }
    }
}