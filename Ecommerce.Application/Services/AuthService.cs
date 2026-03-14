using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.DTOs.Common;
using Ecommerce.Application.Interfaces.IRepositories;
using Ecommerce.Application.Interfaces.IServices;
using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IJwtService _jwtService;

        public AuthService(
            IUserRepository userRepo,
            IJwtService jwtService)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
        }

        public async Task RegisterAsync(RegisterRequestDto request)
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
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email);

            if (user == null)
                throw new Exception("Invalid email");

            var valid = PasswordHasher.Verify(
                request.Password,
                user.PasswordHash);

            if (!valid)
                throw new Exception("Invalid password");

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
