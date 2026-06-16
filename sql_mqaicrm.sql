CREATE DATABASE MQSoft_CRM_AI;
GO
USE MQSoft_CRM_AI;
GO
-- ==========================================
-- 1. VAITRO (Quản lý nhóm quyền)
-- ==========================================
CREATE TABLE VaiTro (
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE, -- Admin, NhanVien, Manager
    Description NVARCHAR(255)
);

-- ==========================================
-- 2. NGUOIDUNG (Tài khoản đăng nhập)
-- ==========================================
CREATE TABLE NguoiDung (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    RoleId INT NOT NULL,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_NguoiDung_VaiTro FOREIGN KEY(RoleId) REFERENCES VaiTro(RoleId)
);

-- ==========================================
-- 3. KHACHHANG (Quản lý khách hàng)
-- ==========================================
CREATE TABLE KhachHang (
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerCode NVARCHAR(20) UNIQUE,
    CompanyName NVARCHAR(255) NOT NULL,
    Representative NVARCHAR(100),
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Address NVARCHAR(255),
    Website NVARCHAR(255),
    Note NVARCHAR(MAX),
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsDeleted BIT DEFAULT 0
);

-- ==========================================
-- 4. TRANGTHAIHOPDONG
-- ==========================================
CREATE TABLE TrangThaiHopDong (
    StatusId INT IDENTITY(1,1) PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL, -- Mới tạo, Đang hiệu lực, Sắp hết hạn, Đã thanh lý
    Description NVARCHAR(255)
);

-- ==========================================
-- 5. HOPDONG
-- ==========================================
CREATE TABLE HopDong (
    ContractId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    StatusId INT NOT NULL,
    ContractNumber NVARCHAR(50) NOT NULL UNIQUE,
    ContractName NVARCHAR(255) NOT NULL,
    ContractValue DECIMAL(18,2),
    SignDate DATE,
    ExpireDate DATE,
    FilePath NVARCHAR(500),
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_HopDong_KhachHang FOREIGN KEY(CustomerId) REFERENCES KhachHang(CustomerId),
    CONSTRAINT FK_HopDong_TrangThai FOREIGN KEY(StatusId) REFERENCES TrangThaiHopDong(StatusId)
);

-- ==========================================
-- 6. TRANGTHAIDUAN
-- ==========================================
CREATE TABLE TrangThaiDuAn (
    StatusId INT IDENTITY(1,1) PRIMARY KEY,
    StatusName NVARCHAR(50) NOT NULL, -- Khởi tạo, Đang triển khai, Tạm dừng, Hoàn thành
    Description NVARCHAR(255)
);

-- ==========================================
-- 7. DUAN
-- ==========================================
CREATE TABLE DuAn (
    ProjectId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    ContractId INT NULL,
    StatusId INT NOT NULL,
    ProjectName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    StartDate DATE,
    EndDate DATE,
    Deadline DATE,
    Budget DECIMAL(18,2),
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_DuAn_KhachHang FOREIGN KEY(CustomerId) REFERENCES KhachHang(CustomerId),
    CONSTRAINT FK_DuAn_HopDong FOREIGN KEY(ContractId) REFERENCES HopDong(ContractId),
    CONSTRAINT FK_DuAn_TrangThai FOREIGN KEY(StatusId) REFERENCES TrangThaiDuAn(StatusId)
);

-- ==========================================
-- 8. NHANVIENPHUTRACH (Hồ sơ nhân sự)
-- ==========================================
CREATE TABLE NhanVienPhuTrach (
    EmployeeId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL, -- Liên kết với tài khoản đăng nhập (có thể NULL nếu nhân viên chưa có tài khoản)
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Department NVARCHAR(100),
    Position NVARCHAR(100),
    IsActive BIT DEFAULT 1,
    CONSTRAINT FK_NhanVien_NguoiDung FOREIGN KEY(UserId) REFERENCES NguoiDung(UserId)
);

-- ==========================================
-- 9. DUANNHANVIEN (Phân công nhân sự)
-- ==========================================
CREATE TABLE DuAnNhanVien (
    AssignmentId INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    EmployeeId INT NOT NULL,
    ProjectRole NVARCHAR(100), -- PM, Backend, Frontend, Tester, BA
    JoinDate DATE,
    CONSTRAINT FK_DuAnNV_DuAn FOREIGN KEY(ProjectId) REFERENCES DuAn(ProjectId),
    CONSTRAINT FK_DuAnNV_NhanVien FOREIGN KEY(EmployeeId) REFERENCES NhanVienPhuTrach(EmployeeId)
);

