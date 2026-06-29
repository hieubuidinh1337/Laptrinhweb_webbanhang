using BuiDinhHieu_2380600640.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuiDinhHieu_2380600640.Repositories.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<List<Category>> GetAllActiveAsync();
        new Task<Category?> GetByIdAsync(object id);
        Task<Category?> GetBySlugAsync(string slug);
        Task UpdateAsync(Category category);
    }
}
