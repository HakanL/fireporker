using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace PlanningPoker.Models
{
    public class GameManager
    {
        private const string CachePrefix = "PokerGame";
        private static readonly object CacheLock = new object();
        private static readonly List<string> CacheKeys = new List<string>();

        public static int GameCount => CacheKeys.Count;

        public static void StorePokerGame(PokerGame game)
        {
            var cacheKey = string.Format("{0}_{1}", CachePrefix, game.Id);
            StoreCacheObject(cacheKey, game, game.ExpirationDate);
            if (!CacheKeys.Contains(cacheKey))
                CacheKeys.Add(cacheKey);
        }
        
        public static PokerGame GetPokerGame(string cacheKey)
        {
            var game = GetCacheObject<PokerGame>(cacheKey);
            return game;
        }

        public static PokerGame GetPokerGame(Guid gameId)
        {
            var cacheKey = string.Format("{0}_{1}", CachePrefix, gameId);
            return GetPokerGame(cacheKey);
        }

        public static IEnumerable<PokerGame> GetPokerGames()
        {
            return CacheKeys.Select(GetPokerGame);
        }

        private static void StoreCacheObject(string cacheKey, object cacheObject, DateTime expirationDate)
        {
            lock (CacheLock)
            {
                MemoryCache.Default.Set(cacheKey, cacheObject, expirationDate);
            }
        }

        private static T GetCacheObject<T>(string cacheKey) where T : class
        {
            var cachedObject = MemoryCache.Default.Get(cacheKey) as T;

            if (cachedObject != null)
            {
                return cachedObject;
            }

            lock (CacheLock)
            {
                //Check to see if anyone wrote to the cache while we where waiting our turn to write the new value.
                cachedObject = MemoryCache.Default.Get(cacheKey) as T;

                return cachedObject;
            }
        }
    }
}