-- ==========================================
-- 10. TIENDODUAN (Cập nhật % hoàn thành)
-- ==========================================
CREATE TABLE TienDoDuAn (
    ProgressId INT IDENTITY(1,1) PRIMARY KEY,
    ProjectId INT NOT NULL,
    ProgressPercent INT CHECK(ProgressPercent BETWEEN 0 AND 100),
    Note NVARCHAR(MAX),
    UpdateDate DATETIME DEFAULT GETDATE(),
    UpdatedBy INT, -- ID của NhanVienPhuTrach thực hiện cập nhật
    CONSTRAINT FK_TienDo_DuAn FOREIGN KEY(ProjectId) REFERENCES DuAn(ProjectId),
    CONSTRAINT FK_TienDo_NhanVien FOREIGN KEY(UpdatedBy) REFERENCES NhanVienPhuTrach(EmployeeId)
);

-- ==========================================
-- 11. LICHSULAMVIEC (Chăm sóc khách hàng)
-- ==========================================
CREATE TABLE LichSuLamViec (
    WorkLogId INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    EmployeeId INT NOT NULL,
    InteractionType NVARCHAR(50), -- Call, Email, Meeting
    WorkDate DATETIME DEFAULT GETDATE(),
    Content NVARCHAR(MAX) NOT NULL,
    Result NVARCHAR(MAX),
    CONSTRAINT FK_LichSu_KhachHang FOREIGN KEY(CustomerId) REFERENCES KhachHang(CustomerId),
    CONSTRAINT FK_LichSu_NhanVien FOREIGN KEY(EmployeeId) REFERENCES NhanVienPhuTrach(EmployeeId)
);

-- ==========================================
-- 12. NHOMTAILIEU (Danh mục tài liệu)
-- ==========================================
CREATE TABLE NhomTaiLieu (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255)
);

-- ==========================================
-- 13. TAILIEUNOIBO (Lưu trữ và phục vụ AI RAG)
-- ==========================================
CREATE TABLE TaiLieuNoiBo (
    DocumentId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId INT NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    FilePath NVARCHAR(500),
    UploadedBy INT NOT NULL,
    UploadDate DATETIME DEFAULT GETDATE(),
    IsProcessedForAI BIT DEFAULT 0, -- Cờ đánh dấu đã Embedding cho VectorDB chưa
    IsDeleted BIT DEFAULT 0,
    CONSTRAINT FK_TaiLieu_Nhom FOREIGN KEY(CategoryId) REFERENCES NhomTaiLieu(CategoryId),
    CONSTRAINT FK_TaiLieu_NhanVien FOREIGN KEY(UploadedBy) REFERENCES NhanVienPhuTrach(EmployeeId)
);

-- ==========================================
-- 14. TAILIEUTAG (Gắn thẻ phân loại)
-- ==========================================
CREATE TABLE TaiLieuTag (
    TagId INT IDENTITY(1,1) PRIMARY KEY,
    DocumentId INT NOT NULL,
    TagName NVARCHAR(100) NOT NULL,
    CONSTRAINT FK_Tag_TaiLieu FOREIGN KEY(DocumentId) REFERENCES TaiLieuNoiBo(DocumentId)
);

-- ==========================================
-- 15. LICHSUHOIDAP (Chatbot AI)
-- ==========================================
CREATE TABLE LichSuHoiDap (
    ChatHistoryId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Question NVARCHAR(MAX) NOT NULL,
    Answer NVARCHAR(MAX) NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_ChatHistory_User FOREIGN KEY(UserId) REFERENCES NguoiDung(UserId)
);

-- ==========================================
-- 16. DANHGIACAUTRALOI (Feedback AI)
-- ==========================================
CREATE TABLE DanhGiaCauTraLoi (
    FeedbackId INT IDENTITY(1,1) PRIMARY KEY,
    ChatHistoryId INT NOT NULL,
    IsHelpful BIT NOT NULL, -- 1: Tốt, 0: Không tốt
    Comment NVARCHAR(MAX),
    CreatedDate DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Feedback_ChatHistory FOREIGN KEY(ChatHistoryId) REFERENCES LichSuHoiDap(ChatHistoryId)
);

-- ==========================================
-- 17. NHATKYHETHONG (Audit Log)
-- ==========================================
CREATE TABLE NhatKyHeThong (
    LogId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NULL,
    Action NVARCHAR(50), -- INSERT, UPDATE, DELETE, LOGIN
    TableName NVARCHAR(100),
    RecordId INT,
    Details NVARCHAR(MAX),
    Timestamp DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_NhatKy_User FOREIGN KEY(UserId) REFERENCES NguoiDung(UserId)
);

