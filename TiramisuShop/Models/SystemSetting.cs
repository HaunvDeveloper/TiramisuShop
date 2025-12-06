using System;
using System.Collections.Generic;

namespace TiramisuShop.Models;

public partial class SystemSetting
{
    public string Key { get; set; } = null!;

    public string? Value { get; set; }
}
