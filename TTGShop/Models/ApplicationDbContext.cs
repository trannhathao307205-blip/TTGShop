using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
// 1. Thêm dòng using này để Entity Framework tìm thấy Model Order và OrderDetail trong thư mục con Cart của bạn

namespace TTGShop.Models
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImages> ProductImages { get; set; }

        // ====================================================================
        // 2. KHAI BÁO THÊM 2 DÒNG NÀY ĐỂ ENTITY FRAMEWORK TỰ ĐỘNG TẠO BẢNG DƯỚI SQL
        // ====================================================================
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}