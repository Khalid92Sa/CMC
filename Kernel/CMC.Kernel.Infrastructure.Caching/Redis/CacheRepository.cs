using Microsoft.Extensions.Caching.Memory;
using CMC.Kernel.Core.Infrastructure;
using System;
using System.Threading.Tasks;

namespace CMC.Kernel.Infrastructure.Caching.Redis
{
    /// <summary>
    /// Cache Repository
    /// </summary>
    public class CacheRepository : ICacheRepository
    {
        /// <summary>
        /// Define the memory cache 
        /// </summary>
        private readonly IMemoryCache _cache;
        /// <summary>
        /// Define the expiry Time
        /// </summary>
        private TimeSpan expiryTime = new TimeSpan(24, 0, 0);
        /// <summary>
        /// Cache Repository constructor to  define the memory cache 
        /// </summary>
        /// <param name="cache"></param>
        public CacheRepository(IMemoryCache cache)
        {
            _cache = cache;
        }
        /// <summary>
        /// Get Object Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<T> GetObjectAsync<T>(string key, Func<Task<T>> func, TimeSpan? expiry = null) where T : class
        {
            var cachedObject = GetObjectFromCache<T>(key);
            if (cachedObject == null)
            {
                expiryTime = expiry ?? expiryTime;
                cachedObject = await UpdateCahceAsync(key, func);
            }
            return cachedObject;
        }
        /// <summary>
        /// Get Object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public T GetObject<T>(string key, Func<T> func, TimeSpan? expiry = null) where T : class
        {
            var cachedObject = GetObjectFromCache<T>(key);
            if (cachedObject == null)
            {
                expiryTime = expiry ?? expiryTime;
                cachedObject = UpdateCahce(key, func);
            }
            return cachedObject;
        }
        /// <summary>
        /// Remove Object
        /// </summary>
        /// <param name="key"></param>
        public void RemoveObject(string key)
        {

            _cache.Remove(key);
        }
        /// <summary>
        /// Get Object From Cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>

        private T GetObjectFromCache<T>(string key) where T : class
        {
            return _cache.Get<T>(key);
        }
        /// <summary>
        /// Get Object
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>

        public object GetObject(string key)
        {
            return _cache.Get(key);
        }
        /// <summary>
        /// Update Cahce
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <returns></returns>

        private T UpdateCahce<T>(string key, Func<T> func) where T : class
        {
            T dbObject = GetObjectFromDataSource(func);
            if (dbObject == null)
            {
                return null;
            }
            InsertObjectIntoCache(key, dbObject);
            return dbObject;
        }
        /// <summary>
        /// Update Cahce Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="func"></param>
        /// <returns></returns>

        private async Task<T> UpdateCahceAsync<T>(string key, Func<Task<T>> func) where T : class
        {
            T dbObject = await GetObjectFromDataSourceAsync(func);
            if (dbObject == null)
            {
                return null;
            }

            InsertObjectIntoCache(key, dbObject);
            return dbObject;
        }
        /// <summary>
        /// Get Object From Data Source
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        private T GetObjectFromDataSource<T>(Func<T> func)
        {
            return func();
        }
        /// <summary>
        /// Get Object From Data Source Async
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="func"></param>
        /// <returns></returns>
        private async Task<T> GetObjectFromDataSourceAsync<T>(Func<Task<T>> func)
        {
            return await func();
        }
        /// <summary>
        /// Insert Object Into Cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expTime"></param>
        public void InsertObjectIntoCache<T>(string key, T value, TimeSpan? expTime = null) where T : class
        {
            if (expTime.HasValue)
                _cache.Set(key, value, expTime.Value);
            else
                _cache.Set(key, value, expiryTime);
        }
        /// <summary>
        /// Insert Object Into Cache
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expTime"></param>
        public void InsertObjectIntoCache(string key, object value, TimeSpan? expTime = null)
        {
            if (expTime.HasValue)
                _cache.Set(key, value, expTime.Value);
            else
                _cache.Set(key, value, expiryTime);
        }
        /// <summary>
        /// Convert To Entity Key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        private string ConvertToEntityKey(string key, string className)
        {
            return key + "_" + className;
        }
    }
}
