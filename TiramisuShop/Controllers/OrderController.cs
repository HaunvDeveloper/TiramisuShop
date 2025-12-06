using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiramisuShop.Models;
using TiramisuShop.ViewModels;
using System.Security.Claims;

namespace TiramisuShop.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được vào Controller này
    public class OrderController : Controller
    {
        private readonly TiramisuShopContext _context;

        public OrderController(TiramisuShopContext context)
        {
            _context = context;
        }

        // GET: /Order
        public async Task<IActionResult> Index()
        {
            var userId = long.Parse(User.FindFirst("UserId")?.Value ?? "0");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImages)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(orders);
        }


        // GET: /Order/Create?itemIds=1,2,3
        [HttpGet]
        public async Task<IActionResult> Create(string itemIds)
        {
            if (string.IsNullOrEmpty(itemIds)) return RedirectToAction("Index", "Cart");

            var userId = long.Parse(User.FindFirst("UserId").Value);
            var idList = itemIds.Split(',').Select(long.Parse).ToList();

            // Lấy các món hàng user đã chọn trong giỏ
            var cartItems = await _context.CartItems
                .Include(c => c.Product).ThenInclude(p => p.ProductImages)
                .Where(c => c.Cart.UserId == userId && idList.Contains(c.Id))
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            // Chuẩn bị ViewModel
            var vm = new CheckoutVM
            {
                CheckoutItems = cartItems,
                TotalAmount = cartItems.Sum(c => c.Quantity * c.Product.Price),
                // Điền sẵn thông tin user nếu có
                ReceiverName = User.FindFirst("FullName")?.Value,
                ReceiverPhone = await _context.Users.Where(u => u.Id == userId).Select(u => u.Phone).FirstOrDefaultAsync()
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CheckoutVM model, string itemIds)
        {
            var userId = long.Parse(User.FindFirst("UserId").Value);
            var idList = itemIds.Split(',').Select(long.Parse).ToList();

            var cartItems = await _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.Cart.UserId == userId && idList.Contains(c.Id))
                .ToListAsync();

            if (ModelState.IsValid)
            {
                var order = new Order
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    Status = "Pending",
                    PaymentMethod = model.PaymentMethod,
                    TotalAmount = cartItems.Sum(c => c.Quantity * c.Product.Price),

                    // --- SỬA ĐOẠN NÀY: Bỏ WardName ---
                    // Định dạng: Số nhà, Quận Huyện, Tỉnh Thành
                    Address = $"{model.SpecificAddress}, {model.WardName}, {model.ProvinceName}"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    };
                    _context.OrderItems.Add(orderItem);
                    item.Product.Stock -= item.Quantity;
                }

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                return RedirectToAction("Success", new { orderId = order.Id });
            }

            model.CheckoutItems = cartItems;
            model.TotalAmount = cartItems.Sum(c => c.Quantity * c.Product.Price);
            return View(model);
        }

        public IActionResult Success(long orderId)
        {
            return View(orderId);
        }
    }
}