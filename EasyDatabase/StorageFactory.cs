using EasyDatabase.Configurations;
using EasyDatabase.Interfaces;
using EasyDatabase.Repositories;
using EasyDatabase.Services;

namespace EasyDatabase
{
    public static class StorageFactory
    {
        public static Storage GetStorage(CacheConfiguration cacheConfiguration = null, IRepository repository = null)
        {
            if(cacheConfiguration == null)
            {
                cacheConfiguration = new CacheConfiguration();
            }

            if(repository == null)
            {
                repository = new FileRepository();
            }

            return new Storage(new Service(repository, cacheConfiguration));
        }
    }
}
