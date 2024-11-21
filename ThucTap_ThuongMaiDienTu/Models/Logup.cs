using System;
using System.Collections.Generic;

namespace ThucTap_ThuongMaiDienTu.Models;

public partial class Logup
{

    public string? Name { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;
    public string CFPassword { get; set; } = null!;

    public string? Address { get; set; }
    public string Email { get; set; } = null!;
}
