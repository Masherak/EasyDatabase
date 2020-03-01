using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EasyDatabase.Core.Interfaces;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace EasyDatabase.FileRepository
{
    public class FileRepository : IRepository
    {
        private const string _defaultFolderName = "EasyDatabase";
        private const string _fileNameSuffix = ".json";
        private const string _guidRegex = @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}";

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private readonly string _path;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public FileRepository()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly()?.Location ?? throw new InvalidOperationException("Assembly location is not available");
            _path = Path.Combine(Path.GetDirectoryName(assemblyLocation), _defaultFolderName);

            Directory.CreateDirectory(_path);

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
        }

        public async Task<T> ReadEntity<T>(Guid id) where T : IEntity
        {
            await _semaphore.WaitAsync();
            try
            {
                var path = GetPath(typeof(T), GetFileName(id));

                if (!File.Exists(path))
                {
                    throw new ArgumentException($"The file does not exist on the path {path}");
                }

                using var reader = File.OpenText(path);
                return JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync(), _jsonSerializerSettings);
            }
            catch (Exception e)
            {
                e.Data.Add("Type", typeof(T));
                e.Data.Add("FileName", GetFileName(id));

                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IEnumerable<T>> ReadEntities<T>() where T : IEntity
        {
            var path = GetPath(typeof(T));

            Directory.CreateDirectory(path);

            var dirInfo = new DirectoryInfo(path);
            var fileInfos = dirInfo.GetFiles($"*{_fileNameSuffix}", SearchOption.TopDirectoryOnly);

            var ids = fileInfos.SelectMany(_ => Regex.Matches(_.Name, _guidRegex).Cast<Match>()).ToList();

            if (ids.Any() && ids.Any(_ => !_.Success))
            {
                throw new InvalidOperationException($"Data for type {nameof(T)} are corrupted");
            }

            return await Task.WhenAll(ids.Select(async _ => await ReadEntity<T>(Guid.Parse(_.Value))));
        }

        public async Task WriteEntity<T>(T entity) where T : IEntity
        {
            if (entity.Id == Guid.Empty)
            {
                throw new ArgumentException($"The id cannot be an empty GUID");
            }

            await _semaphore.WaitAsync();
            try
            {
                Directory.CreateDirectory(GetPath(typeof(T)));
                using var stream = new FileStream(GetPath(typeof(T), entity.Id), FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
                using var sw = new StreamWriter(stream, Encoding.UTF8);
                await sw.WriteLineAsync(JsonConvert.SerializeObject(entity, _jsonSerializerSettings));
            }
            catch (Exception e)
            {
                e.Data.Add("Type", typeof(T));
                e.Data.Add("Entity", JsonConvert.SerializeObject(entity, _jsonSerializerSettings));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task DeleteEntity<T>(Guid id) where T : IEntity
        {
            await _semaphore.WaitAsync();
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
                _semaphore.Release();
            }
        }

        private static string GetFileName(Guid id)
        {
            return string.Concat(id, _fileNameSuffix);
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