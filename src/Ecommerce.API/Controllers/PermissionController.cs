using Ecommerce.Application.Authorization;
using Ecommerce.Application.Common.Pagination;
using Ecommerce.Application.Services.Implementations;
using Ecommerce.Application.Services.Interfaces;
using Ecommerce.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionController : Controller
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        [Permission(Permissions.ViewPermission)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationDto pagination)
        {
            var result = await _permissionService.GetAllAsync(pagination);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Permission(Permissions.ViewByIdPermission)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _permissionService.GetByIdAsync(id);
            return Ok(result);
        }

    }
}
