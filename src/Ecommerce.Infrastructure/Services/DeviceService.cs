using Ecommerce.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Infrastructure.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DeviceService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
                   ?? "unknown";
        }

        public string GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString()
                   ?? "unknown";
        }

        public string GenerateDeviceId(string userAgent, string ip)
        {
            return $"{userAgent}-{ip}".GetHashCode().ToString();
        }
    }
}
