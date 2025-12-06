using System.ComponentModel.DataAnnotations;

namespace TiramisuShop.ViewModels
{
    public class UserProfileVM
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ")]
        [Display(Name = "Họ")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập tên")]
        [Display(Name = "Tên")]
        public string LastName { get; set; } = null!;

        [Display(Name = "Email")]
        public string Email { get; set; } = null!; // Thường không cho sửa Email

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        // Helper để lấy tên đầy đủ
        public string FullName => $"{FirstName} {LastName}";

        // Helper lấy chữ cái đầu làm Avatar
        public string AvatarLetter => !string.IsNullOrEmpty(FirstName) ? FirstName.Substring(0, 1).ToUpper() : "U";
    }
}