using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;

namespace HR_PAYROLL_V2.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(15);

    // EF Core fixes up navigation properties automatically for entities tracked within the
    // same DbContext (e.g. Company.OrganizationalUnits[].Company), which can form a real
    // object cycle at runtime even without eager Include(). IgnoreCycles keeps caching of
    // plain entities safe without changing the JSON shape for the common non-cyclic case.
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var data = await _cache.GetStringAsync(key);
        return data is null ? default : JsonSerializer.Deserialize<T>(data, SerializerOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? DefaultExpiry
        };
        await _cache.SetStringAsync(key, JsonSerializer.Serialize(value, SerializerOptions), options);
    }

    public Task RemoveAsync(string key) => _cache.RemoveAsync(key);

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
    {
        var cached = await GetAsync<T>(key);
        if (cached is not null)
        {
            return cached;
        }

        var value = await factory();
        await SetAsync(key, value, expiry);
        return value;
    }
}
