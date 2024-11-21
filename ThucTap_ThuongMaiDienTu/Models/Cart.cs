using System;
using System.Collections.Generic;

namespace ThucTap_ThuongMaiDienTu.Models;

public partial class Cart
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public int? Total { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();
}
