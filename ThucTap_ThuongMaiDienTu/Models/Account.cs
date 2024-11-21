using System;
using System.Collections.Generic;

namespace ThucTap_ThuongMaiDienTu.Models;

public partial class Account
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string AccountType { get; set; } = null!;

    public string? Address { get; set; }

    public string Code { get; set; } = null!;

    public bool Active { get; set; }

    public string Email { get; set; } = null!;

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
