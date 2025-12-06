using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TiramisuShop.Models; // Namespace chứa DbContext của bạn
using TiramisuShop.ViewModels;
using TiramisuShop.Helpers;

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
    }
}