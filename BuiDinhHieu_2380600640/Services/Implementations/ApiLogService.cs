using BuiDinhHieu_2380600640.Data;
using BuiDinhHieu_2380600640.Models.Domain;
using BuiDinhHieu_2380600640.Services.Interfaces;
using System.Threading.Tasks;

namespace BuiDinhHieu_2380600640.Services.Implementations
{
    public class ApiLogService : IApiLogService
    {
        private readonly ApplicationDbContext _context;

        public ApiLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string endpoint, string method, int statusCode, string? body, string? userId, string? ip, int durationMs)
        {
            var log = new ApiLog
            {
                Endpoint = endpoint,
                Method = method,
                StatusCode = (short)statusCode,
                RequestBody = body,
                UserId = userId,
                IpAddress = ip,
                DurationMs = durationMs
            };

            await _context.ApiLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }
    }
}
