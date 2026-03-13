using Ecommerce_API.Models;
using Ecommerce_API.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce_API.Data.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAdminAsync(AppDbContext context)
        {
            if (!await context.Users.AnyAsync(u => u.Email == "admin@shop.com"))
            {
                var admin = new User
                {
                    Email = "admin@shop.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(admin);
                await context.SaveChangesAsync();
            }
        }
    }
}