-- ==========================================
-- 1. VAITRO
-- ==========================================
INSERT INTO VaiTro (RoleName, Description) VALUES 
('Admin', N'Quản trị viên hệ thống'),
('Implementation Manager', N'Quản lý dự án triển khai HIS/LIS/PACS'),
('Technical Support', N'Nhân viên hỗ trợ kỹ thuật và bảo trì Bệnh viện');

-- ==========================================
-- 2. NGUOIDUNG (Cập nhật 4 tài khoản: Duy, Phú, Trí, Anh)
-- ==========================================
INSERT INTO NguoiDung (RoleId, Username, PasswordHash, IsActive) VALUES 
(1, 'admin_duy', 'hashed_duy_123', 1),
(2, 'pm_phu', 'hashed_phu_456', 1),
(3, 'support_tri', 'hashed_tri_789', 1),
(3, 'dev_anh', 'hashed_anh_000', 1);

-- ==========================================
-- 3. KHACHHANG (BVĐK KV Thủ Đức)
-- ==========================================
INSERT INTO KhachHang (CustomerCode, CompanyName, Representative, Email, Phone, Address, Website, Note) VALUES 
('BV-TĐ', N'Bệnh viện Đa khoa Khu vực Thủ Đức', N'BS.CKII Nguyễn Văn A (Giám đốc)', 'bvdktuduc@tphcm.gov.vn', '02838966598', N'Số 64 Lê Văn Chí, P. Linh Trung, TP. Thủ Đức', 'benhvienthuduc.vn', N'Bệnh viện tuyến Thành phố. Đang nâng cấp HIS QĐ 130 và Kiosk.'),
('PK-TĐ', N'Phòng khám Đa khoa vệ tinh Thủ Đức', N'Trần Trọng B (Trưởng phòng KH-TH)', 'phongkham_td@gmail.com', '0912345678', N'Khu công nghệ cao, TP. Thủ Đức', NULL, N'Đơn vị trực thuộc, cần đồng bộ dữ liệu về viện chính');

-- ==========================================
-- 4. TRANGTHAIHOPDONG
-- ==========================================
INSERT INTO TrangThaiHopDong (StatusName, Description) VALUES 
(N'Khảo sát & Báo giá', N'Đang trong giai đoạn đấu thầu hoặc chào giá cạnh tranh'),
(N'Đang hiệu lực', N'Đã ký kết, đang triển khai/bảo hành'),
(N'Bảo trì SLA', N'Đã nghiệm thu, chuyển sang giai đoạn hỗ trợ hàng năm');

-- ==========================================
-- 5. HOPDONG
-- ==========================================
INSERT INTO HopDong (CustomerId, StatusId, ContractNumber, ContractName, ContractValue, SignDate, ExpireDate, FilePath) VALUES 
(1, 2, 'HD-HIS-2024-TD01', N'Hợp đồng Nâng cấp HIS đáp ứng QĐ 130', 1200000000, '2024-03-01', '2025-03-01', '/contracts/HD_BVKVThuDuc_QĐ130.pdf'),
(1, 2, 'HD-PACS-2024-TD02', N'Hợp đồng Triển khai Kiosk đăng ký khám', 850000000, '2024-04-15', '2024-10-15', '/contracts/HD_BVKVThuDuc_Kiosk.pdf');

-- ==========================================
-- 6. TRANGTHAIDUAN
-- ==========================================
INSERT INTO TrangThaiDuAn (StatusName, Description) VALUES 
(N'Cài đặt Server', N'Chuẩn bị hạ tầng, Database tại Data Center của BV'),
(N'Đào tạo End-User', N'Hướng dẫn sử dụng cho Y Bác sĩ, Điều dưỡng'),
(N'Trực viện (Go-Live)', N'Hỗ trợ trực tiếp tại các khoa phòng'),
(N'Nghiệm thu', N'Ký biên bản hoàn thành dự án');

-- ==========================================
-- 7. DUAN
-- ==========================================
INSERT INTO DuAn (CustomerId, ContractId, StatusId, ProjectName, Description, StartDate, EndDate, Deadline, Budget) VALUES 
(1, 1, 3, N'Nâng cấp HIS QĐ 130 - BVĐK Thủ Đức', N'Cập nhật luồng ngoại trú, nội trú xuất file XML chuẩn 130.', '2024-03-10', NULL, '2024-06-30', 800000000),
(1, 2, 1, N'Triển khai Kiosk - BVĐK Thủ Đức', N'Lắp đặt 5 Kiosk tại sảnh khám bệnh.', '2024-05-01', NULL, '2024-08-15', 500000000);

