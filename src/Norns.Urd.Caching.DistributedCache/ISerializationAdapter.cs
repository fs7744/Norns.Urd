namespace Norns.Urd.Caching.DistributedCache
{
    public interface ISerializationAdapter
    {
        string Name { get; }

        byte[] Serialize<T>(T data);

        T Deserialize<T>(byte[] data);
    }
}