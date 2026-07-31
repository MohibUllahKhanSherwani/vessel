using System.Collections.Concurrent;
using System.Text.Json;
using Vessel.Application.Interfaces.Caching;

namespace Vessel.Infrastructure.Services.Caching;

/// <summary>
/// Simple in-process cache using a ConcurrentDictionary.
/// Replaces Redis for local / demo runs — zero external dependencies.
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, (string Json, DateTimeOffset? Expires)> _store = new();

    public Task<T?> GetAsync<T>(string key)
    {
        if (_store.TryGetValue(key, out var entry))
        {
            if (entry.Expires == null || entry.Expires > DateTimeOffset.UtcNow)
            {
                var value = JsonSerializer.Deserialize<T>(entry.Json);
                return Task.FromResult(value);
            }
            // Evict expired entry
            _store.TryRemove(key, out _);
        }
        return Task.FromResult(default(T));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);
        DateTimeOffset? expires = expiration.HasValue ? DateTimeOffset.UtcNow.Add(expiration.Value) : null;
        _store[key] = (json, expires);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
