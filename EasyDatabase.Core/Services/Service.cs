using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyDatabase.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EasyDatabase.Core.Services
{
    public class Service
    {
        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly IRepository _repository;

        public Service(IRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task AddOrUpdateEntities<T>(IEnumerable<T> entities) where T : IEntity
        {
            await Task.WhenAll(entities.Select(async _ => await _repository.WriteEntity(_)));
        }

        public async Task AddOrUpdateEntities<T>(T entity) where T : IEntity
        {
            await AddOrUpdateEntities(new[] { entity });
        }

        public async Task DeleteEntities<T>(IEnumerable<Guid> ids) where T : IEntity
        {
            await Task.WhenAll(ids.Select(async _ => await _repository.DeleteEntity<T>(_)));
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
            return await _repository.ReadEntity<T>(id);
        }
    }
}