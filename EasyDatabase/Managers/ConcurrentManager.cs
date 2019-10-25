using System.Threading;

namespace EasyDatabase.Managers
{
    public static class ConcurrentManager
    {
        public static readonly SemaphoreSlim DocumentLock = new SemaphoreSlim(1);
    }
}
