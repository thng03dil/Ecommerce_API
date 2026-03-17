using Ecommerce.Application.Common.Pagination;
using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Domain.Interfaces
{
    public interface IPermissionRepo
    {
        Task<(IEnumerable<Permission>, int totalCount)> GetAllAsync(PaginationDto pagedto);
        Task<Permission?> GetByIdAsync(int id);
        Task<bool> AllIdsExistAsync(List<int> ids);
        Task<bool> IsPermissionIdExistAsync(int id);
    }
}
