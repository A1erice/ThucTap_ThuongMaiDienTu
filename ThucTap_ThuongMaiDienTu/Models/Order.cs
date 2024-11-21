using System;
using System.Collections.Generic;

namespace ThucTap_ThuongMaiDienTu.Models;

public partial class Order
{
    public int Id { get; set; }

    public int AccountId { get; set; }

    public DateTime Date { get; set; }

    public int? Total { get; set; }

    public string? Address { get; set; }

    public bool Status { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
