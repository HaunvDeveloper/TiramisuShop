using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiramisuShop.Models;

namespace TiramisuShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly TiramisuShopContext _context;
        public ProductController(TiramisuShopContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.Category)
                .Include(p => p.Event) // <--- QUAN TRỌNG: Lấy thông tin Event
                .ToList();

            var categories = _context.Categories.ToList();

            ViewBag.Products = products;
            ViewBag.Categories = categories;

            return View();
        }

        // GET: Product/Details/5
        // GET: Product/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Event)
                // Include reviews and the user who wrote each review
                .Include(p => p.Reviews.Where(r => r.IsVisible))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Get 4 related products (Same category, different ID)
            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Include(p => p.ProductImages)
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            // Calculate average rating if there are reviews
            if (product.Reviews.Any())
            {
                ViewBag.AverageRating = product.Reviews.Average(r => r.Rating);
                ViewBag.ReviewCount = product.Reviews.Count;
            }
            else
            {
                ViewBag.AverageRating = 0;
                ViewBag.ReviewCount = 0;
            }

            return View(product);
        }


    }
}
