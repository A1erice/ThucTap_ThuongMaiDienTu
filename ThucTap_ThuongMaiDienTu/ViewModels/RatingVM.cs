using ThucTap_ThuongMaiDienTu.Models;

namespace ThucTap_ThuongMaiDienTu.ViewModels
{
    public class RatingVM
    {
        public int MedicineId { get; set; }
        public string? Account { get; set; }
        public int Stars { get; set; } // Rating value (e.g., out of 5)
        public string? Comment { get; set; } // The actual comment
    }
}
