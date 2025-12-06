using System;
using System.Collections.Generic;

namespace TiramisuShop.Models;

public partial class Order
{
    public long Id { get; set; }

    public long? UserId { get; set; }

    public string Address { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual User? User { get; set; }
}
