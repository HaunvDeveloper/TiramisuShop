using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiramisuShop.Models;

namespace TiramisuShop.Controllers
{
    public class EventController : Controller
    {
        private readonly TiramisuShopContext _context;

        public EventController(TiramisuShopContext context)
        {
            _context = context;
        }

        // GET: Event
        public async Task<IActionResult> Index()
        {
            // Lấy danh sách sự kiện, sắp xếp ngày bắt đầu giảm dần (mới nhất lên đầu)
            var events = await _context.Events
                                       .OrderByDescending(e => e.StartDate)
                                       .ToListAsync();
            return View(events);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Events
                .Include(e => e.Products)
                    .ThenInclude(p => p.ProductImages) // Quan trọng: Lấy ảnh sản phẩm
                .FirstOrDefaultAsync(m => m.Id == id);

            if (@event == null) return NotFound();

            return View(@event);
        }
    }
}