-- ==========================================
-- 8. NHANVIENPHUTRACH (Cập nhật Tên Nhân Sự)
-- ==========================================
INSERT INTO NhanVienPhuTrach (UserId, FullName, Email, Phone, Department, Position) VALUES 
(1, N'Nguyễn Thanh Duy', 'duy.nt@mqsoft.com', '0909999999', N'Ban Giám Đốc', N'Giám đốc Dự án'),
(2, N'Lê Hoàng Phú', 'phu.lh@mqsoft.com', '0908888888', N'Phòng Triển Khai', N'Trưởng nhóm Triển khai HIS'),
(3, N'Trần Minh Trí', 'tri.tm@mqsoft.com', '0907777777', N'Phòng CSKH', N'Chuyên viên Trực viện'),
(4, N'Phạm Tuấn Anh', 'anh.pt@mqsoft.com', '0906666666', N'Phòng R&D', N'Chuyên viên Tích hợp API');

-- ==========================================
-- 9. DUANNHANVIEN
-- ==========================================
INSERT INTO DuAnNhanVien (ProjectId, EmployeeId, ProjectRole, JoinDate) VALUES 
(1, 1, 'Project Director', '2024-03-10'),     -- Duy quản lý chung
(1, 2, 'Team Leader', '2024-03-10'),          -- Phú làm Team Lead dự án 130
(1, 3, 'On-site Support', '2024-05-01'),      -- Trí trực viện hỗ trợ bác sĩ
(2, 2, 'Project Manager', '2024-05-01'),      -- Phú quản lý dự án Kiosk
(2, 4, 'Integration Dev', '2024-05-05');      -- Anh lo code tích hợp Kiosk

-- ==========================================
-- 10. TIENDODUAN
-- ==========================================
INSERT INTO TienDoDuAn (ProjectId, ProgressPercent, Note, UpdatedBy) VALUES 
(1, 30, N'Hoàn thành update Database cấu trúc mới phục vụ QĐ 130', 2), -- Phú update
(1, 75, N'Đã test gửi file XML thành công lên cổng test BHYT', 4),    -- Anh update
(2, 20, N'Hoàn tất cấu hình mạng LAN cho 5 Kiosk', 3);                -- Trí update

-- ==========================================
-- 11. LICHSULAMVIEC
-- ==========================================
INSERT INTO LichSuLamViec (CustomerId, EmployeeId, InteractionType, WorkDate, Content, Result) VALUES 
(1, 1, 'Meeting', '2024-02-20', N'Họp với BGĐ BV Thủ Đức chốt phương án', N'Thống nhất tiến độ triển khai QĐ 130'), -- Duy đi họp
(1, 3, 'On-site', '2024-05-15', N'Trực hỗ trợ khoa Dược nhập kho', N'Giải quyết tình trạng kẹt bill'),            -- Trí hỗ trợ khoa Dược
(1, 4, 'Remote', '2024-06-01', N'Check log lỗi API đẩy cổng giám định', N'Đã fix xong lỗi sai mã thẻ BHYT');      -- Anh fix bug từ xa

-- ==========================================
-- 12. NHOMTAILIEU
-- ==========================================
INSERT INTO NhomTaiLieu (CategoryName, Description) VALUES 
(N'Đặc tả quy trình Bệnh viện', N'Tài liệu quy trình KCB tại BVĐK Thủ Đức'),
(N'Tài liệu Tích hợp Thiết bị', N'Protocol kết nối Kiosk, API PACS'),
(N'Hướng dẫn sử dụng HIS', N'Manual các phân hệ dành cho End-User');

-- ==========================================
-- 13. TAILIEUNOIBO (Phục vụ AI Chatbot)
-- ==========================================
INSERT INTO TaiLieuNoiBo (CategoryId, Title, Description, FilePath, UploadedBy, IsProcessedForAI) VALUES 
(1, N'Quy trình Khám BHYT - BV Thủ Đức', N'Luồng đi bệnh nhân từ Kiosk đến Dược', '/docs/QuyTrinh_KCB_ThuDuc.pdf', 1, 1),
(2, N'API Integration Kiosk', N'Tài liệu gọi API lấy số thứ tự', '/docs/API_Kiosk.pdf', 4, 1),
(3, N'HDSD Phân hệ Viện phí - QĐ 130', N'Hướng dẫn xuất XML', '/docs/HDSD_VienPhi_130.docx', 2, 0);

