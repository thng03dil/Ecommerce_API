using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public int UserId { get; private set; }

        public string TokenHash { get; private set; } = null!;

        public DateTime ExpiryDate { get; private set; }

        public bool IsRevoked { get; private set; }

        public string? IpAddress { get; private set; }

        public string? UserAgent { get; private set; }

        public string? DeviceId { get; private set; }

        public User User { get; private set; } = null!;

        public RefreshToken(
            int userId,
            string tokenHash,
            DateTime expiryDate,
            string? ipAddress,
            string? userAgent,
            string? deviceId)
        {
            UserId = userId;
            TokenHash = tokenHash;
            ExpiryDate = expiryDate;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            DeviceId = deviceId;
            IsRevoked = false;
        }

        // Constructor dùng khi tạo token
        public RefreshToken(int userId, string tokenHash, DateTime expiryDate)
        {
            UserId = userId;
            TokenHash = tokenHash;
            ExpiryDate = expiryDate;
            IsRevoked = false;
        }

        // EF Core cần
        private RefreshToken() { }

        public void Revoke()
        {
            IsRevoked = true;
        }
    }
}