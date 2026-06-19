using TTGShop.Models;

namespace TTGShop.Repositories
{
    public interface IOrderRepository
    {
        Task CreateOrderAsync(Order order);
    }
}
