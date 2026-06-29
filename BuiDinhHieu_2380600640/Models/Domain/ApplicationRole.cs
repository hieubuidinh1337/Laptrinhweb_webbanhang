using Microsoft.AspNetCore.Identity;

namespace BuiDinhHieu_2380600640.Models.Domain
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
    }
}
