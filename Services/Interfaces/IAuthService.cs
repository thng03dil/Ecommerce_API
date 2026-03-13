using Ecommerce_API.DTOs.Auth;
using Ecommerce_API.DTOs.Common;
using Microsoft.AspNetCore.Identity.Data;

namespace Ecommerce_API.Services.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterDto request);

        Task<AuthResponseDto> LoginAsync(LoginDto request);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequest request);
    }
}