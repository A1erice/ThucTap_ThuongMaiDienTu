using Microsoft.AspNetCore.Mvc;
using ThucTap_ThuongMaiDienTu.ViewModels;
using ThucTap_ThuongMaiDienTu.Models;

namespace ThucTap_ThuongMaiDienTu.Views.ViewComponents

{
    public class MedicineRelatedMenu : ViewComponent
    {
        private readonly HisContext db;

        public MedicineRelatedMenu(HisContext context) => db = context;
        public IViewComponentResult Invoke(int categoryID)
        {
            var data = db.Medicines
                .Where(c => c.CategoryId == categoryID)
                .Select(m => new MedicineVM
            {
                Id = m.Id,
                Name = m.Name,
                Description = m.Description,
                Price = m.Price ?? 0,
                Pack = m.Pack,
                Img = m.Img,
                Category = m.Category.Name
            });
            return View("/Views/Shared/Components/MedicineRelatedMenu.cshtml", data);
        }
    }
}
