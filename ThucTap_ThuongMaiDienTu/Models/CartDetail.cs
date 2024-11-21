using System;
using System.Collections.Generic;

namespace ThucTap_ThuongMaiDienTu.Models;

public partial class CartDetail
{
    public int CartId { get; set; }

    public int MedicineId { get; set; }

    public int? Amount { get; set; }

    public int? Total { get; set; }

    public virtual Cart Cart { get; set; } = null!;

    public virtual Medicine Medicine { get; set; } = null!;
}
