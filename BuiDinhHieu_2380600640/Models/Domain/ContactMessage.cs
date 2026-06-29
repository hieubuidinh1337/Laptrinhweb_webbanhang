using System;
using System.Collections.Generic;

namespace BuiDinhHieu_2380600640.Models.Domain;

public partial class ContactMessage
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string Subject { get; set; } = null!;

    public string Message { get; set; } = null!;

    public bool IsRead { get; set; }

    public bool IsReplied { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? RepliedAt { get; set; }

    public string? RepliedById { get; set; }

    public virtual ApplicationUser? RepliedBy { get; set; }
}
