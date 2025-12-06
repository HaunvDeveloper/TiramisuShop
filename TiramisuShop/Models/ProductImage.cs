using System;
using System.Collections.Generic;

namespace TiramisuShop.Models;

public partial class ProductImage
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
