using System;
using System.Collections.Generic;

namespace ThucTap_ThuongMaiDienTu.Models;

public partial class Rating
{
    public int MedicineId { get; set; }

    public int AccountId { get; set; }

    public int Stars { get; set; }

    public string? Comment { get; set; }

    public string? Reply { get; set; }

    public virtual Account Account { get; set; } = null!;

    public virtual Medicine Medicine { get; set; } = null!;
}
