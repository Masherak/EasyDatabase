using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EasyDatabase.Interfaces;

namespace EasyDatabase.Services
{
    public class DocumentRepository
    {
        private readonly Configuration _configuration;

        public DocumentRepository(Configuration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<T> ReadEntity<T>(string fileName) where T : IEntity
        {
            await ConcurrentManager.DocumentLock.WaitAsync();
            try
            {
                var path = GetPath(typeof(T), fileName);

                if (!File.Exists(path))
                {
                    throw new ArgumentException($"The file does not exist on the path {path}");
                }

                using (var reader = File.OpenText(path))
                {
                    return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync(), _configuration.JsonSerializerSettings);
                }
            }
            catch (Exception e)
            {
                e.Data.Add("Type", typeof(T));
                e.Data.Add("FileName", fileName);

                throw;
            }
            finally
            {
                ConcurrentManager.DocumentLock.Release();
            }
        }

        public async Task WriteEntity<T>(T entity) where T : IEntity
        {
            if (entity.Id == Guid.Empty)
            {
                throw new ArgumentException($"The id cannot be an empty GUID");
            }

            await ConcurrentManager.DocumentLock.WaitAsync();
            try
            {
                Directory.CreateDirectory(GetPath(typeof(T)));
                using (var stream = new FileStream(GetPath(typeof(T), entity.Id), FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                using (var sw = new StreamWriter(stream, _configuration.Encoding))
                {
                    await sw.WriteLineAsync(JsonConvert.SerializeObject(entity, _configuration.JsonSerializerSettings));
                }
            }
            catch (Exception e)
            {
                e.Data.Add("Type", typeof(T));
                e.Data.Add("Entity", JsonConvert.SerializeObject(entity, _configuration.JsonSerializerSettings));
            }
            finally
            {
                ConcurrentManager.DocumentLock.Release();
            }
        }

        public async Task DeleteEntity<T>(Guid id)
        {
            await ConcurrentManager.DocumentLock.WaitAsync();
            try
            {
                var path = GetPath(typeof(T), id);

                if (!File.Exists(path))
                {
                    throw new ArgumentException($"The file does not exist on the path {path}");
                }

                using (var stream = new FileStream(path, FileMode.Truncate, FileAccess.Write, FileShare.Delete, 4096, true))
                {
                    await stream.FlushAsync();
                }

                File.Delete(path);
            }
            catch (Exception e)
            {
                e.Data.Add("Type", typeof(T));
                e.Data.Add("Id", id);

                throw;
            }
            finally
            {
                ConcurrentManager.DocumentLock.Release();
            }
        }

        public static string GetFileName(Guid id)
        {
            return string.Concat(id, Configuration.FileNameSuffix);
        }

        public string GetPath(Type type)
        {
            return Path.Combine(_configuration.DocumentsPath, type?.Namespace ?? throw new ArgumentNullException($"The namespace of type {nameof(type)} cannot be a null"), type.Name);
        }

        private string GetPath(Type type, string fileName)
        {
            return Path.Combine(GetPath(type), fileName ?? throw new ArgumentNullException($"The filename cannot be a null"));
        }

        private string GetPath(Type type, Guid id)
        {
            return GetPath(type, GetFileName(id));
        }
    }
}