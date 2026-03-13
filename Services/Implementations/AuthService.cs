using Ecommerce_API.Services.Interfaces;
using Ecommerce_API.Repositories.Interfaces;
using Ecommerce_API.Models;
using Ecommerce_API.DTOs.Auth;
using Ecommerce_API.Helpers;
using Microsoft.AspNetCore.Identity.Data;
using Ecommerce_API.DTOs.Common;
using System.Security.Claims;
using Ecommerce_API.Exceptions;

namespace Ecommerce_API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepo _userRepo;
        private readonly IJwtService _jwtService;

        public AuthService(
            IUserRepo userRepo,
            IJwtService jwtService)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
        }

        public async Task RegisterAsync(RegisterDto request)
        {
            var exist = await _userRepo.GetByEmailAsync(request.Email);

            if (exist != null)
                throw new Exception("Email already exists");

            var user = new User
            {
                Email = request.Email,
                PasswordHash = PasswordHasher.Hash(request.Password)
            };

            await _userRepo.AddAsync(user);
            await _userRepo.SaveChangesAsync();
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto request)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email);

            if (user == null)
                throw new UnauthorizedException("Invalid email or password");

            var valid = PasswordHasher.Verify(
                request.Password,
                user.PasswordHash);

            if (!valid)
                throw new UnauthorizedException("Invalid email or password");

            var accessToken = _jwtService.GenerateAccessToken(user);

            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

            await _userRepo.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 900
            };
        }
        public async Task<AuthResponseDto> RefreshTokenAsync(
    RefreshTokenRequest request)
        {
            var principal =
                _jwtService.GetPrincipalFromExpiredToken(
                    request.AccessToken);

            var email =
                principal.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                throw new UnauthorizedException("Invalid token");

            var user = await _userRepo.GetByEmailAsync(email);

            if (user == null)
                throw new NotFoundException("User not found");

            if (user.RefreshToken != request.RefreshToken)
                throw new UnauthorizedException("Invalid refresh token");

            if (user.RefreshTokenExpiryTime < DateTime.Now)
                throw new UnauthorizedException("Refresh token expired");

            // generate new tokens

            var newAccessToken = _jwtService.GenerateAccessToken(user);

            var newRefreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);

            await _userRepo.UpdateAsync(user);

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 900
            };
        }
    }
}
