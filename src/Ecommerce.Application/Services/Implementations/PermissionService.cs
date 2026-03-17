using Ecommerce.Application.Common.Pagination;
using Ecommerce.Application.Common.Responses;
using Ecommerce.Application.DTOs.Permission;
using Ecommerce.Application.DTOs.Role;
using Ecommerce.Application.Exceptions;
using Ecommerce.Application.Services.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Services.Implementations
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepo _permissionRepo;
        public PermissionService( IPermissionRepo permissionRepo)
        {
            _permissionRepo = permissionRepo;
        }
        public async Task<ApiResponse<PagedResponse<PermissionResponseDto>>> GetAllAsync(PaginationDto pagedto)
        {
            var (permissions, totalCount) = await _permissionRepo.GetAllAsync(pagedto);

            var data = permissions.Select(r => MapToResponseDto(r)).ToList();

            var pagedResponse = new PagedResponse<PermissionResponseDto>(
                data,
                totalCount,
                pagedto.PageNumber,
                pagedto.PageSize);

            return ApiResponse<PagedResponse<PermissionResponseDto>>.SuccessResponse(pagedResponse);

        }
        public async Task<ApiResponse<PermissionResponseDto>> GetByIdAsync(int id) 
        {
            var permission = await _permissionRepo.GetByIdAsync(id);
            if (permission == null) throw new NotFoundException("Role not found");

            var dto = new PermissionResponseDto
            {
                Id = permission.Id,
                Name = permission.Name,
                Description = permission.Description,
                
            };

            return ApiResponse<PermissionResponseDto>.SuccessResponse(dto);
        }

        private PermissionResponseDto MapToResponseDto(Permission p) => new PermissionResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description
        };
    }
}
