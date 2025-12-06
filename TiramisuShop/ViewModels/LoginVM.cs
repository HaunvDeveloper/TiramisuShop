using System.ComponentModel.DataAnnotations;

namespace TiramisuShop.ViewModels
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        public string Password { get; set; }

        public string? ReturnUrl { get; set; }
    }
}