# 🏥 MQSoft AI-CRM: Hệ Thống Quản Trị Khách Hàng Y Tế Thông Minh Tích Hợp AI

[![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/apps/aspnet)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4?logo=nuget&logoColor=white)](https://learn.microsoft.com/ef/core/)
[![Database](https://img.shields.io/badge/Database-SQL%20Server-CC292B?logo=microsoftsqlserver&logoColor=white)](https://www.microsoft.com/sql-server/)
[![AI Engine](https://img.shields.io/badge/AI-Google%20Gemini-4285F4?logo=google&logoColor=white)](https://ai.google.dev/)
[![Frontend](https://img.shields.io/badge/UI-Bootstrap%205-7952B3?logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-brightgreen)](#kiến-trúc-hệ-thống)

---

## 📌 1. Tổng Quan Dự Án

**MQSoft AI-CRM** là giải pháp phần mềm quản trị quan hệ khách hàng (CRM) chuyên biệt hóa cao cấp dành riêng cho các doanh nghiệp cung cấp dịch vụ, thiết bị và giải pháp công nghệ thông tin trong ngành y tế (HealthTech). Hệ thống quản lý toàn diện mạng lưới cơ sở y tế (Bệnh viện tuyến TW, Tỉnh, Huyện, Bệnh viện Tư nhân, Phòng khám đa khoa) cùng các gói giải pháp y tế tiêu chuẩn:
- 🏥 **HIS** (Hospital Information System) - Hệ thống thông tin bệnh viện
- 🔬 **LIS** (Laboratory Information System) - Hệ thống thông tin xét nghiệm
- 🖼️ **PACS** (Picture Archiving and Communication System) - Hệ thống lưu trữ và truyền hình ảnh y tế
- 📋 **EMR** (Electronic Medical Record) - Bệnh án điện tử
- 🌐 **Telemedicine** - Hệ thống hội chẩn và khám chữa bệnh từ xa

Điểm đột phá của **MQSoft AI-CRM** là việc tích hợp sâu **Trí tuệ nhân tạo (Google Gemini LLM)** kết hợp kiến trúc **RAG (Retrieval-Augmented Generation)**: AI có khả năng thấu cảm dữ liệu nội bộ, tự động trích xuất ngữ cảnh trực tiếp từ cơ sở dữ liệu (khách hàng, hợp đồng, tiến độ dự án, tài liệu kỹ thuật) để hỗ trợ tư vấn, tra cứu số liệu tức thì cho đội ngũ quản lý, kỹ sư và chuyên viên chăm sóc khách hàng.

---

## 🚀 2. Các Tính Năng Nổi Bật

### 🏢 2.1. Quản Lý Hồ Sơ Khách Hàng Y Tế Chuyên Sâu
- **Đặc thù ngành y tế**: Quản lý Mã cơ sở khám chữa bệnh (CSKCB), Tuyến bệnh viện (TW, Tỉnh, Huyện, Xã), Hạng bệnh viện (Đặc biệt, Hạng I, II, III, IV, Chưa phân hạng).
- **Quy mô hoạt động**: Theo dõi số giường bệnh kế hoạch, lượt khám ngoại trú/ngày.
- **Hiện trạng công nghệ**: Ghi nhận phần mềm cũ đang vận hành để dễ dàng xây dựng kế hoạch chuyển đổi và tích hợp hệ thống.
- **Đầu mối CNTT**: Lưu trữ chi tiết thông tin Trưởng phòng/Chuyên viên IT phụ trách tại bệnh viện (Họ tên, SĐT, Email).
- **Phân loại sở hữu**: Phân biệt Cơ sở Công lập và Cơ sở Tư nhân.
- **Tiện ích**: Hỗ trợ xuất dữ liệu danh sách khách hàng ra định dạng CSV/Excel.

### 📝 2.2. Quản Lý Hợp Đồng & Hồ Sơ Pháp Lý
- Theo dõi toàn bộ vòng đời hợp đồng: Số hợp đồng, Giá trị hợp đồng, Ngày ký kết, Ngày hết hạn.
- Phân loại trạng thái hợp đồng: *Mới ký*, *Đang hiệu lực*, *Đã thanh lý*, *Hết hạn*.
- Quản lý tệp đính kèm tài liệu hợp đồng số hóa (PDF, DOCX) với dung lượng hỗ trợ lên đến **50MB**.

### 📊 2.3. Quản Trị Dự Án & Theo Dõi Tiến Độ (Project Management)
- Quản lý các dự án triển khai phần mềm (HIS 2.0, PACS, LIS, EMR...).
- Phân bổ nhân sự vào dự án theo các vai trò rõ ràng (Project Manager, Developer, Business Analyst, Tester).
- Nhật ký tiến độ công việc: Cập nhật tỷ lệ % hoàn thành và ghi chú chi tiết theo từng cột mốc.

### 🤝 2.4. Nhật Ký Tương Tác & Lịch Sử Làm Việc (Interactions)
- Ghi nhận mọi hoạt động chăm sóc khách hàng: Gọi điện thoại, Họp trực tiếp, Demo sản phẩm, Đào tạo chuyển giao công nghệ, Hỗ trợ sự cố kỹ thuật.
- Theo dõi kết quả và phản hồi từ phía khách hàng bệnh viện.

### 📚 2.5. Kho Tài Liệu Nội Bộ & Knowledge Base
- Quản lý tài liệu theo danh mục (Hướng dẫn triển khai, Tài liệu kỹ thuật, Biểu mẫu hợp đồng).
- Gắn thẻ Tags tìm kiếm linh hoạt.
- Quản lý trạng thái số hóa dữ liệu cho AI (`IsProcessedForAI`).

### 🤖 2.6. Trợ Lý Ảo AI Thông Minh (Smart AI Chatbot & RAG Engine)
- **Công nghệ cốt lõi**: Tích hợp Google Gemini API với cơ chế **Fallback đa mô hình** (`gemini-2.5-flash` ➡️ `gemini-2.0-flash` ➡️ `gemini-flash-latest` ➡️ `gemini-flash-lite-latest`) đảm bảo bot luôn phản hồi thông suốt.
- **Dynamic RAG Pipeline**: Tự động nhận diện ý định câu hỏi để truy xuất dữ liệu phù hợp (Top khách hàng, Danh sách dự án đang chạy, Giá trị hợp đồng, Nhân sự phụ trách, Tài liệu hướng dẫn) làm ngữ cảnh (Context) trước khi gửi tới Gemini.
- **Context Memory**: Lưu giữ ngữ cảnh 5 lượt trao đổi gần nhất, giúp hội thoại liền mạch như trao đổi với trợ lý thực thụ.
- **Bảo vệ hệ thống**: Cơ chế Rate-limiting (30 giây/lượt) chống spam và tiết kiệm hạn ngạch API.
- **Cơ chế đánh giá**: Nhân viên có thể bấm Hài lòng / Không hài lòng kèm góp ý để liên tục cải tiến chất lượng tri thức AI.

### 📈 2.7. Báo Cáo & Phân Tích Thông Minh (Analytics Dashboard)
- Thống kê tổng hợp: Tổng số khách hàng, Hợp đồng đang hiệu lực, Dự án đang chạy, Số truy vấn AI.
- Biểu đồ doanh thu theo tháng, biểu đồ tỷ lệ phân bổ trạng thái dự án, phân bổ nhân sự phòng ban và Top hợp đồng giá trị lớn.

### 🛡️ 2.8. Nhật Ký Hệ Thống (Audit Logs) & An Ninh Phân Quyền
- **Tự động Audit Log**: Cơ chế tự động bắt vết `SaveChangesAsync` trong `ApplicationDbContext` để ghi nhận mọi thao tác `INSERT`, `UPDATE`, `DELETE` cùng ID người thực hiện và bảng dữ liệu bị tác động.
- **Giám sát hoạt động**: Filter tự động theo dõi hoạt động và trạng thái online của người dùng.
- **Hệ thống thông báo**: ViewComponent chuông thông báo (Notification Bell) hiển thị hoạt động mới nhất theo thời gian thực.
- **Xác thực bảo mật**: Đăng nhập bằng tài khoản cục bộ mã hóa mật khẩu chuẩn `BCrypt` kết hợp hỗ trợ Single Sign-On qua **Google OAuth 2.0**.
- **Phân quyền người dùng (RBAC)**: Hỗ trợ các vai trò `Admin`, `Manager`, `NhanVien`.

---

## 🏛️ 3. Kiến Trúc Hệ Thống (System Architecture)

Dự án được xây dựng theo mô hình **Clean Architecture / Onion Architecture** chuẩn doanh nghiệp:

```
MQ_AICRM/
│
├── 📂 src/
│   ├── 📂 Core/
│   │   ├── 📁 AI_CRM.Domain/             # Thực thể nghiệp vụ cốt lõi (Entities, Value Objects)
│   │   │   └── Entities/
│   │   │       └── AppEntities.cs        # KhachHang, HopDong, DuAn, NhanVien, NhatKyHeThong...
│   │   │
│   │   └── 📁 AI_CRM.Application/        # Giao diện nghiệp vụ trừu tượng, DTOs & Interfaces
│   │       └── Interfaces/               # ICustomerService, IChatbotService, IContractService...
│   │
│   ├── 📂 Infrastructure/
│   │   ├── 📁 AI_CRM.Infrastructure/     # Triển khai tầng hạ tầng, cơ sở dữ liệu & dịch vụ ngoại vi
│   │   │   ├── Data/                     # ApplicationDbContext, DbSeeder (Khởi tạo dữ liệu mẫu)
│   │   │   ├── Migrations/               # EF Core Migrations
│   │   │   └── Services/                 # ChatbotService (RAG + Gemini), AuthService, CustomerService...
│   │   │
│   │   └── 📁 AI_CRM.AI/                 # Module chuyên trách trí tuệ nhân tạo
│   │
│   └── 📂 Presentation/
│       └── 📁 AI_CRM.WebMvc/             # Ứng dụng Web MVC chính
│           ├── Controllers/              # Admin, Auth, Chatbot, Customer, Project, Contract...
│           ├── Views/                    # Giao diện Razor Views & Thành phần UI
│           ├── ViewComponents/           # NotificationViewComponent, v.v.
│           ├── Filters/                  # UserActivityFilter (Theo dõi truy cập)
│           ├── Services/                 # CurrentUserService, UserActivityService
│           ├── wwwroot/                  # Tệp tĩnh: CSS, JavaScript, Thư viện hình ảnh, Uploads
│           ├── Program.cs                # Dependency Injection & Middleware Pipeline
│           └── appsettings.json          # Cấu hình Connection String & API Keys
│
├── 📄 MQ_AICRM.sln                       # Visual Studio Solution
└── 📄 README.md                          # Tài liệu hướng dẫn dự án
```

---

## 💻 4. Công Nghệ Sử Dụng (Tech Stack)

| Hạng Mục | Công Nghệ / Thư Viện | Mô Tả |
| :--- | :--- | :--- |
| **Nền tảng chính** | `.NET 8.0` & `C# 12` | Framework hiện đại, hiệu năng cao từ Microsoft |
| **Kiến trúc Web** | `ASP.NET Core MVC` | Kiến trúc Model-View-Controller tách bạch giao diện & nghiệp vụ |
| **Hệ quản trị CSDL** | `Microsoft SQL Server` | CSDL quan hệ lưu trữ dữ liệu an toàn, tin cậy |
| **ORM** | `Entity Framework Core 8.0` | ORM mạnh mẽ, hỗ trợ Code-First, Migration và Audit Tracker |
| **Trí tuệ nhân tạo (AI)** | `Google Gemini API` | Tích hợp các model `gemini-2.5-flash`, `gemini-2.0-flash` với kiến trúc RAG |
| **Xác thực & Bảo mật** | `Cookie Auth` & `Google OAuth 2.0` | Đăng nhập đa kênh, mã hóa mật khẩu với `BCrypt.Net-Next` |
| **Giao diện Frontend** | `Bootstrap 5`, `HTML5`, `CSS3`, `JS` | Giao diện hiện đại, tối ưu Responsive trên mọi thiết bị |
| **Biểu đồ & Thống kê** | `Chart.js` | Trực quan hóa dữ liệu kinh doanh và tiến độ dự án |
| **Tải lên tệp** | `Kestrel 50MB Upload Limit` | Hỗ trợ lưu trữ tài liệu kỹ thuật, hợp đồng dung lượng lớn |

---

## ⚙️ 5. Yêu Cầu Môi Trường & Hướng Dẫn Cài Đặt

### 📋 5.1. Yêu cầu tiên quyết
- **.NET 8.0 SDK** ([Tải về tại đây](https://dotnet.microsoft.com/download/dotnet/8.0))
- **SQL Server 2019 / 2022** hoặc **SQL Server Express** / **LocalDB**
- **Visual Studio 2022** (bản 17.8 trở lên) hoặc **Visual Studio Code** (kèm C# Dev Kit)
- **Google Gemini API Key** ([Đăng ký miễn phí tại Google AI Studio](https://aistudio.google.com/))

---

### 📥 5.2. Các bước cài đặt chi tiết

#### Bước 1: Sao chép mã nguồn (Clone Repository)
```bash
git clone https://github.com/dieydev/mqsoft_crm.git
cd mqsoft_crm
```

#### Bước 2: Cấu hình `appsettings.json`
Mở file `src/Presentation/AI_CRM.WebMvc/appsettings.json` và cấu hình các thông số phù hợp với môi trường của bạn:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MQSoft_CRM_AI;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  },
  "GeminiApiKey": "YOUR_GEMINI_API_KEY_HERE"
}
```

> 💡 **Mẹo**:
> - Nếu bạn sử dụng SQL Server Express, hãy đổi Server thành: `Server=.\\SQLEXPRESS;...`
> - Nếu chưa có Google OAuth, bạn vẫn có thể đăng nhập bằng tài khoản cục bộ thông thường.
> - Thay thế `YOUR_GEMINI_API_KEY_HERE` bằng API Key thực tế của bạn để kích hoạt Trợ lý ảo AI.

#### Bước 3: Khởi tạo Cơ sở dữ liệu (Migration)
Mở Terminal hoặc Package Manager Console:

```bash
# Cách 1: Sử dụng .NET CLI
dotnet ef database update --project src/Infrastructure/AI_CRM.Infrastructure --startup-project src/Presentation/AI_CRM.WebMvc

# Cách 2: Sử dụng Package Manager Console trong Visual Studio
Update-Database -Project AI_CRM.Infrastructure -StartupProject AI_CRM.WebMvc
```

*(Lưu ý: Hệ thống đã tích hợp sẵn cơ chế `DbSeeder.SeedData` trong `Program.cs`. Trong lần đầu khởi chạy, ứng dụng sẽ tự động nạp các dữ liệu mẫu về Khách hàng bệnh viện, Dự án mẫu, Hợp đồng và Nhân sự).*

#### Bước 4: Khởi chạy Ứng dụng
```bash
dotnet run --project src/Presentation/AI_CRM.WebMvc
```
Truy cập trình duyệt theo địa chỉ: `https://localhost:7041` (hoặc cổng được hiển thị trong cửa sổ terminal).

---

## 👥 6. Dữ Liệu Mẫu & Phân Quyền Truy Cập

Hệ thống được thiết kế với cơ chế phân quyền dựa trên Vai trò (Role-Based Access Control):
- **Admin**: Toàn quyền cấu hình, quản trị tài khoản người dùng, xem toàn bộ log kiểm toán hệ thống.
- **Manager**: Quản lý khách hàng, duyệt hợp đồng, quản lý tiến độ dự án, xem báo cáo thống kê chuyên sâu.
- **NhanVien**: Thao tác chăm sóc khách hàng, cập nhật tiến độ công việc, tải lên tài liệu, tương tác với AI Chatbot.

---

## 🔒 7. Bảo Mật & Quy Định Bản Quyền

- Hệ thống không lưu trữ API Key hay thông tin nhạy cảm công khai trên repository.
- Các mật khẩu người dùng đều được băm bằng thuật toán `BCrypt` với Salt bảo mật cao.
- Mọi thao tác can thiệp dữ liệu đều được lưu vết chi tiết trong bảng `NhatKyHeThong` để phục vụ công tác thanh tra, bảo mật.

---

## 👨‍💻 Tác Giả & Liên Hệ

- **Dự án**: MQSoft AI-CRM (HealthTech Smart CRM)
- **Tác giả / Nhóm phát triển**: Đội ngũ MQSoft Team (`dieydev`)
- **Kho lưu trữ GitHub**: [https://github.com/dieydev/mqsoft_crm](https://github.com/dieydev/mqsoft_crm)

---
*Phát triển với sự tận tâm nhằm nâng cao chất lượng quản trị và chuyển đổi số cho ngành Y tế Việt Nam.*
