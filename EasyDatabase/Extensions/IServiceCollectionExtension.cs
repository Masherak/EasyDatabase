using EasyDatabase.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDatabase.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static void UseEasyDatabase(this IServiceCollection services, Configuration configuration = null, IRepository repository = null)
        {
            services.AddSingleton(_ => StorageFactory.GetStorage(configuration, repository));
        }
    }
}
