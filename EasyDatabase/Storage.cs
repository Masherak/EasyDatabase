using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyDatabase.Interfaces;
using EasyDatabase.Services;

namespace EasyDatabase
{
    public class Storage
    {
        private readonly Service _documentService;

        public Storage(Service documentService)
        {
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
        }

        public async Task AddOrUpdate<T>(T entity) where T : IEntity
        {
            await _documentService.AddOrUpdateEntities(entity);
        }

        public async Task AddOrUpdate<T>(IEnumerable<T> entities) where T : IEntity
        {
            await _documentService.AddOrUpdateEntities(entities);
        }

        public async Task Delete<T>(Guid id) where T : IEntity
        {
            await _documentService.DeleteEntity<T>(id);
        }

        public async Task Delete<T>(IEnumerable<Guid> ids) where T : IEntity
        {
            await _documentService.DeleteEntities<T>(ids);
        }

        public async Task Delete<T>(IEntity entity) where T : IEntity
        {
            await Delete<T>(entity.Id);
        }

        public async Task Delete<T>(IEnumerable<IEntity> entities) where T : IEntity
        {
            await Delete<T>(entities.Select(_ => _.Id));
        }

        public async Task<IEnumerable<T>> Get<T>() where T : IEntity
        {
            return await _documentService.ReadEntities<T>();
        }

        public async Task<T> Get<T>(Guid id) where T : IEntity
        {
            return await _documentService.ReadEntity<T>(id);
        }
    }
}