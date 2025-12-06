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
        public async Task<IActionResult> Index()
        {
            var vm = new DashboardVM();

            // 1. Tính tổng doanh thu (Chỉ tính đơn đã hoàn thành)
            vm.TotalRevenue = await _context.Orders
                .SumAsync(o => o.TotalAmount);

            // 2. Đếm số lượng
            vm.TotalOrders = await _context.Orders.CountAsync();
            vm.TotalProducts = await _context.Products.CountAsync();
            vm.TotalUsers = await _context.Users.CountAsync();

            // 3. Lấy 5 đơn hàng mới nhất
            vm.RecentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            // 4. Lấy 5 đánh giá mới nhất
            vm.RecentReviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(vm);
        }
    }
}
