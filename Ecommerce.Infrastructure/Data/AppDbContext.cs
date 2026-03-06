using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Infrastructure.Data
{
    public class AppDbContext: DbContext, IAppDbContext
    {

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
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
            new Category
            {
                Id = 1,
                Name = "Điện thoại",
                Description = "Các loại smartphone mới nhất",
                slug = "dien-thoai"
            },
            new Category
            {
                Id = 2,
                Name = "Laptop",
                Description = "Máy tính xách tay làm việc và chơi game",
                slug = "laptop"
            },
            new Category
            {
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
                Description = "iPhone 15 Pro với chip A17 Pro và camera nâng cấp",
                Price = 25000000,
                Stock = 20,
                ImageUrl = "https://example.com/images/iphone15pro.jpg",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1),
                CategoryId = 1
            },

            new Product
            {
                Id = 2,
                Name = "Samsung Galaxy S24",
                Description = "Samsung flagship với AI và camera 200MP",
                Price = 22000000,
                Stock = 15,
                ImageUrl = "https://example.com/images/galaxy-s24.jpg",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1),
                CategoryId = 1
            },

            new Product
            {
                Id = 3,
                Name = "MacBook Air M2",
                Description = "Laptop mỏng nhẹ chip Apple M2 hiệu năng cao",
                Price = 28000000,
                Stock = 10,
                ImageUrl = "https://example.com/images/macbook-air-m2.jpg",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1),
                CategoryId = 2
            },

            new Product
            {
                Id = 4,
                Name = "Dell XPS 13",
                Description = "Laptop cao cấp màn hình InfinityEdge",
                Price = 32000000,
                Stock = 8,
                ImageUrl = "https://example.com/images/dell-xps13.jpg",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1),
                CategoryId = 2
            },

            new Product
            {
                Id = 5,
                Name = "AirPods Pro 2",
                Description = "Tai nghe không dây chống ồn chủ động",
                Price = 6000000,
                Stock = 30,
                ImageUrl = "https://example.com/images/airpods-pro2.jpg",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1),
                CategoryId = 3
            },

            new Product
            {
                Id = 6,
                Name = "Sạc nhanh 65W",
                Description = "Củ sạc nhanh cho điện thoại và laptop",
                Price = 500000,
                Stock = 50,
                ImageUrl = "https://example.com/images/charger-65w.jpg",
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1),
                CategoryId = 3
            }

        );
        }
    }
}
