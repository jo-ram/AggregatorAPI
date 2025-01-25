using Microsoft.Extensions.Caching.Memory;

namespace AggregatorAPI.Interfaces
{
    public interface IMemoryCacheService
    {
        void Add<T>(string key, T value);
        void Add<T>(string key, T value, MemoryCacheEntryOptions memoryOptions);
        T Retrieve<T>(string key);
    }
}