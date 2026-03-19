
using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.DTOs.Common;
using Ecommerce.Application.Exceptions;
using Ecommerce.Application.Extensions;
using Ecommerce.Application.Services.Interfaces;
using Ecommerce.Domain.Common.Settings;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Ecommerce.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepo _userRepo;
        private readonly IRefreshTokenRepo _refreshTokenRepo;
        private readonly IRoleRepo _roleRepo;
        private readonly IJwtService _jwtService;
        private readonly JwtSettings _jwtSettings;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IDeviceService _deviceService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<AuthService> _logger;
        public AuthService(
            IUserRepo userRepo,
            IRoleRepo roleRepo,
            IJwtService jwtService,
            IOptions<JwtSettings> jwtSettings,
            IPasswordHasher passwordHasher,
            IRefreshTokenRepo refreshTokenRepo,
            IDeviceService deviceService,
            ICacheService cacheService,
            ILogger<AuthService> logger
            )
        {
            _userRepo = userRepo; 
            _roleRepo = roleRepo;
            _jwtService = jwtService;
            _jwtSettings = jwtSettings.Value;
            _passwordHasher = passwordHasher;
            _refreshTokenRepo = refreshTokenRepo;
            _deviceService = deviceService;
            _cacheService = cacheService;
            _logger = logger;
        } 

        public async Task RegisterAsync(RegisterDto request) 
        {
            var exist = await _userRepo.GetByEmailAsync(request.Email);

            if (exist != null) throw new ConflictException("Email already exists");
            
            var defaultRole = await _roleRepo.GetByNameRoleAsync("User");

            if (defaultRole == null)
                throw new Exception("System role configuration error");
            
            var user = new User
            {
                Email = request.Email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                RoleId = defaultRole.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto request)
        {

            var user = await _userRepo.GetByEmailAsync(request.Email);

            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedException("Invalid email or password");
            

            var userWithPermissions = await _userRepo.GetByIdWithPermissionsAsync(user.Id);

            if (userWithPermissions == null)
            {
                throw new NotFoundException("User not found");
            }

            var accessToken = _jwtService.GenerateAccessToken(userWithPermissions);
            var rawRefreshToken = _jwtService.GenerateRefreshToken();

            var tokenHash = _jwtService.HashToken(rawRefreshToken);

            var ip = _deviceService.GetIpAddress();
            var userAgent = _deviceService.GetUserAgent();
            var deviceId = _deviceService.GenerateDeviceId(userAgent, ip);

            var refreshToken = new RefreshToken(
                user.Id,
                tokenHash,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
                ip,
                userAgent,
                deviceId
            );

            await _refreshTokenRepo.AddAsync(refreshToken);
            await _refreshTokenRepo.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                ExpiresIn = _jwtSettings.ExpiryMinutes * 60
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);

            var userId = principal.GetUserId();

            if (userId <= 0) throw new UnauthorizedException("Invalid user identification");
            var user = await _userRepo.GetByIdWithPermissionsAsync(userId);

            if (user == null || user.IsDeleted)
                throw new UnauthorizedException("Invalid user");

            var tokenHash = _jwtService.HashToken(request.RefreshToken);

            var storedToken = await _refreshTokenRepo.GetByTokenHashAsync(tokenHash);

            if (storedToken == null)
                throw new UnauthorizedException("Invalid refresh token");

            if (storedToken.ExpiryDate <= DateTime.UtcNow)
                throw new UnauthorizedException("Refresh token expired");

            var ip = _deviceService.GetIpAddress();
            var userAgent = _deviceService.GetUserAgent();
            var deviceId = _deviceService.GenerateDeviceId(userAgent, ip);

            // device binding check
            if (storedToken.DeviceId != deviceId)
            {
                user.RevokeAllTokens();
                await _refreshTokenRepo.SaveChangesAsync();

                throw new UnauthorizedException("Token used from different device");
            }

            // Reuse Detection
            if (storedToken.IsRevoked)
            {
                user.RevokeAllTokens();
                await _refreshTokenRepo.SaveChangesAsync();
                throw new UnauthorizedException("Token reuse detected");
            }

            // rotation
            storedToken.Revoke();

            var newAccessToken = _jwtService.GenerateAccessToken(user);
            var newRawRefreshToken = _jwtService.GenerateRefreshToken();

            var newHash = _jwtService.HashToken(newRawRefreshToken);

            var newToken = new RefreshToken(
                user.Id,
                newHash,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
                ip,
                userAgent,
                deviceId
            );

            await _refreshTokenRepo.AddAsync(newToken);
            await _refreshTokenRepo.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRawRefreshToken,
                ExpiresIn = _jwtSettings.ExpiryMinutes * 60
            };
        }

        public async Task<bool> HasPermissionAsync(int userId, string permission)
        {
            if (userId <= 0) return false;
            if (string.IsNullOrWhiteSpace(permission)) return false;

            var user = await _userRepo.GetByIdWithPermissionsAsync(userId);
            if (user?.Role?.RolePermissions == null) return false;

            var normalized = permission.Trim().ToLowerInvariant();
            return user.Role.RolePermissions.Any(rp => rp.Permission.Name == normalized);
        }

        public async Task<UserMeResponseDto> GetMeAsync(int userId)
        {
            if (userId <= 0) throw new UnauthorizedException("Unauthorized");

            var user = await _userRepo.GetByIdWithPermissionsAsync(userId);
            if (user == null) throw new NotFoundException("User not found");

            var permissions = user.Role?.RolePermissions?
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .OrderBy(x => x)
                .ToList() ?? new List<string>();

            return new UserMeResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role?.Name ?? string.Empty,
                Permissions = permissions,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task LogoutAsync(int userId, string accessToken)
        {
            if (userId <= 0) throw new UnauthorizedException("Unauthorized");

            var user = await _userRepo.GetByIdForUpdateAsync(userId);
            if (user == null) throw new NotFoundException("User not found");

            user.RevokeAllTokens();

            await _userRepo.SaveChangesAsync();
            try
            {
                var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
                var expClaim = principal.FindFirst("exp")?.Value;

                if (long.TryParse(expClaim, out var expUnix))
                {
                    var expiryDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                    var remainingTime = expiryDate - DateTime.UtcNow;

                    if (remainingTime.TotalSeconds > 0)
                    {
                        // Key format: Blacklist:AccessToken:{Hash}
                        string blacklistKey = $"Blacklist:Token:{accessToken}";
                        await _cacheService.SetAsync(blacklistKey, "revoked", remainingTime);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Blacklisting failed for a token. It might be already invalid. Error: {Message}", ex.Message);
            }
        }
    }
}
