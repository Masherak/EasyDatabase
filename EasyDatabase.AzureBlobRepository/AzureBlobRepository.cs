using EasyDatabase.Core.Interfaces;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyDatabase.AzureBlobRepository
{
    public class AzureBlobRepository : IRepository
    {
        private readonly CloudBlobContainer _blobContainer;
        private readonly string _containerName;

        public AzureBlobRepository(string connectionString, string containerName)
        {
            _containerName = containerName ?? throw new ArgumentNullException(nameof(containerName));

            var storageAccount = CloudStorageAccount.Parse(connectionString);

            var cloudBlobClient = storageAccount.CreateCloudBlobClient();

            _blobContainer = cloudBlobClient.GetContainerReference(_containerName);
        }

        public async Task<IEnumerable<T>> ReadEntities<T>() where T : IEntity
        {
            // TODO: use IAsyncEnumerable?
            var result = new List<T>();

            var directory = _blobContainer.GetDirectoryReference(typeof(T).FullName);

            var resultSegment = await directory.ListBlobsSegmentedAsync(new BlobContinuationToken());

            do
            {
                foreach (CloudBlockBlob blob in resultSegment.Results)
                {
                    var json = await blob.DownloadTextAsync();

                    result.Add(JsonConvert.DeserializeObject<T>(json));
                }

                resultSegment = await directory.ListBlobsSegmentedAsync(resultSegment.ContinuationToken);
            }
            while (resultSegment.ContinuationToken != null);

            return result;
        }

        public async Task<T> ReadEntity<T>(Guid id) where T : IEntity
        {
            var blob = GetBlob<T>(id);

            return JsonConvert.DeserializeObject<T>(await blob.DownloadTextAsync());
        }

        public async Task WriteEntity<T>(T entity) where T : IEntity
        {
            var blob = GetBlob<T>(entity.Id);

            if (await blob.ExistsAsync())
            {
                await DeleteEntity<T>(entity.Id);
            }

            await blob.UploadTextAsync(JsonConvert.SerializeObject(entity));
        }

        public async Task DeleteEntity<T>(Guid id) where T : IEntity
        {
            var blob = GetBlob<T>(id);

            await blob.DeleteIfExistsAsync();
        }

        private CloudBlockBlob GetBlob<T>(Guid id) => _blobContainer.GetBlockBlobReference($"{typeof(T).FullName}/{id}.json");
    }
}
