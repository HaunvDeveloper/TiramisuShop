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
                .Include(c => c.Product).ThenInclude(p => p.Event) // <--- QUAN TRỌNG: Include Event
                .Where(c => c.Cart.UserId == userId && idList.Contains(c.Id))
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            // Tính tổng tiền dựa trên giá đã giảm
            decimal totalAmount = 0;
            foreach (var item in cartItems)
            {
                totalAmount += CalculateDiscountPrice(item.Product) * item.Quantity;
            }

            var vm = new CheckoutVM
            {
                CheckoutItems = cartItems,
                TotalAmount = totalAmount, // Giá đã giảm
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
                    .ThenInclude(p => p.Event) // <--- Include Event để tính giá lưu DB
                .Where(c => c.Cart.UserId == userId && idList.Contains(c.Id))
                .ToListAsync();
            decimal orderTotal = 0;
            foreach (var item in cartItems)
            {
                orderTotal += CalculateDiscountPrice(item.Product) * item.Quantity;
            }
            if (ModelState.IsValid)
            {
                var order = new Order
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    Status = "Pending",
                    PaymentMethod = model.PaymentMethod,
                    TotalAmount = orderTotal,

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
                        Price = CalculateDiscountPrice(item.Product)
                    };
                    _context.OrderItems.Add(orderItem);
                    item.Product.Stock -= item.Quantity;
                }

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                return RedirectToAction("Success", new { orderId = order.Id });
            }

            model.CheckoutItems = cartItems;
            model.TotalAmount = orderTotal;
            return View(model);
        }

        public IActionResult Success(long orderId)
        {
            return View(orderId);
        }

        private decimal CalculateDiscountPrice(Product product)
        {
            var now = DateTime.Now;
            if (product.Event != null && now >= product.Event.StartDate && now <= product.Event.EndDate)
            {
                return product.Price * (decimal)((100 - product.Event.DiscountPercent) / 100);
            }
            return product.Price;
        }
    }
}