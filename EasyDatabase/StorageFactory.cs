using EasyDatabase.Services;

namespace EasyDatabase
{
    public static class StorageFactory
    {
        public static Storage GetStorage(Configuration configuration = null)
        {
            if(configuration == null)
            {
                configuration = new Configuration();
            }

            return new Storage(new DocumentService(new DocumentRepository(configuration), configuration));
        }
    }
}
