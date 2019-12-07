using EasyDatabase.Core;
using EasyDatabase.Core.Configurations;
using EasyDatabase.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDatabase.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static void UseEasyDatabase(this IServiceCollection services, IRepository repository, CacheConfiguration cacheConfiguration)
        {
            services.AddSingleton(_ => StorageFactory.GetStorage(repository, cacheConfiguration));
        }
    }
}
