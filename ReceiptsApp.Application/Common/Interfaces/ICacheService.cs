namespace ReceiptsApp.Application.Common.Interfaces;

/// <summary>
/// Generic cache abstraction. The Application layer depends only on this
/// interface — it has no idea whether Infrastructure backs it with
/// in-memory cache, Redis, or anything else.
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Removes every cache entry whose key starts with the given prefix.</summary>
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the cached value if present, otherwise invokes the factory,
    /// stores the result, and returns it. Avoids repeating the
    /// "check cache, miss, compute, store" pattern in every handler.
    /// </summary>
    Task<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);
}
