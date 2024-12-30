using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace GoogleGroups.Extensions
{
    public static class DistributedCacheExtensions
    {
        public static async Task SetRecordAsync<T>(this IDistributedCache cache, string recordId, T data, TimeSpan? absoluteExpireTime = null, TimeSpan? unusedExpireTime = null)
        {
            var options = new DistributedCacheEntryOptions();
           
            options.AbsoluteExpirationRelativeToNow = absoluteExpireTime ?? TimeSpan.FromMinutes(5);
            options.SlidingExpiration = unusedExpireTime;

            var jsonData = JsonSerializer.Serialize(data);
            await cache.SetStringAsync(recordId, jsonData, options);
        }

        public static async Task<T> GetRecordAsync<T>(this IDistributedCache cache, string recordId)
        {
            string? jsonData;
            try
            {
                jsonData = await cache.GetStringAsync(recordId);
            }
            catch { jsonData = null; }

            if (jsonData is null)
            {
                return default(T);
            }

            return JsonSerializer.Deserialize<T>(jsonData);
        }

        public static async Task DeleteRecordAsync<T>(this IDistributedCache cache, string recordId)
        {

            await cache.RemoveAsync(recordId);
        }
    }
}
