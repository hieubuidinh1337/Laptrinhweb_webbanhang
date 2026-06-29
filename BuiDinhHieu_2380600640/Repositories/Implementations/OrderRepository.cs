using BuiDinhHieu_2380600640.Models.Domain;
using BuiDinhHieu_2380600640.Data;
using BuiDinhHieu_2380600640.Repositories.Interfaces;

namespace BuiDinhHieu_2380600640.Repositories.Implementations
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
