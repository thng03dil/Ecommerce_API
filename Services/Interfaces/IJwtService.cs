using Ecommerce_API.Models;
using System.Security.Claims;

namespace Ecommerce_API.Services.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);

        string GenerateRefreshToken();

        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
