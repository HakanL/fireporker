using Microsoft.Extensions.Caching.Memory;

namespace PlanningPoker.Models;

public class GameManager
{
    private const string CachePrefix = "PokerGame";
    private static readonly object CacheLock = new();
    private static readonly List<string> CacheKeys = new();
    private static readonly IMemoryCache Cache = new MemoryCache(new MemoryCacheOptions());

    public static int GameCount => CacheKeys.Count;

    public static void StorePokerGame(PokerGame game)
    {
        var cacheKey = $"{CachePrefix}_{game.Id}";
        StoreCacheObject(cacheKey, game, game.ExpirationDate);
        
        lock (CacheLock)
        {
            if (!CacheKeys.Contains(cacheKey))
                CacheKeys.Add(cacheKey);
        }
    }
    
    public static PokerGame? GetPokerGame(string cacheKey)
    {
        return GetCacheObject<PokerGame>(cacheKey);
    }

    public static PokerGame? GetPokerGame(Guid gameId)
    {
        var cacheKey = $"{CachePrefix}_{gameId}";
        return GetPokerGame(cacheKey);
    }

    public static IEnumerable<PokerGame> GetPokerGames()
    {
        return CacheKeys.Select(GetPokerGame).Where(g => g != null)!;
    }

    private static void StoreCacheObject(string cacheKey, object cacheObject, DateTime expirationDate)
    {
        lock (CacheLock)
        {
            Cache.Set(cacheKey, cacheObject, expirationDate);
        }
    }

    private static T? GetCacheObject<T>(string cacheKey) where T : class
    {
        var cachedObject = Cache.Get(cacheKey) as T;

        if (cachedObject != null)
        {
            return cachedObject;
        }

        lock (CacheLock)
        {
            // Check to see if anyone wrote to the cache while we were waiting our turn to write the new value.
            cachedObject = Cache.Get(cacheKey) as T;
            return cachedObject;
        }
    }
}
