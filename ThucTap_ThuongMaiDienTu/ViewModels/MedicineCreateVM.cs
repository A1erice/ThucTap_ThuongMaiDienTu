using System.ComponentModel.DataAnnotations;

namespace ThucTap_ThuongMaiDienTu.ViewModels
{
    public class MedicineCreateVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Category is required.")]
        public int CategoryId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a non-negative number.")]
        public int? Quantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Price must be a non-negative number.")]
        public int? Price { get; set; }

        public string? Unit { get; set; }

        public string? Description { get; set; }

        public string? Uses { get; set; }

        public IFormFile? ImgFile { get; set; }
        public string? Img { get; set; }

        public int? Sold { get; set; }

        public string? Ingredient { get; set; }

        public string? Pack { get; set; }

        // Additional properties can be added here if needed
    }
}
