using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace CalibrationBench.UI.Acquisition
{
    internal static class JsonUtil
    {
        public static void Write<T>(string path, T obj)
        {
            using (var ms = new MemoryStream())
            {
                var settings = new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true };
                var ser = new DataContractJsonSerializer(typeof(T), settings);
                ser.WriteObject(ms, obj);
                File.WriteAllText(path, Encoding.UTF8.GetString(ms.ToArray()), new UTF8Encoding(false));
            }
        }
    }
}
