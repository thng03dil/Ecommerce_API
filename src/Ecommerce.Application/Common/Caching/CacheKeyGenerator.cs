using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Ecommerce.Application.Common.Caching
{
    public static class CacheKeyGenerator
    {
        public static string GetEntityKey<T>(object id)
        {
            return $"{typeof(T).Name}:{id}";
        }

        public static string GetListKey<T>()
        {
            return $"{typeof(T).Name}:List";
        }

        public static string GetListVersionKey<T>()
        {
            return $"{typeof(T).Name}:Version";
        }

        public static string GetQueryKey<T>(string version, int page, int size, string filter = "")
        {
            var key = $"{typeof(T).Name}:List:v{version}:Page:{page}:Size:{size}";

            if (!string.IsNullOrWhiteSpace(filter))
            {
                // Băm chuỗi filter để tránh Redis key bị lộn xộn hoặc lỗi kí tự đặc biệt
                key += $":Filter:{GenerateHash(filter)}";
            }

            return key;
        }

        /// <summary>
        /// Tạo mã Hash ngắn gọn và an toàn làm Cache Key từ chuỗi Filter phức tạp
        /// </summary>
        private static string GenerateHash(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha256.ComputeHash(bytes);

            // Xoá kí tự đặc biệt Base64 để an toàn cho Redis Key
            return Convert.ToBase64String(hash)
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
        }
    }
}
