using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ReceiptsApp.Application.Common.Interfaces;

namespace ReceiptsApp.Infrastructure.Caching;

/// <summary>
/// IDistributedCache-backed implementation of ICacheService. Registered with
/// Redis in production (see ServiceRegistration) but works against any
/// IDistributedCache implementation, including the in-memory one used in
/// local development without Docker.
///
/// IDistributedCache doesn't support prefix-based removal natively, so this
/// implementation also tracks a per-prefix key index using the same cache
/// backend, which is how RemoveByPrefixAsync is able to evict, for example,
/// every "user:{id}:receipts:*" entry after a write.
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<RedisCacheService> _logger;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RedisCacheService(IDistributedCache distributedCache, ILogger<RedisCacheService> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var bytes = await _distributedCache.GetAsync(key, cancellationToken);
        if (bytes is null || bytes.Length == 0)
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(bytes, SerializerOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize cache entry {Key}; treating as a miss", key);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(5)
        };

        await _distributedCache.SetAsync(key, bytes, options, cancellationToken);
        await TrackKeyAsync(key, cancellationToken);
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        _distributedCache.RemoveAsync(key, cancellationToken);

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var indexKey = BuildIndexKey(prefix);
        var indexBytes = await _distributedCache.GetAsync(indexKey, cancellationToken);

        if (indexBytes is null)
        {
            return;
        }

        var trackedKeys = JsonSerializer.Deserialize<HashSet<string>>(indexBytes, SerializerOptions)
                           ?? new HashSet<string>();

        foreach (var key in trackedKeys)
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
        }

        await _distributedCache.RemoveAsync(indexKey, cancellationToken);

        _logger.LogInformation("Evicted {Count} cache entries under prefix {Prefix}", trackedKeys.Count, prefix);
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var value = await factory(cancellationToken);
        await SetAsync(key, value, expiration, cancellationToken);
        return value;
    }

    private async Task TrackKeyAsync(string key, CancellationToken cancellationToken)
    {
        // Index by the first two ':'-delimited segments, e.g. "user:{id}".
        var segments = key.Split(':');
        if (segments.Length < 2)
        {
            return;
        }

        var prefix = $"{segments[0]}:{segments[1]}";
        var indexKey = BuildIndexKey(prefix);

        var indexBytes = await _distributedCache.GetAsync(indexKey, cancellationToken);
        var trackedKeys = indexBytes is null
            ? new HashSet<string>()
            : JsonSerializer.Deserialize<HashSet<string>>(indexBytes, SerializerOptions) ?? new HashSet<string>();

        trackedKeys.Add(key);

        var updatedBytes = JsonSerializer.SerializeToUtf8Bytes(trackedKeys, SerializerOptions);
        await _distributedCache.SetAsync(
            indexKey,
            updatedBytes,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) },
            cancellationToken);
    }

    private static string BuildIndexKey(string prefix) => $"__index__:{prefix}";
}
