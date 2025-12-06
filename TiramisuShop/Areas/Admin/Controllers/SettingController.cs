using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TiramisuShop.Models;

namespace TiramisuShop.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SettingController : Controller
    {
        private readonly TiramisuShopContext _context;

        public SettingController(TiramisuShopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var seasonSetting = _context.SystemSettings.Find("CurrentSeason");
            ViewBag.CurrentSeason = seasonSetting?.Value ?? "None";
            return View();
        }

        [HttpPost]
        public IActionResult UpdateSeason(string season)
        {
            var setting = _context.SystemSettings.Find("CurrentSeason");
            if (setting == null)
            {
                setting = new SystemSetting { Key = "CurrentSeason", Value = season };
                _context.SystemSettings.Add(setting);
            }
            else
            {
                setting.Value = season;
                _context.SystemSettings.Update(setting);
            }
            _context.SaveChanges();

            TempData["Success"] = "Đã cập nhật hiệu ứng mùa thành công!";
            return RedirectToAction("Index");
        }
    }
}
