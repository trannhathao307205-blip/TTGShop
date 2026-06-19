using Microsoft.EntityFrameworkCore;
using TTGShop.Models;

namespace TTGShop.Repositories
{
    public class EFCategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;
        public EFCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            // return await _context.Categories.ToListAsync();
            return await _context.Categories
            .Include(c => c.Products) // Include thông tin về products      
            .ToListAsync();
        }
        public async Task<Category> GetByIdAsync(int id)
        {
            // return await _context.Categories.FindAsync(id);
            // lấy thông tin kèm theo products
            return await _context.Categories.Include(c =>
            c.Products).FirstOrDefaultAsync(c => c.Id == id);
        }
        public async Task AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
