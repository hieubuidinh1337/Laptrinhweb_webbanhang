using BuiDinhHieu_2380600640.Models.Domain;
using BuiDinhHieu_2380600640.Models.DTOs;
using System.Threading.Tasks;

namespace BuiDinhHieu_2380600640.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartViewModel> GetCartAsync(string sessionKey);
        Task AddItemAsync(string sessionKey, AddToCartRequest req, string? userId = null);
        Task UpdateQuantityAsync(string sessionKey, int cartItemId, int qty);
        Task RemoveItemAsync(string sessionKey, int cartItemId);
        Task<int> GetCountAsync(string sessionKey);
        Task MergeAsync(string guestKey, string userKey, string userId);
        Task ClearAsync(string sessionKey);
    }
}
