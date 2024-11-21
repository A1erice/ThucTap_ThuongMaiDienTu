using System;
using System.Collections.Generic;

namespace ThucTap_ThuongMaiDienTu.Models;

public partial class OrderDetail
{
    public int OrderId { get; set; }

    public int MedicineId { get; set; }

    public int? Amount { get; set; }

    public int? MedicinePrice { get; set; }

    public int? SumPrice { get; set; }

    public virtual Medicine Medicine { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
