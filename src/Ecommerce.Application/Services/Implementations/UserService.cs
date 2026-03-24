using Ecommerce.Application.Common.Caching;
using Ecommerce.Application.Common.Pagination;
using Ecommerce.Application.Common.Responses;
using Ecommerce.Application.DTOs.User;
using Ecommerce.Application.Exceptions;
using Ecommerce.Application.Services.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using System.Threading;

namespace Ecommerce.Application.Services.Implementations
{
    public class UserService : IUserService
    {
        private static readonly TimeSpan UserCacheTtl = TimeSpan.FromMinutes(10);
        private static readonly SemaphoreSlim _listLoadLock = new(1, 1);
        private static readonly SemaphoreSlim _itemLoadLock = new(1, 1); 
        private static readonly SemaphoreSlim _writeLock = new(1, 1);

        private readonly IUserRepo _userRepo;
        private readonly ICacheService _cacheService;
        private readonly IUserSessionInvalidationService _sessionInvalidation;

        public UserService(
            IUserRepo userRepo,
            ICacheService cacheService,
            IUserSessionInvalidationService sessionInvalidation)
        {
            _userRepo = userRepo;
            _cacheService = cacheService;
            _sessionInvalidation = sessionInvalidation;
        }

        public async Task<ApiResponse<PagedResponse<UserResponseDto>>> GetAllAsync(PaginationDto pagination)
        {
            var version = await _cacheService.GetVersionAsync(CacheKeyGenerator.UserVersionKey());
            var cacheKey = CacheKeyGenerator.UserList(version, pagination.PageNumber, pagination.PageSize);

            var pagedData = await _cacheService.GetAsync<PagedResponse<UserResponseDto>>(cacheKey);
            if (pagedData != null)
                return ApiResponse<PagedResponse<UserResponseDto>>.SuccessResponse(pagedData, "Get data successfully");

            await _listLoadLock.WaitAsync();
            try
            {
                version = await _cacheService.GetVersionAsync(CacheKeyGenerator.UserVersionKey());
                cacheKey = CacheKeyGenerator.UserList(version, pagination.PageNumber, pagination.PageSize);

                pagedData = await _cacheService.GetAsync<PagedResponse<UserResponseDto>>(cacheKey);
                if (pagedData != null)
                    return ApiResponse<PagedResponse<UserResponseDto>>.SuccessResponse(pagedData, "Get data successfully");

                var (users, totalItems) = await _userRepo.GetAllAsync(pagination);
                var data = users.Select(MapToResponseDto).ToList();
                pagedData = new PagedResponse<UserResponseDto>(data, pagination.PageNumber, pagination.PageSize, totalItems);
                await _cacheService.SetAsync(cacheKey, pagedData, UserCacheTtl);
            }
            finally
            {
                _listLoadLock.Release();
            }

            return ApiResponse<PagedResponse<UserResponseDto>>.SuccessResponse(pagedData!, "Get data successfully");
        }

        public async Task<ApiResponse<UserResponseDto?>> GetByIdAsync(int id)
        {
            var cacheKey = CacheKeyGenerator.User(id);

            // check1
            var item = await _cacheService.GetAsync<UserResponseDto>(cacheKey);
            if (item != null)
                return ApiResponse<UserResponseDto?>.SuccessResponse(item, "Get data successfully");

            await _itemLoadLock.WaitAsync();
            try
            {
                // check 2
                item = await _cacheService.GetAsync<UserResponseDto>(cacheKey);
                if (item != null) return ApiResponse<UserResponseDto?>.SuccessResponse(item, "Get data successfully");

                var user = await _userRepo.GetByIdAsync(id);
                if (user == null) throw new NotFoundException("User not found");

                item = MapToResponseDto(user);
                await _cacheService.SetAsync(cacheKey, item, UserCacheTtl);
            }
            finally { _itemLoadLock.Release(); }

            return ApiResponse<UserResponseDto?>.SuccessResponse(item, "Get data successfully");
        }

        public async Task<ApiResponse<UserResponseDto>> UpdateAsync(int id, AdminUpdateUserDto dto, int adminId)
        {
            if (id == adminId) throw new BusinessException("You cannot modify your own role via this method.");

            await _writeLock.WaitAsync();
            try
            {
                var user = await _userRepo.GetByIdForUpdateAsync(id);
                if (user == null) throw new NotFoundException("User not found");

                var previousRoleId = user.RoleId;
                user.RoleId = dto.RoleId;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepo.SaveChangesAsync();

                if (previousRoleId != dto.RoleId)
                    await _sessionInvalidation.InvalidateAsync(id);

                await _cacheService.RemoveAsync(CacheKeyGenerator.User(id));
                await _cacheService.IncrementAsync(CacheKeyGenerator.UserVersionKey());

                var item = MapToResponseDto(user);
                return ApiResponse<UserResponseDto>.SuccessResponse(
                       item,
                       "Update data successfully"
                       );
            }
            finally
            {
                _writeLock.Release();
            }
        }
        public async Task<ApiResponse<UserResponseDto>> DeleteAsync(int id, int adminId)
        {
            if (id == adminId) throw new BusinessException("You cannot delete your own admin account.");

            await _writeLock.WaitAsync();
            try
            {

                var user = await _userRepo.GetByIdForUpdateAsync(id);
            if (user == null) throw new NotFoundException("User not found");

            user.IsDeleted = true;

            await _userRepo.SaveChangesAsync();

            await _sessionInvalidation.InvalidateAsync(id);

            await _cacheService.RemoveAsync(CacheKeyGenerator.User(id));
            await _cacheService.IncrementAsync(CacheKeyGenerator.UserVersionKey());

            var item = MapToResponseDto(user);
            return ApiResponse<UserResponseDto>.SuccessResponse(
                     item,
                    "Delete data successfully"
            );
            }
            finally
            {
                _writeLock.Release();
            }
        }
        private static UserResponseDto MapToResponseDto(User u) => new()
        {
            Id = u.Id,
            Email = u.Email,
            RoleName = u.Role.Name,
            CreatedAt = u.CreatedAt,
            UpdatedAt = u.UpdatedAt
        };
    }
}
