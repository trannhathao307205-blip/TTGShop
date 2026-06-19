using TTGShop.Models;

namespace TTGShop.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        //Xử lý Logic Tìm kiếm Thông minh (Fuzzy / Partial Search)
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
    }
}
