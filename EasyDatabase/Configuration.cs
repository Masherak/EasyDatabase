using System;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace EasyDatabase
{
    public class Configuration
    {
        public Configuration(string documentsPath = null, Encoding encoding = null, JsonSerializerSettings jsonSerializerSettings = null)
        {
            DocumentsPath = documentsPath ?? Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException("Assembly location is not available"), "Database");
            Encoding = encoding ?? Encoding.UTF8;
            JsonSerializerSettings = jsonSerializerSettings ?? new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
        }

        public const string FileNameSuffix = ".json";

        public string DocumentsPath { get; }
        public Encoding Encoding { get; }
        public JsonSerializerSettings JsonSerializerSettings { get; }
    }
}