-- ==========================================
-- 14. TAILIEUTAG
-- ==========================================
INSERT INTO TaiLieuTag (DocumentId, TagName) VALUES 
(1, 'BVThuDuc'), (1, 'QuyTrinh'), (1, 'BHYT'),
(2, 'Kiosk'), (2, 'API'), (2, 'TichHop'),
(3, 'VienPhi'), (3, 'Cong130');

-- ==========================================
-- 15. LICHSUHOIDAP (AI Chatbot)
-- ==========================================
INSERT INTO LichSuHoiDap (UserId, Question, Answer) VALUES 
(3, N'Bệnh nhân khám trái tuyến tại BV Thủ Đức tỷ lệ hưởng nội trú là bao nhiêu?', N'Theo quy định hiện hành, bệnh nhân khám trái tuyến tại BVĐK KV Thủ Đức (tuyến TP) được thanh toán 100% chi phí điều trị nội trú theo mức hưởng của thẻ BHYT.'),
(4, N'Endpoint API lấy số thứ tự Kiosk là gì?', N'Endpoint để cấp số thứ tự là POST /api/v1/kiosk/ticket. Chi tiết tham khảo tài liệu API Integration Kiosk do Tuấn Anh upload.');

-- ==========================================
-- 16. DANHGIACAUTRALOI
-- ==========================================
INSERT INTO DanhGiaCauTraLoi (ChatHistoryId, IsHelpful, Comment) VALUES 
(1, 1, N'AI trả lời đúng luật BHYT cho Trí tra cứu khi trực viện'),
(2, 1, N'Chuẩn endpoint');

-- ==========================================
-- 17. NHATKYHETHONG
-- ==========================================
INSERT INTO NhatKyHeThong (UserId, Action, TableName, RecordId, Details) VALUES 
(2, 'UPDATE', 'TienDoDuAn', 2, '{"OldPercent": 30, "NewPercent": 75, "Note": "Test XML thành công"}'),
(1, 'LOGIN', 'NguoiDung', 1, '{"IP": "10.0.0.1", "Location": "Office"}');

-- 1. XÓA DỮ LIỆU TỪ BẢNG CON LÊN BẢNG CHA
DELETE FROM NhatKyHeThong;
DELETE FROM DanhGiaCauTraLoi;
DELETE FROM LichSuHoiDap;
DELETE FROM TaiLieuTag;
DELETE FROM TaiLieuNoiBo;
DELETE FROM NhomTaiLieu;
DELETE FROM LichSuLamViec;
DELETE FROM TienDoDuAn;
DELETE FROM DuAnNhanVien;
DELETE FROM NhanVienPhuTrach;
DELETE FROM DuAn;
DELETE FROM TrangThaiDuAn;
DELETE FROM HopDong;
DELETE FROM TrangThaiHopDong;
DELETE FROM KhachHang;
DELETE FROM NguoiDung;
DELETE FROM VaiTro;
GO

-- 2. RESET LẠI IDENTITY (ID TỰ TĂNG) VỀ 0 ĐỂ BẮT ĐẦU LẠI TỪ 1
DBCC CHECKIDENT ('NhatKyHeThong', RESEED, 0);
DBCC CHECKIDENT ('NhatKyHeThong', RESEED, 0);
DBCC CHECKIDENT ('DanhGiaCauTraLoi', RESEED, 0);
DBCC CHECKIDENT ('LichSuHoiDap', RESEED, 0);
DBCC CHECKIDENT ('TaiLieuTag', RESEED, 0);
DBCC CHECKIDENT ('TaiLieuNoiBo', RESEED, 0);
DBCC CHECKIDENT ('NhomTaiLieu', RESEED, 0);
DBCC CHECKIDENT ('LichSuLamViec', RESEED, 0);
DBCC CHECKIDENT ('TienDoDuAn', RESEED, 0);
DBCC CHECKIDENT ('DuAnNhanVien', RESEED, 0);
DBCC CHECKIDENT ('NhanVienPhuTrach', RESEED, 0);
DBCC CHECKIDENT ('DuAn', RESEED, 0);
DBCC CHECKIDENT ('TrangThaiDuAn', RESEED, 0);
DBCC CHECKIDENT ('HopDong', RESEED, 0);
DBCC CHECKIDENT ('TrangThaiHopDong', RESEED, 0);
DBCC CHECKIDENT ('KhachHang', RESEED, 0);
DBCC CHECKIDENT ('NguoiDung', RESEED, 0);
DBCC CHECKIDENT ('VaiTro', RESEED, 0);
GO