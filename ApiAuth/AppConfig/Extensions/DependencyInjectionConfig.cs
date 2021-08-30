using ApiAuth.Security;
using Microsoft.Extensions.DependencyInjection;

namespace ApiAuth.AppConfig.Extensions
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection ConfigureDependencyInjection(this IServiceCollection services)
        {
            services.AddScoped<AccessManager>();

            return services;
        }
    }
}
