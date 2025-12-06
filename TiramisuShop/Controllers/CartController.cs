using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiramisuShop.Models;
using System.Security.Claims;

namespace TiramisuShop.Controllers
{
    public class CartController : Controller
    {
        private readonly TiramisuShopContext _context;

        public CartController(TiramisuShopContext context)
        {
            _context = context;
        }

        // GET: /Cart (Xem giỏ hàng - sẽ làm View sau)
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (userId == -1) return RedirectToAction("Login", "User", new { returnUrl = "/Cart" });

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return View(cart);
        }

        // POST: /Cart/AddToCart (API cho AJAX)
        [HttpPost]
        public async Task<IActionResult> AddToCart(long productId, int quantity = 1)
        {
            // 1. Kiểm tra đăng nhập
            var userId = GetUserId();
            if (userId == -1)
            {
                return Json(new { success = false, requireLogin = true, message = "Vui lòng đăng nhập để mua hàng." });
            }

            try
            {
                // 2. Lấy hoặc tạo Giỏ hàng cho User
                var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
                if (cart == null)
                {
                    cart = new Cart { UserId = userId };
                    _context.Carts.Add(cart);
                    await _context.SaveChangesAsync();
                }

                // 3. Kiểm tra sản phẩm đã có trong giỏ chưa
                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);

                if (cartItem != null)
                {
                    // Đã có -> Cộng dồn số lượng
                    cartItem.Quantity += quantity;
                }
                else
                {
                    // Chưa có -> Thêm mới
                    cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = productId,
                        Quantity = quantity
                    };
                    _context.CartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();

                // 4. Tính tổng số lượng item để cập nhật Badge trên Header
                var totalCount = await _context.CartItems.Where(ci => ci.CartId == cart.Id).SumAsync(ci => ci.Quantity);

                return Json(new { success = true, message = "Đã thêm vào giỏ hàng!", cartCount = totalCount });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        // API: Lấy số lượng giỏ hàng hiện tại (Để update header khi reload trang)
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = GetUserId();
            if (userId == -1) return Json(0);

            var count = await _context.CartItems
                .Where(ci => ci.Cart.UserId == userId)
                .SumAsync(ci => ci.Quantity);

            return Json(count);
        }

        // Helper: Lấy UserId từ Claims
        private long GetUserId()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long userId))
            {
                return userId;
            }
            return -1;
        }
    }
}