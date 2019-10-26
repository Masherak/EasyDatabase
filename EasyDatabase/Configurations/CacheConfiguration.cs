using EasyDatabase.Enums;
using System;

namespace EasyDatabase.Configurations
{
    public class CacheConfiguration
    {
        public CacheConfiguration(CacheType? type = null, TimeSpan? offset = null)
        {
            Type = type;
            Offset = offset;
        }

        public bool IsEnabled => Type.HasValue && Offset.HasValue;

        public CacheType? Type { get; }

        public TimeSpan? Offset { get; }
    }
}