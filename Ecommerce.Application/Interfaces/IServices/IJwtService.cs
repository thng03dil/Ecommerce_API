using Ecommerce.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces.IServices
{
    internal interface IJwtService
    {
        public interface IJwtService
        {
            string GenerateAccessToken(User user);

            string GenerateRefreshToken();

            ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        }
    }
