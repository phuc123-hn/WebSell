using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using web1.Infrastructure.Logging;
using web1.Data;

namespace web1.Infrastructure.Caching
{
    /// <summary>
    /// Memory cache wrapper - cache frequently accessed data
    /// Using System.Web.Caching instead of System.Runtime.Caching for .NET Framework
    /// </summary>
    public interface ICacheService
    {
        T Get<T>(string key);
        void Set<T>(string key, T value, int durationMinutes = 60);
        void Remove(string key);
        void RemoveByPattern(string pattern);
        void Clear();
    }

    public class CacheService : ICacheService
    {
        private readonly FileLogger _logger = new FileLogger();
        private const string CACHE_PREFIX = "AppCache:";

        public T Get<T>(string key)
        {
            try
            {
                var cacheKey = CACHE_PREFIX + key;
                var value = System.Web.HttpRuntime.Cache.Get(cacheKey);

                if (value != null)
                {
                    _logger.LogInfo($"Cache HIT: {key}");
                    return (T)value;
                }

                _logger.LogInfo($"Cache MISS: {key}");
                return default(T);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting cache for key: {key}", ex);
                return default(T);
            }
        }

        public void Set<T>(string key, T value, int durationMinutes = 60)
        {
            try
            {
                var cacheKey = CACHE_PREFIX + key;
                var dependency = new System.Web.Caching.CacheDependency(null, null);

                System.Web.HttpRuntime.Cache.Insert(
                    cacheKey,
                    value,
                    dependency,
                    DateTime.UtcNow.AddMinutes(durationMinutes),
                    System.Web.Caching.Cache.NoSlidingExpiration
                );

                _logger.LogInfo($"Cache SET: {key} ({durationMinutes}min)");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error setting cache for key: {key}", ex);
            }
        }

        public void Remove(string key)
        {
            try
            {
                var cacheKey = CACHE_PREFIX + key;
                System.Web.HttpRuntime.Cache.Remove(cacheKey);
                _logger.LogInfo($"Cache REMOVED: {key}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing cache for key: {key}", ex);
            }
        }

        public void RemoveByPattern(string pattern)
        {
            try
            {
                var cache = System.Web.HttpRuntime.Cache;
                var cachePrefix = CACHE_PREFIX + pattern;
                var enumerator = cache.GetEnumerator();
                var keysToRemove = new List<string>();

                while (enumerator.MoveNext())
                {
                    string key = enumerator.Key.ToString();
                    if (key.StartsWith(cachePrefix))
                    {
                        keysToRemove.Add(key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    cache.Remove(key);
                }

                _logger.LogInfo($"Cache REMOVED by pattern: {pattern} ({keysToRemove.Count} items)");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing cache by pattern: {pattern}", ex);
            }
        }

        public void Clear()
        {
            try
            {
                RemoveByPattern("");
                _logger.LogInfo("All cache cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error clearing all cache", ex);
            }
        }
    }

    /// <summary>
    /// Cache keys constants
    /// </summary>
    public static class CacheKeys
    {
        public const string CATEGORIES = "Categories:All";
        public const string PRODUCTS = "Products:All";
        public const string PRODUCTS_HOT = "Products:Hot";
        public const string NEWS = "News:All";
        public const string NEWS_BY_CATEGORY = "News:Category:{0}";
        public const string VOUCHERS = "Vouchers:All";
        public const string EVENTS = "Events:Active";
        public const string SETTINGS = "Settings:All";

        public static string GetNewsCategoryKey(string category) => string.Format(NEWS_BY_CATEGORY, category);
    }
}
