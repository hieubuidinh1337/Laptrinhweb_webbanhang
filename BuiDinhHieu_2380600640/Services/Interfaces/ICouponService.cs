using System.Threading.Tasks;

namespace BuiDinhHieu_2380600640.Services.Interfaces
{
    public interface ICouponService
    {
        Task<CouponValidationResult> ValidateAsync(string code, decimal subTotal);
        Task IncrementUsageAsync(string code);
    }

    public class CouponValidationResult
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? DiscountText { get; set; }
    }
}
