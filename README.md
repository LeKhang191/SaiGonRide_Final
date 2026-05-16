# SaiGonRide_SE_FinalProject
🚲 SaigonRide - Distributed Vehicle Rental System
Course: Software Engineering (HK2 - 2025-2026)
Team: F2 | Tier: 4 (Advanced)

 Hướng dẫn cài đặt và chạy (Deployment Guide)
 1. Yêu cầu hệ thống (Prerequisites)
 IDE: Visual Studio 2022 (v17.4 trở lên).
 Framework: .NET 8.0.
 Database: SQL Server (LocalDB).
 Browser: Chrome/Edge (để chạy Automation Test).
 
 2. Thiết lập Cơ sở dữ liệu (Database Setup)
 Nhóm đã chuẩn bị sẵn Script trong thư mục /Database. Vui lòng thực hiện các bước sau:
 Mở SQL Server Object Explorer trong Visual Studio hoặc SSMS.
 Chạy file SaigonRide_Database.sql để khởi tạo cấu trúc bảng.
 Chạy file SeedData.sql để nạp dữ liệu mẫu (Trạm xe, Xe, Tài khoản).
 Kiểm tra chuỗi kết nối trong appsettings.json để đảm bảo khớp với Server của bạn.
 
 3. Cách khởi chạy Project
 Mở file SaigonRide.sln.
 Nhấn F5 để chạy ứng dụng dưới quyền IIS Express hoặc SaigonRide.
 Hệ thống sẽ tự động mở trên trình duyệt tại địa chỉ localhost.
 
  Kiểm thử tự động (Quality Assurance - Tier 4 Only)
  Dự án bao gồm bộ Automation Testing sử dụng Selenium WebDriver để đảm bảo chất lượng hệ thống.
  Mở cửa sổ Test Explorer trong Visual Studio.Chuột phải vào Project SaigonRide.Tests.
  Chọn Run.
  Kết quả: 8/8 Test Cases (bao gồm luồng Login, Start Rental, và Payment).
  
  Tài khoản thử nghiệm (Test Credentials)
  Để thuận tiện cho việc chấm bài, thầy có thể sử dụng các tài khoản sau:
  
  Vai tròEmailMật khẩuAdminadmin@saigonride.com123456Userkhang@student.tdtu.edu.vn123456
  
   Công nghệ sử dụng
   Backend: ASP.NET MVC, Entity Framework Core (Code First).
   Frontend: Bootstrap 5, Razor View Engine.
   Payment: VNPAY API & PayPal API.
   Testing: Selenium WebDriver, xUnit.
   Cloud: Render (PostgreSQL).
   
   Thành viên thực hiện (Team F2)
   Lê Duy Khang (Leader) - 524H0097
   Nguyễn Minh Hoàng - 524H0206
   Lý Phúc Hải - 524H0013