using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace CalibrationEngine
{
    /// <summary>零依赖 JSON 读写（DataContractJsonSerializer）。</summary>
    public static class Json
    {
        public static T ReadFile<T>(string path)
        {
            using (var fs = File.OpenRead(path))
            {
                var ser = new DataContractJsonSerializer(typeof(T));
                return (T)ser.ReadObject(fs);
            }
        }

        public static string ToJson<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var settings = new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };
                var ser = new DataContractJsonSerializer(typeof(T), settings);
                ser.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        public static void WriteFile<T>(string path, T obj)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, ToJson(obj), new UTF8Encoding(false));
        }
    }
}
