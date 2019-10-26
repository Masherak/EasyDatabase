using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyDatabase.Configurations;
using EasyDatabase.Enums;
using EasyDatabase.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EasyDatabase.Services
{
    public class Service
    {
        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly IRepository _repository;
        private readonly CacheConfiguration _cacheConfiguration;

        public Service(IRepository repository, CacheConfiguration cacheConfiguration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _cacheConfiguration = cacheConfiguration ?? throw new ArgumentNullException(nameof(cacheConfiguration));
        }

        public async Task AddOrUpdateEntities<T>(IEnumerable<T> entities) where T : IEntity
        {
            await Task.WhenAll(entities.Select(async _ => await _repository.WriteEntity(_)));

            if (_cacheConfiguration.IsEnabled)
            {
                foreach (var entity in entities)
                {
                    InvalidateCache(entity.Id);
                }
            }
        }

        public async Task AddOrUpdateEntities<T>(T entity) where T : IEntity
        {
            await AddOrUpdateEntities(new[] { entity });
        }

        public async Task DeleteEntities<T>(IEnumerable<Guid> ids) where T : IEntity
        {
            await Task.WhenAll(ids.Select(async _ => await _repository.DeleteEntity<T>(_)));

            if (_cacheConfiguration.IsEnabled)
            {
                foreach(var id in ids)
                {
                    InvalidateCache(id);
                }
            }
        }

        public async Task DeleteEntity<T>(Guid id) where T : IEntity
        {
            await DeleteEntities<T>(new[] { id });
        }

        public async Task<IEnumerable<T>> ReadEntities<T>() where T : IEntity
        {
            return await _repository.ReadEntities<T>();
        }

        public async Task<T> ReadEntity<T>(Guid id) where T : IEntity
        {
            if(!_cacheConfiguration.IsEnabled)
            {
                return await _repository.ReadEntity<T>(id);
            }

            return await _cache.GetOrCreateAsync(id, async _ =>
            {
                if (_cacheConfiguration.Type == CacheType.Absolute)
                {
                    _.SetAbsoluteExpiration(_cacheConfiguration.Offset.Value);
                }
                else if(_cacheConfiguration.Type == CacheType.Sliding)
                {
                    _.SetSlidingExpiration(_cacheConfiguration.Offset.Value);
                }
                else
                {
                    throw new ArgumentOutOfRangeException(_cacheConfiguration.Type.ToString());
                }

                return await _repository.ReadEntity<T>(id);
            });
        }

        private void InvalidateCache(Guid id)
        {
            _cache.Remove(id);
        }
    }
}