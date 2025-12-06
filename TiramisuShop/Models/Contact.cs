using System;
using System.Collections.Generic;

namespace TiramisuShop.Models;

public partial class Contact
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Subject { get; set; }

    public string Message { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public int? Status { get; set; }
}
