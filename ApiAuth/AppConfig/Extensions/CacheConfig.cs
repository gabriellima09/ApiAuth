using Microsoft.Extensions.DependencyInjection;

namespace ApiAuth.AppConfig.Extensions
{
    public static class CacheConfig
    {
        public static IServiceCollection ConfigureCache(this IServiceCollection services)
        {
            // Cache using MemoryCache
            services.AddDistributedMemoryCache();

            // Cache using Redis
            //services.AddDistributedRedisCache(options =>
            //{
            //    options.Configuration =
            //        Configuration.GetConnectionString("ConexaoRedis");
            //    options.InstanceName = "ApiAuth";
            //});

            return services;
        }
    }
}
