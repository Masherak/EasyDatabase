using EasyDatabase.Interfaces;
using EasyDatabase.Repositories;
using EasyDatabase.Services;

namespace EasyDatabase
{
    public static class StorageFactory
    {
        public static Storage GetStorage(Configuration configuration = null, IRepository repository = null)
        {
            if(configuration == null)
            {
                configuration = new Configuration();
            }

            if(repository == null)
            {
                repository = new FileRepository();
            }

            return new Storage(new Service(repository, configuration));
        }
    }
}
