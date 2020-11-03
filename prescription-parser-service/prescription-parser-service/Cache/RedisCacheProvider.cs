using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using CachingFramework.Redis;
using CachingFramework.Redis.Contracts.RedisObjects;
using StackExchange.Redis;

namespace prescription_parser_service.Cache
{
    public class RedisCacheProvider : ICacheProvider
    {
        private static string _connectionString;

        private static readonly Lazy<ConnectionMultiplexer> LazyConnection
            = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_connectionString));

        private readonly RedisContext _redisContext;

        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        public RedisCacheProvider(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException("connectionString");

            _redisContext = new RedisContext(Connection);
        }

        public void SetSliding<T>(string key, T value, TimeSpan timeSpan)
        {
            _redisContext.Cache.SetObject(key, value, timeSpan);
        }

        public async Task SetSlidingAsync<T>(string key, T value, TimeSpan timeSpan)
        {
            await _redisContext.Cache.SetObjectAsync(key, value, timeSpan);
        }

        public void Set<T>(string key, T value)
        {
            _redisContext.Cache.SetObject(key, value);
        }

        public async Task SetAsync<T>(string key, T value)
        {
            await _redisContext.Cache.SetObjectAsync(key, value);
        }

        public void SetAbsolute<T>(string key, T value, DateTime expiration)
        {
            _redisContext.Cache.SetObject(key, value, expiration.TimeOfDay);
        }

        public async Task SetAbsoluteAsync<T>(string key, T value, DateTime expiration)
        {
            await _redisContext.Cache.SetObjectAsync(key, value, expiration.TimeOfDay);
        }

        public void SetAbsolute<T>(string key, T value, TimeSpan timeSpan)
        {
            _redisContext.Cache.SetObject(key, value, timeSpan);
        }

        public async Task SetAbsoluteAsync<T>(string key, T value, TimeSpan timeSpan)
        {
            await _redisContext.Cache.SetObjectAsync(key, value, timeSpan);
        }

        public T Get<T>(string key)
        {
            try
            {
                return _redisContext.Cache.GetObject<T>(key, CommandFlags.PreferReplica);
            }
            catch (RedisException ex)
            {
                Console.WriteLine(key);
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                return await _redisContext.Cache.GetObjectAsync<T>(key, CommandFlags.PreferReplica);
            }
            catch (RedisException ex)
            {
                Console.WriteLine(key);
                throw ex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public T GetSliding<T>(string key, TimeSpan slidingExpiration)
        {
            _redisContext.Cache.KeyExpire(key, DateTime.Now.Add(slidingExpiration));

            return _redisContext.Cache.GetObject<T>(key, CommandFlags.PreferReplica);
        }

        public bool TryGetValue<T>(string key, out T result)
        {
            return _redisContext.Cache.TryGetObject(key, out result, CommandFlags.PreferReplica);
        }

        public async Task<(bool keyExists, T result)> TryGetValueAsync<T>(string key)
        {
            var keyExists = true;
            var result = await _redisContext.Cache.GetObjectAsync<T>(key, CommandFlags.PreferReplica);
            if (result == null)
            {
                keyExists = await _redisContext.Cache.KeyExistsAsync(key);
            }

            return (keyExists, result);
        }


        public bool TryGetValueSliding<T>(string key, TimeSpan slidingExpiration, out T result)
        {
            _redisContext.Cache.KeyExpireAsync(key, DateTime.Now.Add(slidingExpiration));

            return _redisContext.Cache.TryGetObject(key, out result, CommandFlags.PreferReplica);
        }

        public void Set<T>(List<KeyValuePair<string, T>> keyValuePairs)
        {
            var tasks = new List<Task>();

            foreach (var item in keyValuePairs)
            {
                tasks.Add(Task.Run(() => _redisContext.Cache.SetObjectAsync(item.Key, item.Value)));
            }
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ex)
            {
                var exceptions = new List<string>();
                foreach (var innerEx in ex.InnerExceptions)
                {
                    exceptions.Add(innerEx.Message);
                }
                var concatenated = string.Join(", ", exceptions);
                throw new Exception(concatenated);
            }

        }

        public void SetAbsolute<T>(NameValueCollection valueCollection, TimeSpan timeSpan)
        {
            throw new NotImplementedException();
        }

        public int ExpireKeysByPattern(string pattern, TimeSpan timeSpan)
        {
            // 2 different ways to do the same thing.
            // one uses the CachingFramework.Redis (code in this method)
            // other uses stackExchange.Redis 
            return ExpireKeysByPatternRedis(pattern, timeSpan);

        }

        public int ExpireKeysByPatternRedis(string pattern, TimeSpan timeSpan)
        {
            var keyCount = 0;

            var endPoints = Connection.GetEndPoints();

            IServer server = null;

            foreach (var endPoint in endPoints)
            {
                server = Connection.GetServer(endPoint);
                if (server.IsReplica)
                {
                    break;
                }
            }

            if (server == null) return keyCount;

            var keys = server.Keys(pattern: pattern, pageSize: 5000).ToArray();



            if (!keys.Any()) return keyCount;

            keyCount = keys.Length;

            var db = Connection.GetDatabase();

            foreach (var redisKey in keys)
            {
                db.KeyExpire(redisKey, timeSpan, CommandFlags.FireAndForget);
            }

            return keyCount;
        }

        public int RemoveKeysByPattern(string pattern)
        {
            return RemoveKeysByPatternRedis(pattern);
        }

        public int RemoveKeysByPatternRedis(string pattern)
        {
            var keyCount = 0;

            var endPoints = Connection.GetEndPoints();

            IServer server = null;

            foreach (var endPoint in endPoints)
            {
                server = Connection.GetServer(endPoint);
                if (server.IsReplica)
                {
                    break;
                }
            }

            if (server == null) return keyCount;

            var keys = server.Keys(pattern: pattern, pageSize: 5000).ToArray();

            var db = Connection.GetDatabase();

            foreach (var redisKey in keys)
            {
                db.KeyDelete(redisKey, CommandFlags.FireAndForget);
                keyCount++;
            }

            return keyCount;
        }

        public void SetList<T>(string key, List<T> value)
        {
            var rs = _redisContext.Collections.GetRedisList<T>(key);
            rs.Clear();
            rs.AddRange(value);
        }

        public void SetDictionary<TK, TV>(string key, ConcurrentDictionary<TK, TV> value)
        {
            IRedisDictionary<TK, TV> hash = _redisContext.Collections.GetRedisDictionary<TK, TV>(key);
            hash.Clear();
            hash.AddRange(value);
            hash.Expiration = null;
        }

        public void AddDictionaryItem<TK, TV>(string key, TK itemKey, TV value)
        {
            IRedisDictionary<TK, TV> hash = _redisContext.Collections.GetRedisDictionary<TK, TV>(key);
            hash.Remove(itemKey);
            hash.Add(itemKey, value);

        }

        public void RemoveDictionaryItem<TK, TV>(string key, TK itemKey)
        {
            IRedisDictionary<TK, TV> hash = _redisContext.Collections.GetRedisDictionary<TK, TV>(key);
            hash.Remove(itemKey);
        }

        public void RemoveByKey(string key)
        {
            _redisContext.Cache.Remove(key);
        }

        public TimeSpan? GetTimeToLive(string key)
        {
            var db = Connection.GetDatabase();

            return db.KeyTimeToLive(key);
        }

        public TV GetDictionaryItem<TK, TV>(string key, TK itemKey)
        {
            IRedisDictionary<TK, TV> hash = _redisContext.Collections.GetRedisDictionary<TK, TV>(key);
            return hash.GetValue(itemKey);
        }
        bool ICacheProvider.TryGetValue<T>(string key, out T result)
        {
            return TryGetValue(key, out result);
        }

        public List<T> GetList<T>(string key)
        {
            if (!_redisContext.Collections.GetRedisList<T>(key).Any()) return null;

            var rs = _redisContext.Collections.GetRedisList<T>(key);
            return rs.GetRange().ToList();

        }
        public bool TryGetList<T>(string key, out List<T> result)
        {
            if (_redisContext.Collections.GetRedisList<T>(key).Any())
            {
                var rs = _redisContext.Collections.GetRedisList<T>(key);
                result = rs.GetRange().ToList();
                return true;
            }

            result = null;
            return false;
        }
    }
}
