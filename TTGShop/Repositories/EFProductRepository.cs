using TTGShop.Models;
using Microsoft.EntityFrameworkCore;

namespace TTGShop.Repositories
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;
        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            // return await _context.Products.ToListAsync();
            return await _context.Products
            .Include(p => p.Category) // Include thông tin về category
            .ToListAsync();
        }
        public async Task<Product> GetByIdAsync(int id)
        {
            // return await _context.Products.FindAsync(id);
            // lấy thông tin kèm theo category
            return await _context.Products.Include(p =>
            p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        //Xử lý Logic Tìm kiếm Thông minh (Fuzzy / Partial Search)

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await _context.Products.ToListAsync();
            }

            searchTerm = searchTerm.Trim().ToLower();

            // Tìm kiếm gần đúng theo tên sản phẩm hoặc mô tả (nếu có)
            return await _context.Products
                .Where(p => p.Name.ToLower().Contains(searchTerm)
                         || (p.Category != null && p.Category.Name.ToLower().Contains(searchTerm)))
                .Include(p => p.Category) // Nếu cần hiển thị tên danh mục
                .ToListAsync();
        }
    }
}
