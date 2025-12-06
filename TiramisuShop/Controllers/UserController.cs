using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TiramisuShop.Helpers;
using TiramisuShop.Models; // Namespace chứa DbContext của bạn
using TiramisuShop.ViewModels;

namespace TiramisuShop.Controllers
{
    public class UserController : Controller
    {
        private readonly TiramisuShopContext _context;

        public UserController(TiramisuShopContext context)
        {
            _context = context;
        }

        // --- ĐĂNG KÝ ---
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                // 1. Kiểm tra Email đã tồn tại chưa
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(model);
                }

                // 2. Tạo User mới
                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    PasswordHash = SecurityHelper.HashPassword(model.Password), // Mã hóa password
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // --- ĐĂNG NHẬP ---
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                // 1. Tìm user theo Email
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                // 2. Kiểm tra password
                if (user != null && user.PasswordHash == SecurityHelper.HashPassword(model.Password))
                {
                    // 3. Tạo danh sách Claims (Thông tin lưu trong Cookie)
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim("UserId", user.Id.ToString()), // Lưu ID để dùng sau này
                        new Claim("FullName", $"{user.FirstName} {user.LastName}"),
                        new Claim(ClaimTypes.Role, "Customer") // Mặc định là khách hàng
                    };

                    // Nếu là email admin (ví dụ hardcode hoặc check trong DB)
                    if (user.Email == "4801103024@student.hcmue.edu.vn")
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "Admin"));
                    }

                    // 4. Tạo Identity và Principal
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // 5. Sign In (Tạo Cookie)
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

                    // MERGE CART LOGIC
                    var sessionCart = HttpContext.Session.Get<List<CartItem>>("GuestCart");
                    if (sessionCart != null && sessionCart.Any())
                    {
                        // 1. Lấy Cart DB của User
                        var dbCart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == user.Id);
                        if (dbCart == null)
                        {
                            dbCart = new Cart { UserId = user.Id };
                            _context.Carts.Add(dbCart);
                            await _context.SaveChangesAsync();
                        }

                        // 2. Chuyển items từ Session sang DB
                        foreach (var item in sessionCart)
                        {
                            var dbItem = await _context.CartItems.FirstOrDefaultAsync(x => x.CartId == dbCart.Id && x.ProductId == item.ProductId);
                            if (dbItem != null)
                            {
                                dbItem.Quantity += item.Quantity;
                            }
                            else
                            {
                                _context.CartItems.Add(new CartItem { CartId = dbCart.Id, ProductId = item.ProductId, Quantity = item.Quantity });
                            }
                        }
                        await _context.SaveChangesAsync();

                        // 3. Xóa Session
                        HttpContext.Session.Remove("GuestCart");
                    }

                    // 6. Redirect về trang cũ hoặc trang chủ
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("Password", "Email hoặc mật khẩu không chính xác.");
            }
            return View(model);
        }

        // --- ĐĂNG XUẤT ---
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }


        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = long.Parse(User.FindFirst("UserId").Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            var vm = new UserProfileVM
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                CreatedAt = user.CreatedAt
            };

            return View(vm);
        }

        // --- 2. CHỈNH SỬA HỒ SƠ ---
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userId = long.Parse(User.FindFirst("UserId").Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            var vm = new UserProfileVM
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email, // Email hiển thị nhưng readonly
                Phone = user.Phone
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EditProfile(UserProfileVM model)
        {
            var userId = long.Parse(User.FindFirst("UserId").Value);

            // Bảo mật: Đảm bảo user chỉ sửa được profile của chính mình
            if (userId != model.Id) return Forbid();

            if (ModelState.IsValid)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return NotFound();

                // Cập nhật thông tin
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Phone = model.Phone;
                // Không cập nhật Email để tránh lỗi xác thực

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("Profile");
            }

            return View(model);
        }
    }
}