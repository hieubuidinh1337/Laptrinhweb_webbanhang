using BuiDinhHieu_2380600640.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuiDinhHieu_2380600640.Repositories.Interfaces
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<List<Review>> GetByProductIdAsync(int productId, bool approvedOnly = true);
        Task<Dictionary<int, int>> GetRatingBreakdownAsync(int productId);
        Task<bool> UserHasPurchasedAsync(string userId, int productId);
    }
}
