using System.Threading.Tasks;

namespace BuiDinhHieu_2380600640.Services.Interfaces
{
    public interface IApiLogService
    {
        Task LogAsync(string endpoint, string method, int statusCode, string? body, string? userId, string? ip, int durationMs);
    }
}
