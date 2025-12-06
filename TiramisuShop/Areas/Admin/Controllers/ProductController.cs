using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiramisuShop.Models;

namespace TiramisuShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly TiramisuShopContext _context;

        public ProductController(TiramisuShopContext context)
        {
            _context = context;
        }

        // GET: /AdminProduct
        public async Task<IActionResult> Index(string searchString)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .AsQueryable();

            // Chức năng tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            // Sắp xếp mới nhất lên đầu
            return View(await products.OrderByDescending(p => p.Id).ToListAsync());
        }


        // --- 1. CREATE (THÊM MỚI) ---
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Events = _context.Events.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            ModelState.Remove("Category");
            ModelState.Remove("Event");
            ModelState.Remove("CartItems");
            ModelState.Remove("OrderItems");
            ModelState.Remove("ProductImages");
            ModelState.Remove("Reviews");
            if (ModelState.IsValid)
            {
                // 1. Lưu sản phẩm trước để lấy ID
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // 2. Xử lý Upload ảnh (nếu có)
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Tạo tên file duy nhất
                    var fileName = DateTime.Now.Ticks + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Lưu vào bảng ProductImages
                    var pImage = new ProductImage
                    {
                        ProductId = product.Id,
                        ImageUrl = "/assets/images/" + fileName
                    };
                    _context.ProductImages.Add(pImage);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi thì load lại danh sách
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Events = _context.Events.ToList();
            return View(product);
        }

        // --- 2. EDIT (CHỈNH SỬA) ---
        [HttpGet]
        public async Task<IActionResult> Edit(long id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Events = _context.Events.ToList();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(long id, Product product, IFormFile? imageFile)
        {
            if (id != product.Id) return NotFound();
            ModelState.Remove("Category");
            ModelState.Remove("Event");
            ModelState.Remove("CartItems");
            ModelState.Remove("OrderItems");
            ModelState.Remove("ProductImages");
            ModelState.Remove("Reviews");
            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật thông tin cơ bản
                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    // Xử lý ảnh mới (Nếu có upload ảnh mới thì thêm vào)
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var fileName = DateTime.Now.Ticks + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        // Xóa ảnh cũ (Optional - Nếu muốn mỗi sp chỉ 1 ảnh)
                        var oldImages = _context.ProductImages.Where(p => p.ProductId == id);
                        _context.ProductImages.RemoveRange(oldImages);

                        // Thêm ảnh mới
                        var pImage = new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = "/assets/images/" + fileName
                        };
                        _context.ProductImages.Add(pImage);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Events = _context.Events.ToList();
            return View(product);
        }

        // Action Xóa (Dùng cho AJAX)
        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return Json(new { success = false, message = "Không tìm thấy sản phẩm." });

            try
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
