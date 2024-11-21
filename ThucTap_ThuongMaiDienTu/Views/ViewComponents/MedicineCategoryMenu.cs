using Microsoft.AspNetCore.Mvc;
using ThucTap_ThuongMaiDienTu.ViewModels;
using ThucTap_ThuongMaiDienTu.Models;

namespace ThucTap_ThuongMaiDienTu.Views.ViewComponents

{
    public class MedicineCategoryMenu : ViewComponent
    {
        private readonly HisContext db;

        public MedicineCategoryMenu(HisContext context) => db = context;
        public IViewComponentResult Invoke()
        {
            var data = db.MedicineCategories.Select(c => new MedicineCategoryVM
            {
                Id = c.Id,
                Name = c.Name,
                Number = c.Medicines.Count 
            });
            return View("/Views/Shared/Components/MedicineCategoryMenu.cshtml", data);
        }
    }
}
