using System;
using System.Collections.Generic;

namespace TiramisuShop.Models;

public partial class CartItem
{
    public long Id { get; set; }

    public long CartId { get; set; }

    public long ProductId { get; set; }

    public long Quantity { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
