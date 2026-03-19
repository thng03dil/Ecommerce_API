using Ecommerce.Application.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ecommerce.Infrastructure.RedisCaching
{
    public class RedisCacheOptions
    {
        public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        public int DefaultExpirationMinutes { get; set; } = 5;
    }

    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _distributedCache;
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;   // tác dụng trong đếm số lượt xem sản phẩm
        private readonly ILogger<RedisCacheService> _logger;
        private readonly RedisCacheOptions _options;

        public RedisCacheService(
            IDistributedCache distributedCache,
            IConnectionMultiplexer redis,
            ILogger<RedisCacheService> logger,
            IOptions<RedisCacheOptions> options)
        {
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _db = _redis.GetDatabase();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Lấy options hoặc dùng mặc định nếu chưa đăng ký trong Program.cs
            _options = options?.Value ?? new RedisCacheOptions();
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var data = await _distributedCache.GetStringAsync(key);

                if (string.IsNullOrEmpty(data))
                {
                    _logger.LogDebug("Cache miss for key: {CacheKey}", key);
                    return default;
                }

                _logger.LogDebug("Cache hit for key: {CacheKey}", key);
                // Sử dụng config _options để deserialize
                return JsonSerializer.Deserialize<T>(data, _options.JsonSerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value from Redis cache for key: {CacheKey}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
        {
            try
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    // Lấy thời gian TTL từ settings cấu hình (tránh hardcode)
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(_options.DefaultExpirationMinutes)
                };

                // Sử dụng config _options để serialize
                var serializedData = JsonSerializer.Serialize(value, _options.JsonSerializerOptions);
                await _distributedCache.SetStringAsync(key, serializedData, cacheOptions);

                _logger.LogDebug("Successfully set cache for key: {CacheKey}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value to Redis cache for key: {CacheKey}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _distributedCache.RemoveAsync(key);
                _logger.LogDebug("Successfully removed cache for key: {CacheKey}", key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from Redis cache for key: {CacheKey}", key);
            }
        }

        public async Task<long> IncrementAsync(string key)
        {
            try
            {
                // Sử dụng hàm gốc của Redis để tăng số chuẩn Atomic, không sợ Race Condition
                var newValue = await _db.StringIncrementAsync(key);
                _logger.LogDebug("Successfully incremented cache for key: {CacheKey} to {Value}", key, newValue);
                return newValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing cache for key: {CacheKey}", key);
                return DateTime.UtcNow.Ticks; // Fallback an toàn nếu Redis lỗi
            }
        }

        public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            var cachedValue = await GetAsync<T>(key);

            if (cachedValue != null && !cachedValue.Equals(default(T)))
            {
                return cachedValue;
            }

            var value = await factory();

            if (value != null)
            {
                await SetAsync(key, value, expiration);
            }

            return value;
        }
    }
}
