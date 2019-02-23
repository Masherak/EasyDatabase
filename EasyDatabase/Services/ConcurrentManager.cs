using System.Threading;

namespace EasyDatabase.Services
{
    public static class ConcurrentManager
    {
        public static readonly SemaphoreSlim DocumentLock = new SemaphoreSlim(1);
    }
}
