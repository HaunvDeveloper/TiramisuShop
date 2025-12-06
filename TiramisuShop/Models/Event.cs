using System;
using System.Collections.Generic;

namespace TiramisuShop.Models;

public partial class Event
{
    public long Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public double DiscountPercent { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
