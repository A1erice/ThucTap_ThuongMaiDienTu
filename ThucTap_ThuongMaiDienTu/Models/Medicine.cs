using System;
using System.Collections.Generic;

namespace ThucTap_ThuongMaiDienTu.Models;

public partial class Medicine
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int CategoryId { get; set; }

    public int? Quantity { get; set; }

    public int? Price { get; set; }

    public string? Unit { get; set; }

    public string? Description { get; set; }

    public string? Uses { get; set; }

    public string? Img { get; set; }

    public int? Sold { get; set; }

    public string? Ingredient { get; set; }

    public string? Pack { get; set; }

    public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

    public virtual MedicineCategory Category { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
