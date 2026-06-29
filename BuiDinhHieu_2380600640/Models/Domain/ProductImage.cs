using System;
using System.Collections.Generic;

namespace BuiDinhHieu_2380600640.Models.Domain;

public partial class ProductImage
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string Url { get; set; } = null!;

    public string? FilePath { get; set; }

    public string? AltText { get; set; }

    public byte SortOrder { get; set; }

    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
