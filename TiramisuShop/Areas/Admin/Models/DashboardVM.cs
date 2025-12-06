using TiramisuShop.Models;

namespace TiramisuShop.Areas.Admin.Models
{
    public class DashboardVM
    {
        // Thống kê số liệu
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<Review> RecentReviews { get; set; } = new List<Review>();

        // --- THÊM MỚI: DỮ LIỆU BIỂU ĐỒ ---
        public List<string> ChartLabels { get; set; } = new List<string>();
        public List<decimal> ChartData { get; set; } = new List<decimal>();
    }
}
