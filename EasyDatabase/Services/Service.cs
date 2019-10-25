using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyDatabase.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EasyDatabase.Services
{
    public class Service
    {
        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly IRepository _repository;
        private readonly Configuration _configuration;
        private readonly bool _cacheIsEnabled;

        public Service(IRepository repository, Configuration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _cacheIsEnabled = configuration.CacheSlidingExpirationTimeInHours.HasValue;
        }

        public async Task AddOrUpdateEntities<T>(IEnumerable<T> entities) where T : IEntity
        {
            await Task.WhenAll(entities.Select(async _ => await _repository.WriteEntity(_)));

            if (_cacheIsEnabled)
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

        public async Task DeleteEntities<T>(IEnumerable<Guid> ids)
        {
            await Task.WhenAll(ids.Select(async _ => await _repository.DeleteEntity<T>(_)));

            if (_cacheIsEnabled)
            {
                foreach(var id in ids)
                {
                    InvalidateCache(id);
                }
            }
        }

        public async Task DeleteEntity<T>(Guid id)
        {
            await DeleteEntities<T>(new[] { id });
        }

        public async Task<IEnumerable<T>> ReadEntities<T>() where T : IEntity
        {
            return await _repository.ReadEntities<T>();
        }

        public async Task<T> ReadEntity<T>(Guid id) where T : IEntity
        {
            if(!_cacheIsEnabled)
            {
                return await _repository.ReadEntity<T>(id);
            }

            return await _cache.GetOrCreateAsync(id, async _ =>
            {
                _.SetSlidingExpiration(TimeSpan.FromHours(_configuration.CacheSlidingExpirationTimeInHours.Value));

                return await _repository.ReadEntity<T>(id);
            });
        }

        private void InvalidateCache(Guid id)
        {
            _cache.Remove(id);
        }
    }
}