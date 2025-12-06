using TiramisuShop.Models;

namespace TiramisuShop.Areas.Admin.Models
{
    public class DashboardVM
    {
        // Thống kê số liệu
        public decimal TotalRevenue { get; set; } // Tổng doanh thu
        public int TotalOrders { get; set; }      // Tổng đơn hàng
        public int TotalProducts { get; set; }    // Tổng sản phẩm
        public int TotalUsers { get; set; }       // Tổng thành viên

        // Danh sách mới nhất
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<Review> RecentReviews { get; set; } = new List<Review>();
    }
}
