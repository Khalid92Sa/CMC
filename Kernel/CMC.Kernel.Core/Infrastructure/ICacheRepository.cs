using System;
using System.Threading.Tasks;

namespace CMC.Kernel.Core.Infrastructure
{
    public interface ICacheRepository
    {
        object GetObject(string key);
        T GetObject<T>(string key, Func<T> func, TimeSpan? expiry = null) where T : class;
        Task<T> GetObjectAsync<T>(string key, Func<Task<T>> func, TimeSpan? expiry = null) where T : class;
        void RemoveObject(string key);
        void InsertObjectIntoCache<T>(string key, T value, TimeSpan? expTime = null) where T : class;
        void InsertObjectIntoCache(string key, object value, TimeSpan? expTime = null);
    }
}