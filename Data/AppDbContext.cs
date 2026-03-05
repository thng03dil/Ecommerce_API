
using Microsoft.EntityFrameworkCore;
using Ecommerce_API.Models;
namespace Ecommerce_API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
                : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<Category> Categories { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            //create categories
            modelBuilder.Entity<Category>().HasData(
            new Category { 
                Id = 1,
                Name = "Điện thoại",
                Description = "Các loại smartphone mới nhất",
                slug = "dien-thoai" 
            },
            new Category { 
                Id = 2,
                Name = "Laptop",
                Description = "Máy tính xách tay làm việc và chơi game",
                slug = "laptop"
            },
            new Category {
                Id = 3,
                Name = "Phụ kiện",
                Description = "Tai nghe, sạc, cáp...",
                slug = "phu-kien"
            }
            );

            //create products
            modelBuilder.Entity<Product>().HasData(
            new Product
            {
                Id = 1,
                Name = "iPhone 15 Pro",
                Price = 25000000m,
                Stock = 50,
                CategoryId = 1 
            },
            new Product
            {
                Id = 2,
                Name = "MacBook M3",
                Price = 35000000m,
                Stock = 20,
                CategoryId = 2 
            }
        );
        }

    }
}
