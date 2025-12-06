using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiramisuShop.Models;
using TiramisuShop.Helpers; // Import Helper vừa tạo

namespace TiramisuShop.Controllers
{
    public class CartController : Controller
    {
        private readonly TiramisuShopContext _context;
        private const string CART_KEY = "GuestCart"; // Key lưu session

        public CartController(TiramisuShopContext context)
        {
            _context = context;
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            Cart cart;

            if (userId != -1)
            {
                // --- USER ĐÃ ĐĂNG NHẬP (Lấy từ DB) ---
                cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                            .ThenInclude(p => p.ProductImages)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
            }
            else
            {
                // --- KHÁCH VÃNG LAI (Lấy từ Session) ---
                var sessionItems = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();

                // Vì trong session chỉ lưu ProductId, ta cần load thông tin Product từ DB để hiển thị
                foreach (var item in sessionItems)
                {
                    item.Product = await _context.Products
                        .Include(p => p.ProductImages)
                        .Include(p => p.Category)
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);
                }

                cart = new Cart { CartItems = sessionItems };
            }

            // Nếu cart null (user mới chưa có gì), khởi tạo rỗng
            if (cart == null)
            {
                cart = new Cart { CartItems = new List<CartItem>() };
            }

            return View(cart);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(long productId, int quantity = 1)
        {
            var userId = GetUserId();

            if (userId != -1)
            {
                // --- LOGIC CHO USER ĐÃ ĐĂNG NHẬP (Lưu DB) ---
                try
                {
                    var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
                    if (cart == null)
                    {
                        cart = new Cart { UserId = userId };
                        _context.Carts.Add(cart);
                        await _context.SaveChangesAsync();
                    }

                    var cartItem = await _context.CartItems
                        .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == productId);

                    if (cartItem != null)
                    {
                        cartItem.Quantity += quantity;
                    }
                    else
                    {
                        cartItem = new CartItem { CartId = cart.Id, ProductId = productId, Quantity = quantity };
                        _context.CartItems.Add(cartItem);
                    }

                    await _context.SaveChangesAsync();
                    var totalCount = await _context.CartItems.Where(ci => ci.CartId == cart.Id).SumAsync(ci => ci.Quantity);
                    return Json(new { success = true, message = "Đã thêm vào giỏ hàng!", cartCount = totalCount });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi: " + ex.Message });
                }
            }
            else
            {
                // --- LOGIC CHO KHÁCH VÃNG LAI (Lưu Session) ---
                var sessionItems = HttpContext.Session.Get<List<CartItem>>(CART_KEY) ?? new List<CartItem>();
                var existingItem = sessionItems.FirstOrDefault(x => x.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    // Lưu ý: Chỉ cần lưu ProductId và Quantity vào session
                    sessionItems.Add(new CartItem { ProductId = productId, Quantity = quantity, Id = DateTime.Now.Ticks }); // Id tạm
                }

                // Lưu ngược lại vào Session
                HttpContext.Session.Set(CART_KEY, sessionItems);

                var totalCount = sessionItems.Sum(x => x.Quantity);
                return Json(new { success = true, message = "Đã thêm vào giỏ hàng (Tạm)!", cartCount = totalCount });
            }
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(long itemId, int quantity)
        {
            if (quantity < 1) quantity = 1;
            var userId = GetUserId();

            if (userId != -1)
            {
                // DB Logic
                var cartItem = await _context.CartItems.Include(x => x.Product).FirstOrDefaultAsync(x => x.Id == itemId);
                if (cartItem == null) return Json(new { success = false });

                cartItem.Quantity = quantity;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            else
            {
                // Session Logic
                var sessionItems = HttpContext.Session.Get<List<CartItem>>(CART_KEY);
                var item = sessionItems?.FirstOrDefault(x => x.Id == itemId); // itemId ở đây là Id tạm
                if (item != null)
                {
                    item.Quantity = quantity;
                    HttpContext.Session.Set(CART_KEY, sessionItems);
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            }
        }

        // POST: /Cart/RemoveItem
        [HttpPost]
        public async Task<IActionResult> RemoveItem(long itemId)
        {
            var userId = GetUserId();

            if (userId != -1)
            {
                // DB Logic
                var cartItem = await _context.CartItems.FindAsync(itemId);
                if (cartItem == null) return Json(new { success = false });
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            else
            {
                // Session Logic
                var sessionItems = HttpContext.Session.Get<List<CartItem>>(CART_KEY);
                var item = sessionItems?.FirstOrDefault(x => x.Id == itemId);
                if (item != null)
                {
                    sessionItems.Remove(item);
                    HttpContext.Session.Set(CART_KEY, sessionItems);
                    return Json(new { success = true });
                }
                return Json(new { success = false });
            }
        }

        // API Get Count
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = GetUserId();
            if (userId != -1)
            {
                var count = await _context.CartItems.Where(ci => ci.Cart.UserId == userId).SumAsync(ci => ci.Quantity);
                return Json(count);
            }
            else
            {
                var sessionItems = HttpContext.Session.Get<List<CartItem>>(CART_KEY);
                return Json(sessionItems?.Sum(x => x.Quantity) ?? 0);
            }
        }

        // POST: /Cart/Checkout
        [HttpPost]
        public IActionResult Checkout(List<long> selectedItems)
        {
            var userId = GetUserId();
            if (userId == -1)
            {
                // Nếu chưa đăng nhập mà bấm Checkout -> Bắt đăng nhập
                return RedirectToAction("Login", "User", new { returnUrl = "/Cart" });
            }

            if (selectedItems == null || !selectedItems.Any())
            {
                return RedirectToAction("Index");
            }

            string ids = string.Join(",", selectedItems);
            return RedirectToAction("Create", "Order", new { itemIds = ids });
        }

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