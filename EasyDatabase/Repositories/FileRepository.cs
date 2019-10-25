using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EasyDatabase.Interfaces;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EasyDatabase.Managers;

namespace EasyDatabase.Repositories
{
    public class FileRepository : IRepository
    {
        private const string DefaultFolderName = "EasyDatabase";
        private const string FileNameSuffix = ".json";

        private readonly string _path;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public FileRepository()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly()?.Location ?? throw new InvalidOperationException("Assembly location is not available");
            _path = Path.Combine(Path.GetDirectoryName(assemblyLocation), DefaultFolderName);

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
        }

        public async Task<T> ReadEntity<T>(Guid id) where T : IEntity
        {
            await ConcurrentManager.DocumentLock.WaitAsync();
            try
            {
                var path = GetPath(typeof(T), GetFileName(id));

                if (!File.Exists(path))
                {
                    throw new ArgumentException($"The file does not exist on the path {path}");
                }

                using (var reader = File.OpenText(path))
                {
                    return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync(), _jsonSerializerSettings);
                }
            }
            catch (Exception e)
            {
                e.Data.Add("Type", typeof(T));
                e.Data.Add("FileName", GetFileName(id));

                throw;
            }
            finally
            {
                ConcurrentManager.DocumentLock.Release();
            }
        }

        public async Task<T> ReadEntity<T>(string id) where T : IEntity
        {
            var guids = Regex.Matches(id, @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}")
                .Cast<Match>()
                .Where(_ => _.Success)
                .Select(_ => _.Value)
                .ToList();

            if (guids.Count != 1)
            {
                return default;
            }

            return await ReadEntity<T>(Guid.Parse(guids.Single()));
        }

        public async Task<IEnumerable<T>> ReadEntities<T>() where T : IEntity
        {
            var dirInfo = new DirectoryInfo(GetPath(typeof(T)));
            var fileInfos = dirInfo.GetFiles($"*{FileNameSuffix}", SearchOption.TopDirectoryOnly);

            return await Task.WhenAll(fileInfos.Select(async _ => await ReadEntity<T>(_.Name)));
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
                using (var sw = new StreamWriter(stream, Encoding.UTF8))
                {
                    await sw.WriteLineAsync(JsonConvert.SerializeObject(entity, _jsonSerializerSettings));
                }
            }
            catch (Exception e)
            {
                e.Data.Add("Type", typeof(T));
                e.Data.Add("Entity", JsonConvert.SerializeObject(entity, _jsonSerializerSettings));
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

        private static string GetFileName(Guid id)
        {
            return string.Concat(id, FileNameSuffix);
        }

        private string GetPath(Type type)
        {
            return Path.Combine(_path, type?.Namespace ?? throw new ArgumentNullException($"The namespace of type {nameof(type)} cannot be a null"), type.Name);
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