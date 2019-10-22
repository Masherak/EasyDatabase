using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EasyDatabase.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace EasyDatabase.Services
{
    public class DocumentService
    {
        private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        private readonly DocumentRepository _documentRepository;
        private readonly Configuration _configuration;
        private readonly bool _cacheIsEnabled;

        public DocumentService(DocumentRepository documentRepository, Configuration configuration)
        {
            _documentRepository = documentRepository ?? throw new ArgumentNullException(nameof(documentRepository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _cacheIsEnabled = configuration.CacheSlidingExpirationTimeInHours.HasValue;
        }

        public async Task AddOrUpdateEntities<T>(IEnumerable<T> entities) where T : IEntity
        {
            await Task.WhenAll(entities.Select(async _ => await _documentRepository.WriteEntity(_)));

            if (_cacheIsEnabled)
            {
                foreach (var entity in entities)
                {
                    InvalidateCache(DocumentRepository.GetFileName(entity.Id));
                }
            }
        }

        public async Task AddOrUpdateEntities<T>(T entity) where T : IEntity
        {
            await AddOrUpdateEntities(new[] { entity });
        }

        public async Task DeleteEntities<T>(IEnumerable<Guid> ids)
        {
            await Task.WhenAll(ids.Select(async _ => await _documentRepository.DeleteEntity<T>(_)));

            if (_cacheIsEnabled)
            {
                foreach(var id in ids)
                {
                    InvalidateCache(DocumentRepository.GetFileName(id));
                }
            }
        }

        public async Task DeleteEntity<T>(Guid id)
        {
            await DeleteEntities<T>(new[] { id });
        }

        public async Task<IEnumerable<T>> ReadEntities<T>() where T : IEntity
        {
            var dirInfo = new DirectoryInfo(_documentRepository.GetPath(typeof(T)));
            var fileInfos = dirInfo.GetFiles($"*{Configuration.FileNameSuffix}", SearchOption.TopDirectoryOnly);

            return await Task.WhenAll(fileInfos.Select(async _ => await ReadEntity<T>(_.Name)));
        }

        public async Task<T> ReadEntity<T>(Guid id) where T : IEntity
        {
            return await ReadEntity<T>(DocumentRepository.GetFileName(id));
        }

        private async Task<T> ReadEntity<T>(string fileName) where T : IEntity
        {
            if(!_cacheIsEnabled)
            {
                return await _documentRepository.ReadEntity<T>(fileName);
            }

            return await _cache.GetOrCreateAsync(fileName, async _ =>
            {
                _.SetSlidingExpiration(TimeSpan.FromHours(_configuration.CacheSlidingExpirationTimeInHours.Value));

                return await _documentRepository.ReadEntity<T>(fileName);
            });
        }

        private void InvalidateCache(string fileName)
        {
            _cache.Remove(fileName);
        }
    }
}