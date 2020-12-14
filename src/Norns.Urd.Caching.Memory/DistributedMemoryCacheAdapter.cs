using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Caching.Memory
{
    public class DistributedMemoryCacheAdapter<T> : ICacheAdapter<T>
    {
        public string Name => throw new NotImplementedException();

        public void Set(CacheOptions op, T result)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(CacheOptions op, T result, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(CacheOptions op, out T result)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> TryGetValueAsync(CacheOptions op, out T result)
        {
            throw new NotImplementedException();
        }
    }
}
