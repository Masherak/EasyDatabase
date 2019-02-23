using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EasyDatabase.Interfaces;

namespace EasyDatabase.Services
{
    public class DocumentService
    {
        private readonly DocumentRepository _documentRepository;

        public DocumentService(Configuration configuration)
        {
            _documentRepository = new DocumentRepository(configuration ?? new Configuration());
        }

        public async Task AddOrUpdateEntities<T>(IEnumerable<T> entities) where T : IEntity
        {
            await Task.WhenAll(entities.Select(async _ => await _documentRepository.WriteEntity(_)));
        }

        public async Task AddOrUpdateEntities<T>(T entity) where T : IEntity
        {
            await AddOrUpdateEntities<T>(new[] {entity});
        }

        public async Task DeleteEntities<T>(IEnumerable<Guid> ids)
        {
            await Task.WhenAll(ids.Select(async _ => await _documentRepository.DeleteEntity<T>(_)));
        }

        public async Task DeleteEntity<T>(Guid id)
        {
            await DeleteEntities<T>(new[] {id});
        }

        public async Task<IEnumerable<T>> ReadEntities<T>() where T : IEntity
        {
            var dirInfo = new DirectoryInfo(_documentRepository.GetPath(typeof(T)));
            var fileInfos = dirInfo.GetFiles($"*{Configuration.FileNameSuffix}", SearchOption.TopDirectoryOnly);

            return await Task.WhenAll(fileInfos.Select(async _ => await _documentRepository.ReadEntity<T>(_.Name)));
        }

        public async Task<T> ReadEntity<T>(Guid id) where T : IEntity
        {
            return await _documentRepository.ReadEntity<T>(DocumentRepository.GetFileName(id));
        }
    }
}