using Microsoft.AspNetCore.Mvc;
using TiramisuShop.Models;

namespace TiramisuShop.Controllers
{
    public class ContactController : Controller
    {
        private readonly TiramisuShopContext _context;

        public ContactController(TiramisuShopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.CreatedAt = DateTime.Now;
                contact.Status = 0;

                _context.Contacts.Add(contact);
                await _context.SaveChangesAsync();

                // TRẢ VỀ JSON CHO AJAX
                return Json(new { success = true, message = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất." });
            }

            // TRẢ VỀ LỖI NẾU CÓ
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = "Vui lòng kiểm tra lại thông tin.", errors = errors });
        }
    }
}