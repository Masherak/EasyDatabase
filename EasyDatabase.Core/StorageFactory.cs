using EasyDatabase.Core.Interfaces;
using EasyDatabase.Core.Services;

namespace EasyDatabase.Core
{
    public static class StorageFactory
    {
        public static Storage GetStorage(IRepository repository)
        {
            return new Storage(new Service(repository));
        }
    }
}
