using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiramisuShop.Models;
using System.Security.Claims;

namespace TiramisuShop.Controllers
{
    public class ReviewController : Controller
    {
        private readonly TiramisuShopContext _context;

        public ReviewController(TiramisuShopContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(Review review)
        {
            // 1. Kiểm tra đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để đánh giá." });
            }

            // 2. Lấy UserId từ Claims
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null)
            {
                review.UserId = long.Parse(userIdClaim.Value);
            }

            if (review.Rating > 0 && review.ProductId > 0)
            {
                try
                {
                    review.CreatedAt = DateTime.Now;
                    review.IsVisible = true; // Hoặc false nếu cần duyệt

                    _context.Reviews.Add(review);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Cảm ơn bạn đã đánh giá sản phẩm!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
                }
            }

            return Json(new { success = false, message = "Vui lòng nhập đầy đủ thông tin (Số sao & Nội dung)." });
        }
    }
}