using EasyDatabase.Core.Configurations;
using EasyDatabase.Core.Interfaces;
using EasyDatabase.Core.Services;

namespace EasyDatabase.Core
{
    public static class StorageFactory
    {
        public static Storage GetStorage(IRepository repository, CacheConfiguration cacheConfiguration)
        {
            return new Storage(new Service(repository, cacheConfiguration));
        }
    }
}
