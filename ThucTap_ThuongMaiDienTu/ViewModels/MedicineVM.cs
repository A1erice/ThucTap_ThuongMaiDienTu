using System;
using System.Collections.Generic;

namespace ThucTap_ThuongMaiDienTu.ViewModels;

public partial class MedicineVM
{
    public int Id { get; set; }

    public string? Img { get; set; }
    public string Category { get; set; }
    public int CategoryID { get; set; }
    public int Price { get; set; }
    public string? Description { get; set; }
    public string? Uses { get; set; }
    public string? Unit { get; set; }
    public string? Name { get; set; }
    public double AverageRating { get; set; }
    public int RatingsCount { get; set; }

    public string? Ingredient { get; set; }

    public string? Pack { get; set; }
    public List<RatingVM> Ratings { get; set; } 

}
