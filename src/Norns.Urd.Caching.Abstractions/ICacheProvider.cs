using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Caching
{
    public interface ICacheProvider
    {
        Task<object> GetOrCreateValueAsync(Func<AspectContext, CacheOptions>[] optionCreators, AspectContext context, AsyncAspectDelegate next, CancellationToken token, bool isReturnValueTask);

        object GetOrCreateValue(Func<AspectContext, CacheOptions>[] optionCreators, AspectContext context, AspectDelegate next);
    }

    public interface ICacheProvider<T> : ICacheProvider
    {
        Task<T> GetOrCreateAsync(Func<AspectContext, CacheOptions>[] optionCreators, AspectContext context, AsyncAspectDelegate next, CancellationToken token, bool isReturnValueTask);

        T GetOrCreate(Func<AspectContext, CacheOptions>[] optionCreators, AspectContext context, AspectDelegate next);
    }

    public interface ICacheAdapter
    {
        string Name { get; }

        bool TryGetValue<T>(CacheOptions op, out T result);

        void Set<T>(CacheOptions op, T result);

        Task<(bool, T)> TryGetValueAsync<T>(CacheOptions op, CancellationToken token);

        Task SetAsync<T>(CacheOptions op, T result, CancellationToken token);
    }

    public class CacheProvider<T> : ICacheProvider<T>
    {
        private readonly Dictionary<string, ICacheAdapter> adapters;

        public CacheProvider(IEnumerable<ICacheAdapter> adapters)
        {
            this.adapters = adapters.ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
        }

        public T GetOrCreate(Func<AspectContext, CacheOptions>[] optionCreators, AspectContext context, AspectDelegate next)
        {
            return GetOrCreate(optionCreators, context, next, 0);
        }

        private T GetOrCreate(Func<AspectContext, CacheOptions>[] optionCreators, AspectContext context, AspectDelegate next, int index)
        {
            if (index >= optionCreators.Length)
            {
                next(context);
                return (T)context.ReturnValue;
            }

            var op = optionCreators[index](context);
            T result;
            var cacheName = op.CacheName ?? CacheOptions.DefaultCacheName;
            if (adapters.TryGetValue(cacheName, out var adapter))
            {
                if (!adapter.TryGetValue(op, out result))
                {
                    result = GetOrCreate(optionCreators, context, next, ++index);
                    adapter.Set(op, result);
                }
            }
            else
            {
                throw new ArgumentException($"No such cache: {cacheName}.");
            }

            return result;
        }

        public async Task<T> GetOrCreateAsync(Func<AspectContext, CacheOptions>[] optionCreators, AspectContext context, AsyncAspectDelegate next, CancellationToken token, bool isReturnValueTask)
        {
            token.ThrowIfCancellationRequested();
            return await GetOrCreateAsync(optionCreators, context, next, token, isReturnValueTask, 0);
        }

        private async Task<T> GetOrCreateAsync(Func<AspectContext, CacheOptions>[] optionCreators, AspectContext context, AsyncAspectDelegate next, CancellationToken token, bool isReturnValueTask, int index)
        {
            if (index >= optionCreators.Length)
            {
                token.ThrowIfCancellationRequested();
                await next(context);
                return isReturnValueTask
                    ? ((ValueTask<T>)context.ReturnValue).Result
                    : ((Task<T>)context.ReturnValue).Result;
            }

            var op = optionCreators[index](context);
            T result;
            var cacheName = op.CacheName ?? CacheOptions.DefaultCacheName;
            if (adapters.TryGetValue(cacheName, out var adapter))
            {
                var (hasValue, r) = await adapter.TryGetValueAsync<T>(op, token);
                if (hasValue)
                {
                    result = r;
                }
                else
                {
                    token.ThrowIfCancellationRequested();
                    result = await GetOrCreateAsync(optionCreators, context, next, token, isReturnValueTask, ++index);
                    await adapter.SetAsync(op, result, token);
                }
            }
            else
            {
                throw new ArgumentException($"No such cache: {cacheName}.");
            }

            return result;
        }

        public object GetOrCreateValue(Func<AspectContext, CacheOptions>[] optionCreators, AspectContext context, AspectDelegate next)
        {
            return GetOrCreate(optionCreators, context, next);
        }

        public async Task<object> GetOrCreateValueAsync(Func<AspectContext, CacheOptions>[] optionCreators, AspectContext context, AsyncAspectDelegate next, CancellationToken token, bool isReturnValueTask)
        {
            var v = await GetOrCreateAsync(optionCreators, context, next, token, isReturnValueTask);
            return isReturnValueTask ? (object)new ValueTask<T>(v) : Task.FromResult(v);
        }
    }
}