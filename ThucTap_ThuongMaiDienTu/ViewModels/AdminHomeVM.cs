using ThucTap_ThuongMaiDienTu.Models;

namespace ThucTap_ThuongMaiDienTu.ViewModels
{
    public class AdminHomeVM
    {
        public int TotalProducts { get; set; }
        public int TotalPendingOrders { get; set; }
        public int TotalSalesThisMonth { get; set; }
        public int TotalSalesAllTime { get; set; }
        public List<Order> RecentOrders { get; set; }
    }
}
