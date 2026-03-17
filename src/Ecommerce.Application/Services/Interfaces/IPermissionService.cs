using Ecommerce.Application.Common.Pagination;
using Ecommerce.Application.Common.Responses;
using Ecommerce.Application.DTOs.Permission;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Application.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<ApiResponse<PagedResponse<PermissionResponseDto>>> GetAllAsync(PaginationDto pagedto);
        Task<ApiResponse<PermissionResponseDto>> GetByIdAsync(int id);
    }
}
