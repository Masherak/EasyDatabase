namespace EasyDatabase
{
    public class Configuration
    {
        public Configuration(double? cacheSlidingExpirationTimeInHours = null)
        {
            CacheSlidingExpirationTimeInHours = cacheSlidingExpirationTimeInHours;
        }

        public double? CacheSlidingExpirationTimeInHours { get; }
    }
}