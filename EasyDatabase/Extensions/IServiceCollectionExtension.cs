using EasyDatabase.Configurations;
using EasyDatabase.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDatabase.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static void UseEasyDatabase(this IServiceCollection services, CacheConfiguration cacheConfiguration = null, IRepository repository = null)
        {
            services.AddSingleton(_ => StorageFactory.GetStorage(cacheConfiguration, repository));
        }
    }
}
