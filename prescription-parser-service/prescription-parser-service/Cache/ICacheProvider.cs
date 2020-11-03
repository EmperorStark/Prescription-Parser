using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Threading.Tasks;

namespace prescription_parser_service.Cache
{
    public interface ICacheProvider
    {
        void SetSliding<T>(string key, T value, TimeSpan timeSpan);
        Task SetSlidingAsync<T>(string key, T value, TimeSpan timeSpan);
        void Set<T>(string key, T value);
        Task SetAsync<T>(string key, T value);
        void SetAbsolute<T>(string key, T value, DateTime expiration);
        Task SetAbsoluteAsync<T>(string key, T value, DateTime expiration);
        void SetAbsolute<T>(string key, T value, TimeSpan timeSpan);
        Task SetAbsoluteAsync<T>(string key, T value, TimeSpan timeSpan);
        T Get<T>(string key);
        Task<T> GetAsync<T>(string key);
        T GetSliding<T>(string key, TimeSpan slidingExpiration);
        bool TryGetValue<T>(string key, out T result);
        Task<(bool keyExists, T result)> TryGetValueAsync<T>(string key);
        bool TryGetValueSliding<T>(string key, TimeSpan slidingExpiration, out T result);
        void Set<T>(List<KeyValuePair<string, T>> keyValuePairs);
        void SetAbsolute<T>(NameValueCollection valueCollection, TimeSpan timeSpan);
        int ExpireKeysByPattern(string pattern, TimeSpan timeSpan);
        int RemoveKeysByPattern(string pattern);
        void SetList<T>(string key, List<T> value);
        List<T> GetList<T>(string key);
        bool TryGetList<T>(string key, out List<T> result);
        TV GetDictionaryItem<TK, TV>(string key, TK itemKey);
        void SetDictionary<TK, TV>(string key, ConcurrentDictionary<TK, TV> value);
        void RemoveDictionaryItem<TK, TV>(string key, TK itemKey);
        void RemoveByKey(string key);
        TimeSpan? GetTimeToLive(string key);
    }
}
