using Ecommerce.Application.Services.Interfaces;

namespace Ecommerce.API.Middleware
{
    public class TokenBlacklistMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenBlacklistMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICacheService cacheService)
        {
            // Lấy token từ Header Authorization
            var authHeader = context.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var blacklistKey = $"Blacklist:Token:{token}";

                // Kiểm tra trong Redis
                var isRevoked = await cacheService.GetAsync<string>(blacklistKey);

                if (isRevoked != null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"message\": \"Token has been revoked. Please login again.\"}");
                    return;
                }
            }

            await _next(context);
        }
    }
}
