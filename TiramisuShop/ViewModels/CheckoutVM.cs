using System.ComponentModel.DataAnnotations;
using TiramisuShop.Models;

namespace TiramisuShop.ViewModels
{
    public class CheckoutVM
    {
        // Thông tin người nhận
        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        public string ReceiverName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string ReceiverPhone { get; set; }

        // --- ĐỊA CHỈ (Sửa còn 2 cấp) ---
        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành")]
        public string ProvinceId { get; set; }
        public string ProvinceName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Xã/Phường")]
        public string WardId { get; set; }
        public string WardName { get; set; }

        // Đã xóa WardId và WardName

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể")]
        public string SpecificAddress { get; set; } // Số nhà, tên đường, phường xã (nhập tay)

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        public string? Note { get; set; }

        // Dữ liệu hiển thị
        public List<CartItem> CheckoutItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
    }
}