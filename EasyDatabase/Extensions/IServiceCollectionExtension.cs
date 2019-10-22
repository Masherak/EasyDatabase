using EasyDatabase.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EasyDatabase.Extensions
{
    public static class IServiceCollectionExtension
    {
        public static void UseEasyDatabase(this IServiceCollection services, Configuration configuration = null)
        {
            services.AddSingleton(configuration ?? new Configuration());
            services.AddSingleton<DocumentRepository>();
            services.AddSingleton<DocumentService>();
            services.AddSingleton<Storage>();
        }
    }
}
