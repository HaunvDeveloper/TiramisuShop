# TiramisuShop - Cửa Hàng Bánh Ngọt Trực Tuyến

**TiramisuShop** là một ứng dụng web thương mại điện tử chuyên cung cấp các loại bánh Tiramisu, được xây dựng trên nền tảng **ASP.NET Core MVC**. Dự án bao gồm giao diện người dùng (Storefront) để khách hàng mua sắm và trang quản trị (Admin Dashboard) để quản lý cửa hàng.

## 🚀 Tính Năng Chính

### 🛒 Dành Cho Khách Hàng (User)

  * **Trang chủ:** Banner giới thiệu, sản phẩm nổi bật, và các hiệu ứng giao diện theo mùa (Xuân, Hạ, Thu, Đông).
  * **Menu Sản Phẩm:** Xem danh sách bánh, lọc theo danh mục (Truyền thống, Trái cây, Matcha...), sắp xếp theo giá/đánh giá.
  * **Chi Tiết Sản Phẩm:** Xem thông tin chi tiết, hình ảnh, đánh giá từ người dùng khác, và các sản phẩm liên quan.
  * **Giỏ Hàng:** Thêm/sửa/xóa sản phẩm, tự động lưu giỏ hàng tạm thời cho khách vãng lai (Session) và đồng bộ khi đăng nhập.
  * **Đặt Hàng (Checkout):** Quy trình thanh toán đơn giản, tích hợp API chọn địa chỉ hành chính Việt Nam (Tỉnh/Thành - Quận/Huyện - Phường/Xã).
  * **Tài Khoản:** Đăng ký, Đăng nhập, Quản lý hồ sơ cá nhân, Xem lịch sử đơn hàng.
  * **Tương Tác:** Gửi liên hệ/phản hồi, Đánh giá sản phẩm (số sao & bình luận).
  * **Sự Kiện:** Xem các chương trình khuyến mãi và ưu đãi hiện hành.

### 🛠️ Dành Cho Quản Trị Viên (Admin)

  * **Dashboard:** Thống kê tổng quan về doanh thu, số lượng đơn hàng, sản phẩm, khách hàng. Biểu đồ doanh thu theo tuần.
  * **Quản Lý Sản Phẩm:** Thêm, xóa, sửa thông tin bánh, quản lý hình ảnh, tồn kho, giá bán.
  * **Quản Lý Cấu Hình:** Tùy chỉnh hiệu ứng giao diện trang web theo mùa.
  * **Quản Lý Đơn Hàng:** (Các chức năng quản lý khác được tích hợp trong khu vực Admin).

## 💻 Công Nghệ Sử Dụng

  * **Backend:** ASP.NET Core 8.0 (MVC Pattern).
  * **Database:** SQL Server (Entity Framework Core - Code First/Db First hybrid).
  * **Frontend User:** HTML5, CSS3, JavaScript, jQuery, Bootstrap.
      * *Plugins:* Swiper JS (Slider), SweetAlert2 (Thông báo), Select2 (Dropdown).
  * **Frontend Admin:** AdminLTE Template.
  * **Utilities:**
      * Mã hóa mật khẩu SHA256.
      * Quản lý Session & Cookie Authentication.

## 🗄️ Cấu Trúc Cơ Sở Dữ Liệu

Dự án sử dụng SQL Server với các bảng chính:

  * `Users`: Lưu thông tin người dùng và quản trị viên.
  * `Products`: Thông tin sản phẩm.
  * `Categories`: Danh mục sản phẩm.
  * `ProductImages`: Thư viện ảnh sản phẩm.
  * `Orders` & `OrderItems`: Quản lý đơn hàng chi tiết.
  * `Carts` & `CartItems`: Giỏ hàng người dùng.
  * `Reviews`: Đánh giá sản phẩm.
  * `Events`: Các sự kiện khuyến mãi.
  * `Contacts`: Lưu phản hồi từ khách hàng.
  * `SystemSettings`: Cấu hình hệ thống (ví dụ: hiệu ứng mùa).

## ⚙️ Hướng Dẫn Cài Đặt

### Yêu cầu

  * .NET SDK 8.0 trở lên.
  * SQL Server (LocalDB hoặc bản đầy đủ).
  * Visual Studio 2022 hoặc VS Code.

### Các bước cài đặt

1.  **Clone dự án:**

    ```bash
    git clone https://github.com/haunvdeveloper/tiramisushop.git
    ```

2.  **Cấu hình Cơ sở dữ liệu:**

      * Mở SQL Server Management Studio (SSMS).
      * Chạy file script `script.sql` (nằm trong thư mục gốc) để tạo Database `TiramisuShop` và dữ liệu mẫu.
      * **Lưu ý:** Script này đã bao gồm việc tạo bảng và insert dữ liệu mẫu cho Categories, Products, Users (Admin), v.v.

3.  **Cấu hình kết nối:**

      * Mở file `appsettings.json` trong dự án `TiramisuShop`.
      * Cập nhật chuỗi kết nối `DefaultConnection` phù hợp với SQL Server của bạn:

    <!-- end list -->

    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=TiramisuShop;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
    }
    ```

4.  **Chạy ứng dụng:**

      * Mở dự án trong Visual Studio.
      * Nhấn `F5` hoặc chạy lệnh `dotnet run`.

### 🔑 Tài Khoản Admin Mặc Định

Dữ liệu mẫu trong `script.sql` đã tạo sẵn một tài khoản Admin. Mật khẩu được mã hóa SHA256. Nếu bạn cần đăng nhập, hãy kiểm tra bảng `Users` hoặc tạo mới một tài khoản và set quyền trong code/database nếu cần thiết.

  * **Email:** `haunv.cntt@gmail.com`

## 📂 Cấu Trúc Thư Mục Chính

  * `Controllers/`: Chứa logic xử lý cho phần User (Home, Product, Cart, Order...).
  * `Areas/Admin/`: Chứa logic và giao diện cho phần quản trị.
  * `Models/`: Các lớp thực thể (Entity) ánh xạ với Database.
  * `ViewModels/`: Các lớp dữ liệu trung gian dùng cho View (LoginVM, CheckoutVM, DashboardVM...).
  * `Views/`: Giao diện người dùng (Razor Pages).
  * `wwwroot/`: Chứa file tĩnh (CSS, JS, Images, Libs).
  * `Helpers/`: Các tiện ích hỗ trợ (Mã hóa, Session extension).

## 🎨 Giao Diện

Dự án được thiết kế với tông màu chủ đạo **Nâu (Cà phê/Socola)** và **Kem (Sữa/Phô mai)**, mang lại cảm giác ấm cúng và ngọt ngào đặc trưng của tiệm bánh.

-----

*© 2025 Cheese Cheese - Developed by HMCoding.*