using System.Text.Json;
using StackExchange.Redis;
using Vessel.Application.Interfaces.Caching;

namespace Vessel.Infrastructure.Services.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _db;

    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (!value.HasValue)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);
        if (expiration.HasValue)
        {
            await _db.StringSetAsync(key, json, expiration.Value);
        }
        else
        {
            await _db.StringSetAsync(key, json);
        }
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}
