using AggregatorAPI.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AggregatorAPI.Services;
public class MemoryCacheService : IMemoryCacheService
{
    private readonly IMemoryCache _memoryCache;
    public MemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public void Add<T>(string key, T value)
    {
        var memoryOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            SlidingExpiration = null,
            Priority = CacheItemPriority.Normal
        };

        _memoryCache.Set(key, value, memoryOptions);
    }

    public void Add<T>(string key, T value, MemoryCacheEntryOptions memoryOptions)
    {
        _memoryCache.Set(key, value, memoryOptions);
    }

    public T Retrieve<T>(string key)
    {
        if (_memoryCache.TryGetValue(key, out T cachedValue))
        {
            return cachedValue;
        }

        return default(T);
    }
}