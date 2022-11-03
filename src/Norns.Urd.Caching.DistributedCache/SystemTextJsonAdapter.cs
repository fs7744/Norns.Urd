using System.Text.Json;

namespace Norns.Urd.Caching.DistributedCache
{
    public class SystemTextJsonAdapter : ISerializationAdapter
    {
        public string Name { get; }

        public SystemTextJsonAdapter(string name)
        {
            Name = name;
        }

        public T Deserialize<T>(byte[] data)
        {
            return JsonSerializer.Deserialize<T>(data);
        }

        public byte[] Serialize<T>(T data)
        {
            return JsonSerializer.SerializeToUtf8Bytes<T>(data);
        }
    }
}