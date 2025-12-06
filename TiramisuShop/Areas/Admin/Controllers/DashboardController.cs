using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiramisuShop.Areas.Admin.Models;
using TiramisuShop.Models;

namespace TiramisuShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly TiramisuShopContext _context;

        public DashboardController(TiramisuShopContext context)
        {
            _context = context;
        }

        // GET: /Admin/Dashboard
        // GET: /Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            var vm = new DashboardVM();

            // 1, 2, 3, 4. Các thống kê tổng quan (Giữ nguyên code cũ)
            vm.TotalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
            vm.TotalOrders = await _context.Orders.CountAsync();
            vm.TotalProducts = await _context.Products.CountAsync();
            vm.TotalUsers = await _context.Users.CountAsync();

            vm.RecentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            vm.RecentReviews = await _context.Reviews
                .Include(r => r.User).Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            // --- 5. TÍNH DOANH THU 7 NGÀY GẦN NHẤT (MỚI) ---
            var today = DateTime.Today;
            var sevenDaysAgo = today.AddDays(-6); // Lấy từ 6 ngày trước đến hôm nay

            // Truy vấn Group By theo ngày
            // Lưu ý: EF Core đôi khi không hỗ trợ GroupBy DateTime.Date trực tiếp tốt với SQL Server
            // Nên ta lấy dữ liệu thô về rồi xử lý trên RAM (nếu dữ liệu không quá lớn) hoặc dùng SQL Raw.
            // Dưới đây là cách an toàn: Lấy Order trong 7 ngày -> Xử lý C#

            var recentOrdersData = await _context.Orders
                .Where(o => o.CreatedAt >= sevenDaysAgo) // Chỉ lấy đơn trong 7 ngày
                .Select(o => new { o.CreatedAt, o.TotalAmount })
                .ToListAsync();

            // Chuẩn bị mảng 7 ngày liên tiếp để đảm bảo ngày nào không có đơn vẫn hiện số 0
            for (int i = 0; i < 7; i++)
            {
                var date = sevenDaysAgo.AddDays(i);
                var dateLabel = date.ToString("dd/MM"); // Nhãn: 25/12

                // Tính tổng tiền của ngày đó
                var revenue = recentOrdersData
                    .Where(o => o.CreatedAt.Date == date)
                    .Sum(o => o.TotalAmount);

                vm.ChartLabels.Add(dateLabel);
                vm.ChartData.Add(revenue);
            }

            return View(vm);
        }
    }
}
