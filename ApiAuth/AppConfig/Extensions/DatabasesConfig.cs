using ApiAuth.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApiAuth.AppConfig.Extensions
{
    public static class DatabasesConfig
    {
        public static IServiceCollection ConfigureDatabases(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure database context 
            services.AddDbContext<ApplicationDbContext>(options =>
                // Configure database context InMemory
                //options.UseInMemoryDatabase("Auth"));
                options.UseSqlServer(configuration.GetConnectionString("Auth")));

            // EF Core Filter for developers exceptions -> shorturl.at/iyJKU
            services.AddDatabaseDeveloperPageExceptionFilter();

            return services;
        }
    }
}
