using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TriadIdentity.Infrastructure.Services;

namespace TriadIdentity.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient<ApiClientService>(client =>
            {
                client.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]);
            });
            return services;
        }
    }
}