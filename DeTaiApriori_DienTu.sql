USE MASTER;
GO

-- Xóa database nếu đã tồn tại
IF DB_ID('CuaHangMayTinh2') IS NOT NULL
BEGIN
    ALTER DATABASE CuaHangMayTinh2 SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE CuaHangMayTinh2;
END
GO

-- Tạo database mới
CREATE DATABASE CuaHangMayTinh2;
GO

USE CuaHangMayTinh2;
GO

-- Tạo bảng DanhMuc
CREATE TABLE DanhMuc (
    DanhMucID INT PRIMARY KEY,
    TenDanhMuc NVARCHAR(100) NOT NULL
);
GO

-- Tạo bảng SanPham
CREATE TABLE SanPham (
    SanPhamID INT PRIMARY KEY,
    TenSP NVARCHAR(255) NOT NULL,
    MoTa NVARCHAR(MAX),
    Gia DECIMAL(18, 2) NOT NULL,
    DanhMucID INT,
    SoLuongKho INT NOT NULL,
    ThuongHieu NVARCHAR(100),
    ViXuLy NVARCHAR(100),
    RAM NVARCHAR(50),
    LuuTru NVARCHAR(50),
    KichThuocManHinh NVARCHAR(50),
    MauSac NVARCHAR(50),
    DungLuongPin NVARCHAR(50),
    DanhGia FLOAT DEFAULT 0,
    LuotXem INT DEFAULT 0,
    TrangThai NVARCHAR(50) DEFAULT N'ConHang', -- Các giá trị: ConHang, HetHang, SapCoHang
    NgayCapNhat DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_SanPham_DanhMuc FOREIGN KEY (DanhMucID) REFERENCES DanhMuc(DanhMucID)
);
GO

-- Tạo bảng HinhAnhSanPham
CREATE TABLE HinhAnhSanPham (
    HinhAnhID INT IDENTITY(1,1) PRIMARY KEY,
    SanPhamID INT,
    DuongDanAnh NVARCHAR(255) NOT NULL,
    MoTa NVARCHAR(255),
    LaHinhChinh BIT DEFAULT 0,
    CONSTRAINT FK_HinhAnhSanPham_SanPham FOREIGN KEY (SanPhamID) REFERENCES SanPham(SanPhamID)
);
GO

-- Tạo bảng KhachHang
CREATE TABLE KhachHang (
    KhachHangID INT PRIMARY KEY,
    TenKhachHang NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) UNIQUE,
    MatKhau NVARCHAR(255) NOT NULL,
    SoDienThoai NVARCHAR(50),
    DiaChi NVARCHAR(255),
    LaAdmin BIT DEFAULT 0,
    NgayCapNhat DATETIME DEFAULT GETDATE(),
    SoLanDangNhapSai INT DEFAULT 0,
    ThoiGianKhoa DATETIME NULL
);
GO

-- Tạo bảng GioHang
CREATE TABLE GioHang (
    GioHangID INT IDENTITY(1,1) PRIMARY KEY,
    KhachHangID INT,
    SanPhamID INT,
    SoLuong INT NOT NULL,
    CONSTRAINT FK_GioHang_KhachHang FOREIGN KEY (KhachHangID) REFERENCES KhachHang(KhachHangID),
    CONSTRAINT FK_GioHang_SanPham FOREIGN KEY (SanPhamID) REFERENCES SanPham(SanPhamID)
);
GO

-- Tạo bảng DonHang với DonHangID là IDENTITY
CREATE TABLE DonHang (
    DonHangID INT IDENTITY(1,1) PRIMARY KEY,
    KhachHangID INT,
    NgayDatHang DATETIME DEFAULT GETDATE(),
    TongTien DECIMAL(18, 2),
    TrangThaiDonHang NVARCHAR(50),
    PhuongThucThanhToan NVARCHAR(50),
    DiaChiGiaoHang NVARCHAR(255),
    MaDonHang NVARCHAR(20) UNIQUE,
    NgayCapNhat DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_DonHang_KhachHang FOREIGN KEY (KhachHangID) REFERENCES KhachHang(KhachHangID)
);
GO

-- Tạo bảng ChiTietDonHang
CREATE TABLE ChiTietDonHang (
    DonHangID INT,
    SanPhamID INT,
    SoLuong INT,
    Gia DECIMAL(18, 2),
    CONSTRAINT PK_ChiTietDonHang PRIMARY KEY (DonHangID, SanPhamID),
    CONSTRAINT FK_ChiTietDonHang_DonHang FOREIGN KEY (DonHangID) REFERENCES DonHang(DonHangID),
    CONSTRAINT FK_ChiTietDonHang_SanPham FOREIGN KEY (SanPhamID) REFERENCES SanPham(SanPhamID)
);
GO

-- Tạo bảng UserViews
CREATE TABLE UserViews (
    ViewID INT IDENTITY(1,1) PRIMARY KEY,
    KhachHangID INT,
    SanPhamID INT,
    NgayXem DATETIME DEFAULT GETDATE(),
    ThoiGianGui DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_UserViews_KhachHang FOREIGN KEY (KhachHangID) REFERENCES KhachHang(KhachHangID),
    CONSTRAINT FK_UserViews_SanPham FOREIGN KEY (SanPhamID) REFERENCES SanPham(SanPhamID),
    CONSTRAINT UQ_UserViews_KhachHang_SanPham UNIQUE (KhachHangID, SanPhamID)
);
GO

-- Tạo bảng DanhGia
CREATE TABLE DanhGia (
    DanhGiaID INT IDENTITY(1,1) PRIMARY KEY,
    KhachHangID INT,
    SanPhamID INT,
    Diem FLOAT CHECK (Diem BETWEEN 0 AND 5),
    NhanXet NVARCHAR(MAX),
    NgayDanhGia DATETIME DEFAULT GETDATE(),
    ThoiGianGui DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_DanhGia_KhachHang FOREIGN KEY (KhachHangID) REFERENCES KhachHang(KhachHangID),
    CONSTRAINT FK_DanhGia_SanPham FOREIGN KEY (SanPhamID) REFERENCES SanPham(SanPhamID),
    CONSTRAINT UQ_DanhGia_KhachHang_SanPham UNIQUE (KhachHangID, SanPhamID)
);
GO

-- Tạo bảng KhuyenMai
CREATE TABLE KhuyenMai (
    KhuyenMaiID INT PRIMARY KEY,
    TenKhuyenMai NVARCHAR(255) NOT NULL,
    MoTa NVARCHAR(MAX),
    GiaTriGiam DECIMAL(18, 2),
    TyLeGiam FLOAT,
    NgayBatDau DATE,
    NgayKetThuc DATE,
    DieuKienApDung NVARCHAR(MAX)
);
GO

-- Tạo bảng KhuyenMaiSanPham
CREATE TABLE KhuyenMaiSanPham (
    KhuyenMaiID INT,
    SanPhamID INT,
    CONSTRAINT PK_KhuyenMaiSanPham PRIMARY KEY (KhuyenMaiID, SanPhamID),
    CONSTRAINT FK_KhuyenMaiSanPham_KhuyenMai FOREIGN KEY (KhuyenMaiID) REFERENCES KhuyenMai(KhuyenMaiID),
    CONSTRAINT FK_KhuyenMaiSanPham_SanPham FOREIGN KEY (SanPhamID) REFERENCES SanPham(SanPhamID)
);
GO

-- Tạo bảng SanPhamLienQuan
CREATE TABLE SanPhamLienQuan (
    SanPhamChinhID INT,
    SanPhamLienQuanID INT,
    LoaiQuanHe NVARCHAR(50),
    UuTien INT DEFAULT 0,
    CONSTRAINT PK_SanPhamLienQuan PRIMARY KEY (SanPhamChinhID, SanPhamLienQuanID),
    CONSTRAINT FK_SanPhamLienQuan_SanPhamChinh FOREIGN KEY (SanPhamChinhID) REFERENCES SanPham(SanPhamID),
    CONSTRAINT FK_SanPhamLienQuan_SanPhamLienQuan FOREIGN KEY (SanPhamLienQuanID) REFERENCES SanPham(SanPhamID)
);
GO

-- Tạo bảng SanPhamThuongMuaCung
CREATE TABLE SanPhamThuongMuaCung (
    SanPhamID1 INT,
    SanPhamID2 INT,
    SoLanMuaCung INT DEFAULT 0,
    CONSTRAINT PK_SanPhamThuongMuaCung PRIMARY KEY (SanPhamID1, SanPhamID2),
    CONSTRAINT FK_SanPhamThuongMuaCung_SanPham1 FOREIGN KEY (SanPhamID1) REFERENCES SanPham(SanPhamID),
    CONSTRAINT FK_SanPhamThuongMuaCung_SanPham2 FOREIGN KEY (SanPhamID2) REFERENCES SanPham(SanPhamID)
);
GO


CREATE TABLE SanPhamYeuThich (
    SanPhamYeuThichID INT PRIMARY KEY IDENTITY(1,1),
    KhachHangID INT NOT NULL,
    SanPhamID INT NOT NULL,
    NgayThem DATETIME NOT NULL DEFAULT GETDATE(),
    FOREIGN KEY (KhachHangID) REFERENCES KhachHang(KhachHangID),
    FOREIGN KEY (SanPhamID) REFERENCES SanPham(SanPhamID)
);
GO

-- Tạo các index
CREATE INDEX IX_SanPham_DanhMucID ON SanPham(DanhMucID);
CREATE INDEX IX_DonHang_KhachHangID ON DonHang(KhachHangID);
CREATE INDEX IX_ChiTietDonHang_SanPhamID ON ChiTietDonHang(SanPhamID);
CREATE INDEX IX_UserViews_KhachHangID ON UserViews(KhachHangID);
CREATE INDEX IX_DanhGia_SanPhamID ON DanhGia(SanPhamID);
GO

-- Thêm dữ liệu mẫu

-- DanhMuc
INSERT INTO DanhMuc (DanhMucID, TenDanhMuc) 
VALUES 
    (1, N'Laptop'), 
    (2, N'Desktop'), 
    (3, N'Tablet'), 
    (4, N'Monitor'), 
    (5, N'Accessories');
GO

-- SanPham
INSERT INTO SanPham (SanPhamID, TenSP, MoTa, Gia, DanhMucID, SoLuongKho, ThuongHieu, ViXuLy, RAM, LuuTru, KichThuocManHinh, MauSac, DungLuongPin, DanhGia, LuotXem, TrangThai)
VALUES 
    (1, N'Dell XPS 13', N'Lightweight laptop', 25000000.00, 1, 20, N'Dell', N'Intel Core i5-1235U', N'8GB', N'256GB SSD', N'13.4 inches', N'Silver', N'5000mAh', 4.6, 150, N'ConHang'),
    (2, N'HP Pavilion Desktop', N'Affordable desktop', 12000000.00, 2, 15, N'HP', N'Intel Core i3-12100', N'8GB', N'512GB SSD', N'N/A', N'Black', N'N/A', 4.2, 80, N'ConHang'),
    (3, N'Samsung Galaxy Tab S8', N'High-performance tablet', 20000000.00, 3, 25, N'Samsung', N'Snapdragon 8 Gen 1', N'8GB', N'128GB', N'11 inches', N'Grey', N'8000mAh', 4.8, 120, N'ConHang'),
    (4, N'LG UltraGear 27GL850', N'27-inch gaming monitor', 9000000.00, 4, 30, N'LG', N'N/A', N'N/A', N'N/A', N'27 inches', N'Black', N'N/A', 4.5, 90, N'ConHang'),
    (5, N'Logitech MX Master 3', N'Ergonomic mouse', 2500000.00, 5, 50, N'Logitech', N'N/A', N'N/A', N'N/A', N'N/A', N'Black', N'N/A', 4.7, 200, N'ConHang'),
    (6, N'MacBook Air M2', N'Powerful ultrabook', 30000000.00, 1, 10, N'Apple', N'M2', N'16GB', N'512GB SSD', N'13.6 inches', N'Space Grey', N'6000mAh', 4.9, 130, N'ConHang'),
    (7, N'ASUS ROG Strix G15', N'Gaming laptop', 35000000.00, 1, 8, N'ASUS', N'AMD Ryzen 7 6800H', N'16GB', N'1TB SSD', N'15.6 inches', N'Black', N'7500mAh', 4.8, 110, N'ConHang'),
    (8, N'Alienware Aurora R13', N'High-end desktop', 50000000.00, 2, 5, N'Alienware', N'Intel Core i9-12900F', N'32GB', N'1TB SSD', N'N/A', N'White', N'N/A', 4.9, 70, N'ConHang'),
    (9, N'Microsoft Surface Pro 9', N'2-in-1 tablet', 28000000.00, 3, 15, N'Microsoft', N'Intel Core i5-1235U', N'8GB', N'256GB SSD', N'13 inches', N'Platinum', N'7000mAh', 4.6, 95, N'ConHang'),
    (10, N'Dell UltraSharp U2720Q', N'4K monitor', 12000000.00, 4, 12, N'Dell', N'N/A', N'N/A', N'N/A', N'27 inches', N'Black', N'N/A', 4.7, 85, N'ConHang'),
    (11, N'Razer DeathAdder V2', N'Gaming mouse', 1500000.00, 5, 40, N'Razer', N'N/A', N'N/A', N'N/A', N'N/A', N'Black', N'N/A', 4.5, 180, N'ConHang'),
    (12, N'Lenovo IdeaPad 5', N'Budget laptop', 18000000.00, 1, 20, N'Lenovo', N'AMD Ryzen 5 5500U', N'8GB', N'512GB SSD', N'15.6 inches', N'Grey', N'5000mAh', 4.4, 100, N'ConHang'),
    (13, N'Acer Aspire TC', N'Compact desktop', 14000000.00, 2, 18, N'Acer', N'Intel Core i5-12400', N'8GB', N'256GB SSD', N'N/A', N'Black', N'N/A', 4.3, 60, N'ConHang'),
    (14, N'iPad Mini 6', N'Compact tablet', 15000000.00, 3, 25, N'Apple', N'A15 Bionic', N'4GB', N'64GB', N'8.3 inches', N'Purple', N'5000mAh', 4.7, 140, N'ConHang'),
    (15, N'Samsung Odyssey G5', N'Curved monitor', 8000000.00, 4, 15, N'Samsung', N'N/A', N'N/A', N'N/A', N'32 inches', N'Black', N'N/A', 4.6, 75, N'ConHang'),
    (16, N'Corsair K95 RGB', N'Gaming keyboard', 4500000.00, 5, 30, N'Corsair', N'N/A', N'N/A', N'N/A', N'N/A', N'Black', N'N/A', 4.8, 160, N'ConHang'),
    (17, N'MSI GF63 Thin', N'Affordable gaming laptop', 22000000.00, 1, 12, N'MSI', N'Intel Core i5-11400H', N'16GB', N'512GB SSD', N'15.6 inches', N'Black', N'4500mAh', 4.5, 90, N'ConHang'),
    (18, N'HP EliteDesk 800', N'Business desktop', 20000000.00, 2, 10, N'HP', N'Intel Core i7-12700', N'16GB', N'512GB SSD', N'N/A', N'Black', N'N/A', 4.4, 50, N'ConHang'),
    (19, N'Lenovo Tab M10', N'Budget tablet', 6000000.00, 3, 30, N'Lenovo', N'MediaTek Helio P22T', N'4GB', N'64GB', N'10.1 inches', N'Slate Black', N'5000mAh', 4.3, 110, N'ConHang'),
    (20, N'Sony WH-1000XM5', N'Noise-cancelling headphones', 9000000.00, 5, 20, N'Sony', N'N/A', N'N/A', N'N/A', N'N/A', N'Black', N'1200mAh', 4.9, 130, N'ConHang');
GO

-- HinhAnhSanPham
INSERT INTO HinhAnhSanPham (SanPhamID, DuongDanAnh, MoTa, LaHinhChinh)
VALUES 
	(1, N'/images/products/dell_xps_13_main.jpg', N'Hình chính', 1),
	(1, N'/images/products/dell_xps_13/dell_xps_13_side.jpg', N'Hình bên trái', 0),
	(2, N'/images/products/hp_pavilion_desktop/hp_pavilion_desktop_main.jpg', N'Hình chính', 1),
	(3, N'/images/products/samsung_galaxy_tab_s8/samsung_galaxy_tab_s8_main.jpg', N'Hình chính', 1),
	(4, N'/images/products/lg_ultragear_27gl850/lg_ultragear_27gl850_main.jpg', N'Hình chính', 1),
	(5, N'/images/products/logitech_mx_master_3/logitech_mx_master_3_main.jpg', N'Hình chính', 1),
	(5, N'/images/products/logitech_mx_master_3/logitech_mx_master_3_top.jpg', N'Hình từ trên', 0),
	(6, N'/images/products/macbook_air_m2/macbook_air_m2_main.jpg', N'Hình chính', 1),
	(6, N'/images/products/macbook_air_m2/macbook_air_m2_side.jpg', N'Hình bên phải', 0),
	(7, N'/images/products/asus_rog_strix_g15/asus_rog_strix_g15_main.jpg', N'Hình chính', 1),
	(8, N'/images/products/alienware_aurora_r13/alienware_aurora_r13_main.jpg', N'Hình chính', 1),
	(9, N'/images/products/microsoft_surface_pro_9/microsoft_surface_pro_9_main.jpg', N'Hình chính', 1),
	(10, N'/images/products/dell_ultrasharp_u2720q/dell_ultrasharp_u2720q_main.jpg', N'Hình chính', 1),
	(11, N'/images/products/razer_deathadder_v2/razer_deathadder_v2_main.jpg', N'Hình chính', 1),
	(11, N'/images/products/razer_deathadder_v2/razer_deathadder_v2_top.jpg', N'Hình từ trên', 0),
	(12, N'/images/products/lenovo_ideapad_5/lenovo_ideapad_5_main.jpg', N'Hình chính', 1),
	(13, N'/images/products/acer_aspire_tc/acer_aspire_tc_main.jpg', N'Hình chính', 1),
	(14, N'/images/products/ipad_mini_6/ipad_mini_6_main.jpg', N'Hình chính', 1),
	(15, N'/images/products/samsung_odyssey_g5/samsung_odyssey_g5_main.jpg', N'Hình chính', 1),
	(15, N'/images/products/samsung_odyssey_g5/samsung_odyssey_g5_side.jpg', N'Hình bên trái', 0),
	(16, N'/images/products/corsair_k95_rgb/corsair_k95_rgb_main.jpg', N'Hình chính', 1),
	(16, N'/images/products/corsair_k95_rgb/corsair_k95_rgb_side.jpg', N'Hình bên trái', 0),
	(17, N'/images/products/msi_gf63_thin/msi_gf63_thin_main.jpg', N'Hình chính', 1),
	(18, N'/images/products/hp_elitedesk_800/hp_elitedesk_800_main.jpg', N'Hình chính', 1),
	(19, N'/images/products/lenovo_tab_m10/lenovo_tab_m10_main.jpg', N'Hình chính', 1),
	(20, N'/images/products/sony_wh_1000xm5/sony_wh_1000xm5_main.jpg', N'Hình chính', 1),
	(20, N'/images/products/sony_wh_1000xm5/sony_wh_1000xm5_side.jpg', N'Hình bên phải', 0);

GO

-- KhachHang
INSERT INTO KhachHang (KhachHangID, TenKhachHang, Email, MatKhau, SoDienThoai, DiaChi, LaAdmin)
VALUES 
    (1, N'Nguyen Van A', N'nguyenvana@gmail.com', '77d8cc37d976bb412b5c203de58606e9a5ef1549c0bbe07f15b12a99209f0eee', N'0901234567', N'Hanoi', 0),
    (2, N'Tran Thi B', N'tranthib@gmail.com', '77d8cc37d976bb412b5c203de58606e9a5ef1549c0bbe07f15b12a99209f0eee', N'0902345678', N'HCM City', 0),
    (3, N'Le Van C', N'levanc@gmail.com', '77d8cc37d976bb412b5c203de58606e9a5ef1549c0bbe07f15b12a99209f0eee', N'0903456789', N'Danang', 0),
    (4, N'Pham Thi D', N'phamthid@gmail.com', '77d8cc37d976bb412b5c203de58606e9a5ef1549c0bbe07f15b12a99209f0eee', N'0904567890', N'Hue', 1),
    (5, N'Nguyen Hoang Nhat Huy', N'nhathuy@gmail.com', '77d8cc37d976bb412b5c203de58606e9a5ef1549c0bbe07f15b12a99209f0eee', N'0905678901', N'BenTre', 1);
GO

-- GioHang
INSERT INTO GioHang (KhachHangID, SanPhamID, SoLuong)
VALUES 
    (1, 1, 1),
    (1, 5, 1),
    (2, 6, 1);
GO

-- DonHang (DonHangID tự động tăng, không cần chỉ định)
INSERT INTO DonHang (KhachHangID, NgayDatHang, TongTien, TrangThaiDonHang, PhuongThucThanhToan, DiaChiGiaoHang, MaDonHang)
VALUES 
    (1, '2025-03-10 10:00:00', 27500000.00, N'Completed', N'Credit Card', N'Hanoi', 'DH000001'),
    (2, '2025-03-11 14:30:00', 12000000.00, N'Completed', N'Cash on Delivery', N'HCM City', 'DH000002'),
    (3, '2025-03-12 09:15:00', 24500000.00, N'Pending', N'Bank Transfer', N'Danang', 'DH000003'),
    (4, '2025-03-13 16:00:00', 9000000.00, N'Shipping', N'Cash on Delivery', N'Hue', 'DH000004'),
    (5, '2025-03-14 11:45:00', 19500000.00, N'Completed', N'PayPal', N'Haiphong', 'DH000005'),
    (1, '2025-03-15 10:00:00', 18000000.00, N'Completed', N'Credit Card', N'Hanoi', 'DH000006'),
    (2, '2025-03-16 14:00:00', 15000000.00, N'Pending', N'Cash on Delivery', N'HCM City', 'DH000007'),
    (3, '2025-03-16 09:00:00', 40000000.00, N'Completed', N'Bank Transfer', N'Danang', 'DH000008'),
    (1, '2025-03-17 10:00:00', 37500000.00, N'Completed', N'Credit Card', N'Hanoi', 'DH000009');
GO

-- ChiTietDonHang
INSERT INTO ChiTietDonHang (DonHangID, SanPhamID, SoLuong, Gia)
VALUES 
    (1, 1, 1, 25000000.00),
    (1, 5, 1, 2500000.00),
    (2, 2, 1, 12000000.00),
    (3, 7, 1, 35000000.00),
    (4, 4, 1, 9000000.00),
    (5, 12, 1, 18000000.00),
    (5, 11, 1, 1500000.00),
    (6, 12, 1, 18000000.00),
    (7, 14, 1, 15000000.00),
    (8, 7, 1, 35000000.00),
    (8, 11, 1, 1500000.00),
    (9, 1, 1, 25000000.00),
    (9, 5, 1, 2500000.00),
    (9, 16, 1, 4500000.00);
GO

-- UserViews
INSERT INTO UserViews (KhachHangID, SanPhamID, NgayXem, ThoiGianGui)
VALUES 
    (1, 1, '2025-03-09 08:30:00', '2025-03-09 08:30:00'),
    (1, 6, '2025-03-09 09:00:00', '2025-03-09 09:00:00'),
    (2, 2, '2025-03-10 15:00:00', '2025-03-10 15:00:00'),
    (3, 7, '2025-03-11 10:15:00', '2025-03-11 10:15:00'),
    (4, 9, '2025-03-12 13:45:00', '2025-03-12 13:45:00'),
    (5, 12, '2025-03-13 11:30:00', '2025-03-13 11:30:00'),
    (1, 2, '2025-03-09 10:00:00', '2025-03-09 10:00:00'),
    (1, 3, '2025-03-09 11:00:00', '2025-03-09 11:00:00'),
    (2, 7, '2025-03-10 16:00:00', '2025-03-10 16:00:00'),
    (3, 1, '2025-03-11 12:00:00', '2025-03-11 12:00:00'),
    (4, 5, '2025-03-12 14:00:00', '2025-03-12 14:00:00');
GO

-- DanhGia
INSERT INTO DanhGia (KhachHangID, SanPhamID, Diem, NhanXet, NgayDanhGia, ThoiGianGui)
VALUES 
    (1, 1, 4.5, N'Tốt, nhưng pin hơi yếu', '2025-03-11 12:00:00', '2025-03-11 12:00:00'),
    (2, 2, 4.0, N'Hiệu năng ổn, giá hợp lý', '2025-03-12 09:30:00', '2025-03-12 09:30:00'),
    (3, 7, 5.0, N'Tuyệt vời cho chơi game', '2025-03-13 14:00:00', '2025-03-13 14:00:00'),
    (4, 4, 4.5, N'Màu sắc đẹp, chơi game mượt', '2025-03-14 10:15:00', '2025-03-14 10:15:00'),
    (5, 5, 4.8, N'Chuột rất nhạy, đáng tiền', '2025-03-15 08:45:00', '2025-03-15 08:45:00'),
    (1, 6, 4.7, N'Rất tốt, hiệu năng cao', '2025-03-12 13:00:00', '2025-03-12 13:00:00'),
    (2, 9, 4.3, N'Màn hình đẹp', '2025-03-13 09:00:00', '2025-03-13 09:00:00'),
    (3, 11, 4.6, N'Nhạy, phù hợp chơi game', '2025-03-14 15:00:00', '2025-03-14 15:00:00');
GO

-- KhuyenMai
INSERT INTO KhuyenMai (KhuyenMaiID, TenKhuyenMai, MoTa, GiaTriGiam, TyLeGiam, NgayBatDau, NgayKetThuc, DieuKienApDung)
VALUES 
    (1, N'Mua Laptop Tặng Chuột Giảm 10%', N'Giảm 10% giá chuột khi mua cùng laptop', NULL, 0.1, '2025-03-01', '2025-03-31', N'Áp dụng khi mua laptop và chuột cùng lúc');
GO

-- KhuyenMaiSanPham
INSERT INTO KhuyenMaiSanPham (KhuyenMaiID, SanPhamID)
VALUES 
    (1, 1), 
    (1, 5), 
    (1, 6), 
    (1, 11);
GO

-- SanPhamLienQuan
INSERT INTO SanPhamLienQuan (SanPhamChinhID, SanPhamLienQuanID, LoaiQuanHe, UuTien)
VALUES 
    (1, 5, N'Phụ kiện đề xuất', 3),
    (1, 16, N'Phụ kiện đề xuất', 2),
    (1, 20, N'Phụ kiện đề xuất', 1),
    (6, 5, N'Phụ kiện đề xuất', 3),
    (6, 16, N'Phụ kiện đề xuất', 2),
    (6, 11, N'Phụ kiện đề xuất', 3),
    (7, 11, N'Phụ kiện đề xuất', 3),
    (7, 16, N'Phụ kiện đề xuất', 2),
    (7, 15, N'Phụ kiện đề xuất', 1),
    (12, 5, N'Phụ kiện đề xuất', 3),
    (12, 16, N'Phụ kiện đề xuất', 2),
    (12, 20, N'Phụ kiện đề xuất', 1);
GO

-- SanPhamThuongMuaCung
INSERT INTO SanPhamThuongMuaCung (SanPhamID1, SanPhamID2, SoLanMuaCung)
VALUES 
    (1, 5, 2),
    (12, 11, 1),
    (7, 11, 1);
GO

-- Stored Procedures

-- Stored Procedure CapNhatKhoDuaTrenDonHang
CREATE PROCEDURE CapNhatKhoDuaTrenDonHang 
    @DonHangID INT
AS
BEGIN
    DECLARE @SanPhamID INT, @SoLuong INT, @SoLuongKho INT, @TrangThai NVARCHAR(50);
    SELECT @TrangThai = TrangThaiDonHang FROM DonHang WHERE DonHangID = @DonHangID;
    IF @TrangThai != N'Completed'
    BEGIN
        PRINT 'Don hang chua hoan thanh, khong cap nhat kho.';
        RETURN;
    END
    DECLARE cur CURSOR FOR SELECT SanPhamID, SoLuong FROM ChiTietDonHang WHERE DonHangID = @DonHangID;
    OPEN cur;
    FETCH NEXT FROM cur INTO @SanPhamID, @SoLuong;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT @SoLuongKho = SoLuongKho FROM SanPham WHERE SanPhamID = @SanPhamID;
        IF @SoLuongKho < @SoLuong
        BEGIN
            PRINT 'KHONG DU SO LUONG SanPhamID ' + CAST(@SanPhamID AS NVARCHAR(10));
            ROLLBACK TRANSACTION;
            BREAK;
        END
        ELSE
        BEGIN
            UPDATE SanPham SET SoLuongKho = SoLuongKho - @SoLuong WHERE SanPhamID = @SanPhamID;
            PRINT 'SO LUONG DUOC CAP NHAT SanPhamID ' + CAST(@SanPhamID AS NVARCHAR(10));
        END
        FETCH NEXT FROM cur INTO @SanPhamID, @SoLuong;
    END
    CLOSE cur;
    DEALLOCATE cur;
END;
GO

-- Stored Procedure CapNhatSanPhamThuongMuaCung
CREATE PROCEDURE CapNhatSanPhamThuongMuaCung
    @DonHangID INT,
    @SupportThreshold INT = 2
AS
BEGIN
    CREATE TABLE #TempPairs (SanPhamID1 INT, SanPhamID2 INT, SoLanMuaCung INT);
    INSERT INTO #TempPairs (SanPhamID1, SanPhamID2, SoLanMuaCung)
    SELECT c1.SanPhamID, c2.SanPhamID, 1
    FROM ChiTietDonHang c1
    JOIN ChiTietDonHang c2 ON c1.DonHangID = c2.DonHangID AND c1.SanPhamID < c2.SanPhamID
    WHERE c1.DonHangID = @DonHangID;
    MERGE SanPhamThuongMuaCung AS target
    USING #TempPairs AS source
    ON (target.SanPhamID1 = source.SanPhamID1 AND target.SanPhamID2 = source.SanPhamID2)
    WHEN MATCHED THEN
        UPDATE SET SoLanMuaCung = target.SoLanMuaCung + source.SoLanMuaCung
    WHEN NOT MATCHED THEN
        INSERT (SanPhamID1, SanPhamID2, SoLanMuaCung)
        VALUES (source.SanPhamID1, source.SanPhamID2, source.SoLanMuaCung);
    DELETE FROM SanPhamThuongMuaCung WHERE SoLanMuaCung < @SupportThreshold;
    DROP TABLE #TempPairs;
END;
GO

-- Chạy các stored procedure để cập nhật dữ liệu
EXEC CapNhatKhoDuaTrenDonHang 1;
EXEC CapNhatSanPhamThuongMuaCung 1, 2;
EXEC CapNhatSanPhamThuongMuaCung 5, 2;
EXEC CapNhatSanPhamThuongMuaCung 8, 2;
EXEC CapNhatSanPhamThuongMuaCung 9, 2;
GO


-----------------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------------
-----------------------------------------------------------------------------------------------------------------------------------

-- Phần 1: Tạo thêm khách hàng (10000 khách hàng)
PRINT 'Bắt đầu tạo khách hàng...';
DECLARE @StartKhachHangID INT = (SELECT ISNULL(MAX(KhachHangID), 0) + 1 FROM KhachHang);
DECLARE @EndKhachHangID INT = @StartKhachHangID + 9999; -- Tạo thêm 1000 khách hàng
DECLARE @CurrentKhachHangID INT = @StartKhachHangID;

-- Tạo bảng tạm chứa họ và tên------------------------------------------
CREATE TABLE #HoVaTen (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    Ho NVARCHAR(50),
    Ten NVARCHAR(50)
);

-- Thêm dữ liệu họ và tên
INSERT INTO #HoVaTen (Ho, Ten) VALUES 
(N'Nguyễn', N'An'), (N'Nguyễn', N'Bình'), (N'Nguyễn', N'Cường'), (N'Nguyễn', N'Dũng'), (N'Nguyễn', N'Hà'),
(N'Trần', N'Hải'), (N'Trần', N'Hùng'), (N'Trần', N'Linh'), (N'Trần', N'Minh'), (N'Trần', N'Nam'),
(N'Lê', N'Phương'), (N'Lê', N'Quân'), (N'Lê', N'Sơn'), (N'Lê', N'Thảo'), (N'Lê', N'Uyên'),
(N'Phạm', N'Việt'), (N'Phạm', N'Xuân'), (N'Phạm', N'Yến'), (N'Phạm', N'Anh'), (N'Phạm', N'Bảo'),
(N'Hoàng', N'Chi'), (N'Hoàng', N'Đạt'), (N'Hoàng', N'Giang'), (N'Hoàng', N'Hiếu'), (N'Hoàng', N'Khánh'),
(N'Vũ', N'Lâm'), (N'Vũ', N'Mai'), (N'Vũ', N'Ngọc'), (N'Vũ', N'Oanh'), (N'Vũ', N'Phúc'),
(N'Đặng', N'Quỳnh'), (N'Đặng', N'Rạng'), (N'Đặng', N'Sáng'), (N'Đặng', N'Tuấn'), (N'Đặng', N'Uy'),
(N'Bùi', N'Vân'), (N'Bùi', N'Xuyến'), (N'Bùi', N'Yên'), (N'Bùi', N'Zung'), (N'Bùi', N'Ánh');

-- Tạo bảng tạm chứa tỉnh thành----------------------------------------------
CREATE TABLE #TinhThanh (
    ID INT IDENTITY(1,1) PRIMARY KEY,
    TenTinh NVARCHAR(100)
);

-- Thêm dữ liệu tỉnh thành
INSERT INTO #TinhThanh (TenTinh) VALUES 
(N'Hà Nội'), (N'TP. Hồ Chí Minh'), (N'Đà Nẵng'), (N'Hải Phòng'), (N'Cần Thơ'),
(N'An Giang'), (N'Bà Rịa - Vũng Tàu'), (N'Bắc Giang'), (N'Bắc Kạn'), (N'Bạc Liêu'),
(N'Bắc Ninh'), (N'Bến Tre'), (N'Bình Định'), (N'Bình Dương'), (N'Bình Phước'),
(N'Bình Thuận'), (N'Cà Mau'), (N'Cao Bằng'), (N'Đắk Lắk'), (N'Đắk Nông'),
(N'Điện Biên'), (N'Đồng Nai'), (N'Đồng Tháp'), (N'Gia Lai'), (N'Hà Giang'),
(N'Hà Nam'), (N'Hà Tĩnh'), (N'Hải Dương'), (N'Hậu Giang'), (N'Hòa Bình'),
(N'Hưng Yên'), (N'Khánh Hòa'), (N'Kiên Giang'), (N'Kon Tum'), (N'Lai Châu'),
(N'Lâm Đồng'), (N'Lạng Sơn'), (N'Lào Cai'), (N'Long An'), (N'Nam Định'),
(N'Nghệ An'), (N'Ninh Bình'), (N'Ninh Thuận'), (N'Phú Thọ'), (N'Phú Yên'),
(N'Quảng Bình'), (N'Quảng Nam'), (N'Quảng Ngãi'), (N'Quảng Ninh'), (N'Quảng Trị'),
(N'Sóc Trăng'), (N'Sơn La'), (N'Tây Ninh'), (N'Thái Bình'), (N'Thái Nguyên'),
(N'Thanh Hóa'), (N'Thừa Thiên Huế'), (N'Tiền Giang'), (N'Trà Vinh'), (N'Tuyên Quang'),
(N'Vĩnh Long'), (N'Vĩnh Phúc'), (N'Yên Bái');

BEGIN TRY
    BEGIN TRANSACTION;

    WHILE @CurrentKhachHangID <= @EndKhachHangID
    BEGIN
        -- Chọn ngẫu nhiên họ và tên
        DECLARE @RandomHoID INT = FLOOR(RAND() * 40) + 1;
        DECLARE @RandomTenID INT = FLOOR(RAND() * 40) + 1;
        DECLARE @RandomTinhID INT = FLOOR(RAND() * 63) + 1;
        
        DECLARE @Ho NVARCHAR(50), @Ten NVARCHAR(50), @TenTinh NVARCHAR(100);
        
        SELECT @Ho = Ho FROM #HoVaTen WHERE ID = @RandomHoID;
        SELECT @Ten = Ten FROM #HoVaTen WHERE ID = @RandomTenID;
        SELECT @TenTinh = TenTinh FROM #TinhThanh WHERE ID = @RandomTinhID;
        
        -- Tạo tên đầy đủ
        DECLARE @TenKhachHang NVARCHAR(100) = @Ho + N' ' + @Ten;
        
        -- Tạo email - Sửa lỗi chuyển đổi
        DECLARE @FirstChar NVARCHAR(1) = '';
        IF LEN(@Ten) > 0 
            SET @FirstChar = SUBSTRING(@Ten, 1, 1);
            
        DECLARE @EmailPrefix NVARCHAR(100) = LOWER(@FirstChar + @Ho);
        DECLARE @EmailID VARCHAR(10) = CONVERT(VARCHAR(10), @CurrentKhachHangID);
        DECLARE @Email NVARCHAR(150) = @EmailPrefix + @EmailID + '@gmail.com';-------------------------------------------------------------
        
        -- Loại bỏ dấu tiếng Việt trong email           --=================================================================
        SET @Email = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                     REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                     REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                     REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                     REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                     @Email,
                     N'á', 'a'), N'à', 'a'), N'ả', 'a'), N'ã', 'a'), N'ạ', 'a'),  
                     N'ă', 'a'), N'ắ', 'a'), N'ằ', 'a'), N'ẳ', 'a'), N'ẵ', 'a'),
                     N'ặ', 'a'), N'â', 'a'), N'ấ', 'a'), N'ầ', 'a'), N'ẩ', 'a'),
                     N'ẫ', 'a'), N'ậ', 'a'), N'đ', 'd'), N'é', 'e'), N'è', 'e'),
                     N'ẻ', 'e'), N'ẽ', 'e'), N'ẹ', 'e'), N'ê', 'e'), N'ế', 'e');
        
        -- Tạo số điện thoại - Sửa lỗi chuyển đổi
        DECLARE @PhoneDigits VARCHAR(8) = '';
        DECLARE @i INT = 1;
        
        -- Tạo 8 chữ số ngẫu nhiên từ 0-9
        WHILE @i <= 8
        BEGIN
            SET @PhoneDigits = @PhoneDigits + CAST(FLOOR(RAND() * 10) AS VARCHAR(1));
            SET @i = @i + 1;
        END
        
        DECLARE @SoDienThoai VARCHAR(15) = '09' + @PhoneDigits; ----------------------------------
        
        -- Tạo địa chỉ - Sửa lỗi chuyển đổi
        DECLARE @SoNha INT = FLOOR(RAND() * 200) + 1;
        DECLARE @Duong INT = FLOOR(RAND() * 100) + 1;
        DECLARE @DiaChi NVARCHAR(200) = N'Số ' + CAST(@SoNha AS VARCHAR(5)) + N', Đường ' + CAST(@Duong AS VARCHAR(5)) + N', ' + @TenTinh;
        
        -- Thêm khách hàng
        INSERT INTO KhachHang (KhachHangID, TenKhachHang, Email, MatKhau, SoDienThoai, DiaChi, LaAdmin)
        VALUES (
            @CurrentKhachHangID, 
            @TenKhachHang, 
            @Email, 
            '77d8cc37d976bb412b5c203de58606e9a5ef1549c0bbe07f15b12a99209f0eee', -- Mật khẩu mặc định
            @SoDienThoai, 
            @DiaChi, 
            0 -- Không phải admin
        );
        
        SET @CurrentKhachHangID = @CurrentKhachHangID + 1;
        
        -- In tiến trình sau mỗi 100 khách hàng
        IF @CurrentKhachHangID % 100 = 0
            PRINT 'Đã tạo ' + CAST(@CurrentKhachHangID - @StartKhachHangID AS VARCHAR(10)) + ' khách hàng...';
    END

    -- Xóa bảng tạm
    DROP TABLE #HoVaTen;
    DROP TABLE #TinhThanh;
    
    COMMIT TRANSACTION;
    PRINT 'Hoàn thành tạo ' + CAST(@EndKhachHangID - @StartKhachHangID + 1 AS VARCHAR(10)) + ' khách hàng.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    PRINT 'Lỗi khi tạo khách hàng: ' + ERROR_MESSAGE();
    
    -- Xóa bảng tạm nếu tồn tại
    IF OBJECT_ID('tempdb..#HoVaTen') IS NOT NULL
        DROP TABLE #HoVaTen;
    IF OBJECT_ID('tempdb..#TinhThanh') IS NOT NULL
        DROP TABLE #TinhThanh;
END CATCH
GO

-- Phần 2: Tạo dữ liệu lượt xem sản phẩm (100,000 lượt xem)
PRINT 'Bắt đầu tạo lượt xem sản phẩm...';

-- Tạo bảng tạm để lưu trữ các cặp khách hàng-sản phẩm đã tồn tại --------------------------------------------------------------
CREATE TABLE #ExistingViews (
    KhachHangID INT,
    SanPhamID INT
);

-- Chèn dữ liệu hiện có vào bảng tạm
INSERT INTO #ExistingViews (KhachHangID, SanPhamID)
SELECT KhachHangID, SanPhamID FROM UserViews;

-- Tạo dữ liệu lượt xem mới
DECLARE @ViewCount INT = 100000; -- Số lượng lượt xem cần tạo
DECLARE @Counter INT = 1;
DECLARE @MaxKhachHangID INT = (SELECT MAX(KhachHangID) FROM KhachHang);
DECLARE @MaxSanPhamID INT = (SELECT MAX(SanPhamID) FROM SanPham);
DECLARE @BatchSize INT = 1000; -- Số lượng bản ghi mỗi lần insert
DECLARE @CurrentBatch INT = 1;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Tạo bảng tạm để lưu trữ dữ liệu trước khi insert hàng loạt
    CREATE TABLE #TempViews (
        KhachHangID INT,
        SanPhamID INT,
        NgayXem DATETIME,
        ThoiGianGui DATETIME
    );

    WHILE @Counter <= @ViewCount
    BEGIN
        -- Tạo ID khách hàng và sản phẩm ngẫu nhiên
        DECLARE @RandomKhachHangID INT = FLOOR(RAND() * @MaxKhachHangID) + 1;
        DECLARE @RandomSanPhamID INT = FLOOR(RAND() * @MaxSanPhamID) + 1;
        
        -- Kiểm tra xem cặp khách hàng-sản phẩm đã tồn tại chưa ---------------------------------------------------------------------------------------------------------
        IF NOT EXISTS (SELECT 1 FROM #ExistingViews 
                      WHERE KhachHangID = @RandomKhachHangID AND SanPhamID = @RandomSanPhamID)
        BEGIN
            -- Tạo ngày xem ngẫu nhiên trong 1 năm gần đây
            DECLARE @RandomDate DATETIME = DATEADD(DAY, -FLOOR(RAND() * 365), GETDATE());
            
            -- Thêm vào bảng tạm
            INSERT INTO #TempViews (KhachHangID, SanPhamID, NgayXem, ThoiGianGui)
            VALUES (@RandomKhachHangID, @RandomSanPhamID, @RandomDate, @RandomDate);
            
            -- Thêm vào bảng tạm để tránh trùng lặp
            INSERT INTO #ExistingViews (KhachHangID, SanPhamID)
            VALUES (@RandomKhachHangID, @RandomSanPhamID);
            
            SET @Counter = @Counter + 1;
            
            -- Insert hàng loạt sau mỗi @BatchSize bản ghi
            IF @Counter % @BatchSize = 0
            BEGIN
                INSERT INTO UserViews (KhachHangID, SanPhamID, NgayXem, ThoiGianGui)
                SELECT KhachHangID, SanPhamID, NgayXem, ThoiGianGui FROM #TempViews;
                
                -- Xóa dữ liệu trong bảng tạm
                TRUNCATE TABLE #TempViews;
                
                PRINT 'Đã tạo ' + CAST(@Counter AS NVARCHAR(10)) + ' lượt xem...';
                SET @CurrentBatch = @CurrentBatch + 1;
            END
        END
    END

    -- Insert các bản ghi còn lại
    IF EXISTS (SELECT 1 FROM #TempViews)
    BEGIN
        INSERT INTO UserViews (KhachHangID, SanPhamID, NgayXem, ThoiGianGui)
        SELECT KhachHangID, SanPhamID, NgayXem, ThoiGianGui FROM #TempViews;
    END

    -- Xóa bảng tạm
    DROP TABLE #TempViews;
    DROP TABLE #ExistingViews;
    
    -- Cập nhật số lượt xem cho sản phẩm
    UPDATE SanPham
    SET LuotXem = (
        SELECT COUNT(*) 
        FROM UserViews 
        WHERE UserViews.SanPhamID = SanPham.SanPhamID
    );
    
    COMMIT TRANSACTION;
    PRINT 'Hoàn thành tạo ' + CAST(@ViewCount AS NVARCHAR(10)) + ' lượt xem.';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Lỗi khi tạo lượt xem: ' + ERROR_MESSAGE();
    
    -- Xóa bảng tạm nếu tồn tại
    IF OBJECT_ID('tempdb..#TempViews') IS NOT NULL
        DROP TABLE #TempViews;
    IF OBJECT_ID('tempdb..#ExistingViews') IS NOT NULL
        DROP TABLE #ExistingViews;
END CATCH
GO

-- Phần 3: Tạo dữ liệu đánh giá sản phẩm (5,000 đánh giá)
PRINT 'Bắt đầu tạo đánh giá sản phẩm...';

-- Tạo bảng tạm để lưu trữ các cặp khách hàng-sản phẩm đã tồn tại ------------------------------------------------------------
CREATE TABLE #ExistingRatings (
    KhachHangID INT,
    SanPhamID INT
);

-- Chèn dữ liệu hiện có vào bảng tạm
INSERT INTO #ExistingRatings (KhachHangID, SanPhamID)
SELECT KhachHangID, SanPhamID FROM DanhGia;

-- Tạo dữ liệu đánh giá mới
DECLARE @RatingCount INT = 8000; -- Số lượng đánh giá cần tạo
DECLARE @Counter INT = 1;
DECLARE @MaxKhachHangID INT = (SELECT MAX(KhachHangID) FROM KhachHang);
DECLARE @MaxSanPhamID INT = (SELECT MAX(SanPhamID) FROM SanPham);
DECLARE @BatchSize INT = 500; -- Số lượng bản ghi mỗi lần insert
DECLARE @CurrentBatch INT = 1;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Tạo bảng tạm để lưu trữ dữ liệu trước khi insert hàng loạt
    CREATE TABLE #TempRatings (
        KhachHangID INT,
        SanPhamID INT,
        Diem FLOAT,
        NhanXet NVARCHAR(MAX),
        NgayDanhGia DATETIME,
        ThoiGianGui DATETIME
    );

    -- Tạo bảng tạm chứa các mẫu nhận xét
    CREATE TABLE #Comments (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        Comment NVARCHAR(MAX),
        Rating FLOAT -- Điểm đánh giá tương ứng
    );

    -- Thêm dữ liệu mẫu nhận xét với điểm đánh giá tương ứng
    -- Nhận xét 5 sao
    INSERT INTO #Comments (Comment, Rating) VALUES 
    (N'Sản phẩm tuyệt vời, đúng như mô tả. Tôi rất hài lòng với chất lượng và dịch vụ.', 5),
    (N'Chất lượng vượt mong đợi, giao hàng nhanh, đóng gói cẩn thận.', 5),
    (N'Tuyệt vời, sẽ mua lại và giới thiệu cho bạn bè.', 5),
    (N'Sản phẩm chính hãng, hiệu năng tốt, rất đáng tiền.', 5),
    (N'Thiết kế đẹp, chất lượng cao, dịch vụ khách hàng tuyệt vời.', 5),
    (N'Hoàn hảo về mọi mặt, không có gì để phàn nàn.', 5),
    (N'Sản phẩm vượt xa kỳ vọng, rất hài lòng với trải nghiệm mua sắm.', 5),
    (N'Chất lượng xứng đáng với giá tiền, sẽ ủng hộ shop lâu dài.', 5);

    -- Nhận xét 4 sao
    INSERT INTO #Comments (Comment, Rating) VALUES 
    (N'Sản phẩm tốt, giao hàng hơi chậm nhưng vẫn chấp nhận được.', 4),
    (N'Chất lượng ổn, giá cả hợp lý, chỉ có vài chi tiết nhỏ cần cải thiện.', 4),
    (N'Hài lòng với sản phẩm, nhưng hướng dẫn sử dụng không rõ ràng.', 4),
    (N'Sản phẩm đúng như mô tả, chỉ thiếu một vài phụ kiện nhỏ.', 4),
    (N'Hiệu năng tốt, thiết kế đẹp, nhưng pin không được như kỳ vọng.', 4),
    (N'Tốt nhưng còn một số điểm cần cải thiện.', 4),
    (N'Sản phẩm chất lượng cao, nhưng giá hơi cao so với thị trường.', 4);

    -- Nhận xét 3 sao
    INSERT INTO #Comments (Comment, Rating) VALUES 
    (N'Sản phẩm tạm được, không xuất sắc nhưng cũng không tệ.', 3),
    (N'Chất lượng trung bình, giá hơi cao so với chất lượng.', 3),
    (N'Có một số vấn đề nhỏ nhưng vẫn sử dụng được.', 3),
    (N'Sản phẩm không hoàn toàn như mô tả, hơi thất vọng.', 3),
    (N'Hiệu năng trung bình, không đáng với giá tiền.', 3),
    (N'Giao hàng chậm, sản phẩm ổn.', 3);

    -- Nhận xét 2 sao
    INSERT INTO #Comments (Comment, Rating) VALUES 
    (N'Sản phẩm kém chất lượng, không như mô tả.', 2),
    (N'Nhiều vấn đề, không đáng với số tiền bỏ ra.', 2),
    (N'Thất vọng với chất lượng và dịch vụ.', 2),
    (N'Sản phẩm dễ hỏng, không bền.', 2);

    -- Nhận xét 1 sao
    INSERT INTO #Comments (Comment, Rating) VALUES 
    (N'Sản phẩm tệ, hoàn toàn không như mô tả.', 1),
    (N'Rất thất vọng, sẽ không bao giờ mua lại.', 1),
    (N'Chất lượng kém, dịch vụ tệ.', 1);

    WHILE @Counter <= @RatingCount
    BEGIN
        -- Tạo ID khách hàng và sản phẩm ngẫu nhiên
        DECLARE @RandomKhachHangID INT = FLOOR(RAND() * @MaxKhachHangID) + 1;
        DECLARE @RandomSanPhamID INT = FLOOR(RAND() * @MaxSanPhamID) + 1;
        
        -- Kiểm tra xem cặp khách hàng-sản phẩm đã tồn tại chưa
        IF NOT EXISTS (SELECT 1 FROM #ExistingRatings 
                      WHERE KhachHangID = @RandomKhachHangID AND SanPhamID = @RandomSanPhamID)
        BEGIN
            -- Tạo ngày đánh giá ngẫu nhiên trong 1 năm gần đây
            DECLARE @RandomDate DATETIME = DATEADD(DAY, -FLOOR(RAND() * 365), GETDATE());
            
            -- Tạo điểm đánh giá ngẫu nhiên với phân phối thiên về điểm cao -=================================================================
            DECLARE @RandomRating FLOAT;
            DECLARE @RatingRand FLOAT = RAND();
            
            IF @RatingRand < 0.5 -- 50% là 5 sao
                SET @RandomRating = 5;
            ELSE IF @RatingRand < 0.8 -- 30% là 4 sao
                SET @RandomRating = 4;
            ELSE IF @RatingRand < 0.9 -- 10% là 3 sao
                SET @RandomRating = 3;
            ELSE IF @RatingRand < 0.95 -- 5% là 2 sao
                SET @RandomRating = 2;
            ELSE -- 5% là 1 sao
                SET @RandomRating = 1;
            
            -- Chọn nhận xét ngẫu nhiên phù hợp với điểm đánh giá
            DECLARE @RandomComment NVARCHAR(MAX);
            
            SELECT TOP 1 @RandomComment = Comment 
            FROM #Comments 
            WHERE Rating = @RandomRating
            ORDER BY NEWID();
            
            -- Thêm vào bảng tạm
            INSERT INTO #TempRatings (KhachHangID, SanPhamID, Diem, NhanXet, NgayDanhGia, ThoiGianGui)
            VALUES (@RandomKhachHangID, @RandomSanPhamID, @RandomRating, @RandomComment, @RandomDate, @RandomDate);
            
            -- Thêm vào bảng tạm để tránh trùng lặp
            INSERT INTO #ExistingRatings (KhachHangID, SanPhamID)
            VALUES (@RandomKhachHangID, @RandomSanPhamID);
            
            SET @Counter = @Counter + 1;
            
            -- Insert hàng loạt sau mỗi @BatchSize bản ghi
            IF @Counter % @BatchSize = 0
            BEGIN
                INSERT INTO DanhGia (KhachHangID, SanPhamID, Diem, NhanXet, NgayDanhGia, ThoiGianGui)
                SELECT KhachHangID, SanPhamID, Diem, NhanXet, NgayDanhGia, ThoiGianGui FROM #TempRatings;
                
                -- Xóa dữ liệu trong bảng tạm
                TRUNCATE TABLE #TempRatings;
                
                PRINT 'Đã tạo ' + CAST(@Counter AS NVARCHAR(10)) + ' đánh giá...';
                SET @CurrentBatch = @CurrentBatch + 1;
            END
        END
    END

    -- Insert các bản ghi còn lại
    IF EXISTS (SELECT 1 FROM #TempRatings)
    BEGIN
        INSERT INTO DanhGia (KhachHangID, SanPhamID, Diem, NhanXet, NgayDanhGia, ThoiGianGui)
        SELECT KhachHangID, SanPhamID, Diem, NhanXet, NgayDanhGia, ThoiGianGui FROM #TempRatings;
    END

    -- Xóa bảng tạm
    DROP TABLE #TempRatings;
    DROP TABLE #ExistingRatings;
    DROP TABLE #Comments;
    
    -- Cập nhật điểm đánh giá trung bình cho sản phẩm
    UPDATE SanPham
    SET DanhGia = (
        SELECT AVG(Diem) 
        FROM DanhGia 
        WHERE DanhGia.SanPhamID = SanPham.SanPhamID
    )
    WHERE SanPhamID IN (SELECT DISTINCT SanPhamID FROM DanhGia);
    
    COMMIT TRANSACTION;
    PRINT 'Hoàn thành tạo ' + CAST(@RatingCount AS NVARCHAR(10)) + ' đánh giá.';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Lỗi khi tạo đánh giá: ' + ERROR_MESSAGE();
    
    -- Xóa bảng tạm nếu tồn tại
    IF OBJECT_ID('tempdb..#TempRatings') IS NOT NULL
        DROP TABLE #TempRatings;
    IF OBJECT_ID('tempdb..#ExistingRatings') IS NOT NULL
        DROP TABLE #ExistingRatings;
    IF OBJECT_ID('tempdb..#Comments') IS NOT NULL
        DROP TABLE #Comments;
END CATCH
GO

-- Phần 4: Tạo dữ liệu đơn hàng và chi tiết đơn hàng (2,000 đơn hàng)
PRINT 'Bắt đầu tạo đơn hàng và chi tiết đơn hàng...';

DECLARE @OrderCount INT = 20000; -- Số lượng đơn hàng cần tạo
DECLARE @Counter INT = 1;
DECLARE @MaxKhachHangID INT = (SELECT MAX(KhachHangID) FROM KhachHang);
DECLARE @MaxSanPhamID INT = (SELECT MAX(SanPhamID) FROM SanPham);
DECLARE @LastOrderID INT = (SELECT ISNULL(MAX(DonHangID), 0) FROM DonHang);
DECLARE @BatchSize INT = 200; -- Số lượng đơn hàng mỗi lần xử lý
DECLARE @CurrentBatch INT = 1;

BEGIN TRY
    -- Tạo bảng tạm chứa trạng thái đơn hàng
    CREATE TABLE #OrderStatuses (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        Status NVARCHAR(50)
    );

    -- Thêm dữ liệu trạng thái đơn hàng
    INSERT INTO #OrderStatuses (Status) VALUES 
    (N'Completed'), (N'Pending'), (N'Shipping'), (N'Cancelled');

    -- Tạo bảng tạm chứa phương thức thanh toán
    CREATE TABLE #PaymentMethods (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        Method NVARCHAR(50)
    );

    -- Thêm dữ liệu phương thức thanh toán
    INSERT INTO #PaymentMethods (Method) VALUES 
    (N'Credit Card'), (N'Cash on Delivery'), (N'Bank Transfer'), (N'PayPal'), (N'Momo');

    WHILE @Counter <= @OrderCount
    BEGIN
        BEGIN TRANSACTION;
        
        DECLARE @CurrentOrderBatch INT = 0;
        
        WHILE @CurrentOrderBatch < @BatchSize AND @Counter <= @OrderCount
        BEGIN
            -- Tạo ID khách hàng ngẫu nhiên
            DECLARE @RandomKhachHangID INT = FLOOR(RAND() * @MaxKhachHangID) + 1;
            
            -- Tạo ngày đặt hàng ngẫu nhiên trong 1 năm gần đây
            DECLARE @RandomDate DATETIME = DATEADD(DAY, -FLOOR(RAND() * 365), GETDATE());
            
            -- Chọn trạng thái đơn hàng ngẫu nhiên (với trọng số cao hơn cho 'Completed')
            DECLARE @RandomStatusID INT;
            DECLARE @StatusRand FLOAT = RAND();
            
            IF @StatusRand < 0.7 -- 70% là Completed
                SET @RandomStatusID = 1;
            ELSE IF @StatusRand < 0.85 -- 15% là Pending
                SET @RandomStatusID = 2;
            ELSE IF @StatusRand < 0.95 -- 10% là Shipping
                SET @RandomStatusID = 3;
            ELSE -- 5% là Cancelled
                SET @RandomStatusID = 4;
            
            DECLARE @RandomOrderStatus NVARCHAR(50);
            SELECT @RandomOrderStatus = Status FROM #OrderStatuses WHERE ID = @RandomStatusID;
            
            -- Chọn phương thức thanh toán ngẫu nhiên
            DECLARE @RandomPaymentID INT = FLOOR(RAND() * 5) + 1;
            DECLARE @RandomPaymentMethod NVARCHAR(50);
            SELECT @RandomPaymentMethod = Method FROM #PaymentMethods WHERE ID = @RandomPaymentID;
            
            -- Tạo mã đơn hàng
            DECLARE @OrderCode NVARCHAR(20) = 'DH' + RIGHT('000000' + CAST(@LastOrderID + @Counter AS VARCHAR(6)), 6);
            
            -- Lấy địa chỉ khách hàng
            DECLARE @CustomerAddress NVARCHAR(255);
            SELECT @CustomerAddress = DiaChi FROM KhachHang WHERE KhachHangID = @RandomKhachHangID;
            
            -- Thêm đơn hàng
            INSERT INTO DonHang (KhachHangID, NgayDatHang, TrangThaiDonHang, PhuongThucThanhToan, DiaChiGiaoHang, MaDonHang)
            VALUES (@RandomKhachHangID, @RandomDate, @RandomOrderStatus, @RandomPaymentMethod, @CustomerAddress, @OrderCode);
            
            -- Lấy ID đơn hàng vừa thêm
            DECLARE @OrderID INT = SCOPE_IDENTITY();
            
            -- Tạo chi tiết đơn hàng
            -- Số lượng sản phẩm trong đơn hàng (1-5)
            DECLARE @ProductCount INT = FLOOR(RAND() * 5) + 1;
            DECLARE @ProductCounter INT = 1;
            
            -- Bảng tạm để lưu các sản phẩm đã thêm vào đơn hàng
            CREATE TABLE #OrderProducts (SanPhamID INT);
            
            DECLARE @TotalAmount DECIMAL(18, 2) = 0;
            
            WHILE @ProductCounter <= @ProductCount
            BEGIN
                -- Chọn sản phẩm ngẫu nhiên
                DECLARE @RandomSanPhamID INT = FLOOR(RAND() * @MaxSanPhamID) + 1;
                
                -- Kiểm tra xem sản phẩm đã được thêm vào đơn hàng chưa
                IF NOT EXISTS (SELECT 1 FROM #OrderProducts WHERE SanPhamID = @RandomSanPhamID)
                BEGIN
                    -- Số lượng sản phẩm (1-3)
                    DECLARE @RandomQuantity INT = FLOOR(RAND() * 3) + 1;
                    
                    -- Lấy giá sản phẩm
                    DECLARE @ProductPrice DECIMAL(18, 2);
                    SELECT @ProductPrice = Gia FROM SanPham WHERE SanPhamID = @RandomSanPhamID;
                    
                    -- Thêm chi tiết đơn hàng
                    INSERT INTO ChiTietDonHang (DonHangID, SanPhamID, SoLuong, Gia)
                    VALUES (@OrderID, @RandomSanPhamID, @RandomQuantity, @ProductPrice);
                    
                    -- Cập nhật tổng tiền                                                 --================================================================================
                    SET @TotalAmount = @TotalAmount + (@ProductPrice * @RandomQuantity);
                    
                    -- Thêm sản phẩm vào bảng tạm
                    INSERT INTO #OrderProducts (SanPhamID) VALUES (@RandomSanPhamID);
                    
                    SET @ProductCounter = @ProductCounter + 1;
                END
            END
            
            -- Cập nhật tổng tiền cho đơn hàng
            UPDATE DonHang SET TongTien = @TotalAmount WHERE DonHangID = @OrderID;
            
            -- Xóa bảng tạm
            DROP TABLE #OrderProducts;
            
            -- Cập nhật SanPhamThuongMuaCung nếu đơn hàng đã hoàn thành
            IF @RandomOrderStatus = N'Completed'
            BEGIN
                EXEC CapNhatSanPhamThuongMuaCung @OrderID, 1;
            END
            
            SET @Counter = @Counter + 1;
            SET @CurrentOrderBatch = @CurrentOrderBatch + 1;
        END
        
        COMMIT TRANSACTION;
        PRINT 'Đã tạo ' + CAST(@Counter - 1 AS NVARCHAR(10)) + ' đơn hàng...';
        SET @CurrentBatch = @CurrentBatch + 1;
    END

    -- Xóa bảng tạm
    DROP TABLE #OrderStatuses;
    DROP TABLE #PaymentMethods;
    
    PRINT 'Hoàn thành tạo ' + CAST(@OrderCount AS NVARCHAR(10)) + ' đơn hàng.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    PRINT 'Lỗi khi tạo đơn hàng: ' + ERROR_MESSAGE();
    
    -- Xóa bảng tạm nếu tồn tại
    IF OBJECT_ID('tempdb..#OrderStatuses') IS NOT NULL
        DROP TABLE #OrderStatuses;
    IF OBJECT_ID('tempdb..#PaymentMethods') IS NOT NULL
        DROP TABLE #PaymentMethods;
    IF OBJECT_ID('tempdb..#OrderProducts') IS NOT NULL
        DROP TABLE #OrderProducts;
END CATCH
GO


-- Phần 5: Tạo dữ liệu hành vi người dùng (UserBehavior, UserSession, ProductSequence)
PRINT 'Bắt đầu tạo dữ liệu hành vi người dùng...';

-- Tạo bảng UserBehavior nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserBehavior')
BEGIN
    CREATE TABLE UserBehavior (
        BehaviorID INT IDENTITY(1,1) PRIMARY KEY,
        KhachHangID INT,
        SanPhamID INT,
        SessionID VARCHAR(100),
        HanhDong NVARCHAR(50), -- 'View', 'AddToCart', 'Purchase', 'RemoveFromCart', etc.
        ThoiGianBatDau DATETIME,
        ThoiGianKetThuc DATETIME,
        ThoiLuong INT, -- Thời gian (giây) người dùng tương tác với sản phẩm
        TrangThai NVARCHAR(50),
        CONSTRAINT FK_UserBehavior_KhachHang FOREIGN KEY (KhachHangID) REFERENCES KhachHang(KhachHangID),
        CONSTRAINT FK_UserBehavior_SanPham FOREIGN KEY (SanPhamID) REFERENCES SanPham(SanPhamID)
    );
END
GO

-- Tạo bảng UserSession nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'UserSession')
BEGIN
    CREATE TABLE UserSession (
        SessionID VARCHAR(100) PRIMARY KEY,
        KhachHangID INT,
        ThoiGianBatDau DATETIME,
        ThoiGianKetThuc DATETIME,
        UserAgent NVARCHAR(500),
        IPAddress NVARCHAR(50),
        CONSTRAINT FK_UserSession_KhachHang FOREIGN KEY (KhachHangID) REFERENCES KhachHang(KhachHangID)
    );
END
GO

-- Tạo bảng ProductSequence nếu chưa tồn tại
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProductSequence')
BEGIN
    CREATE TABLE ProductSequence (
        SequenceID INT IDENTITY(1,1) PRIMARY KEY,
        SessionID VARCHAR(100),
        SanPhamID INT,
        ThuTu INT,
        ThoiGian DATETIME,
        CONSTRAINT FK_ProductSequence_SanPham FOREIGN KEY (SanPhamID) REFERENCES SanPham(SanPhamID)
    );
END
GO

-- Tạo bảng tạm để lưu trữ các SessionID đã tồn tại
CREATE TABLE #ExistingSessions (
    SessionID VARCHAR(100) PRIMARY KEY
);

-- Chèn dữ liệu hiện có vào bảng tạm
INSERT INTO #ExistingSessions (SessionID)
SELECT SessionID FROM UserSession;

DECLARE @SessionCount INT = 50000; -- Số lượng phiên
DECLARE @Counter INT = 1;
DECLARE @MaxKhachHangID INT = (SELECT MAX(KhachHangID) FROM KhachHang);
DECLARE @MaxSanPhamID INT = (SELECT MAX(SanPhamID) FROM SanPham);
DECLARE @BatchSize INT = 100; -- Giảm kích thước batch
DECLARE @CurrentBatch INT = 1;

BEGIN TRY
    -- Tạo bảng tạm chứa User Agent
    CREATE TABLE #UserAgents (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        Agent NVARCHAR(500)
    );

    -- Thêm dữ liệu User Agent
    INSERT INTO #UserAgents (Agent) VALUES 
    (N'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'),
    (N'Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.1.1 Safari/605.1.15'),
    (N'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:89.0) Gecko/20100101 Firefox/89.0'),
    (N'Mozilla/5.0 (iPhone; CPU iPhone OS 14_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1'),
    (N'Mozilla/5.0 (iPad; CPU OS 14_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1'),
    (N'Mozilla/5.0 (Linux; Android 11; SM-G991B) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.120 Mobile Safari/537.36'),
    (N'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36 Edg/91.0.864.59'),
    (N'Mozilla/5.0 (X11; Ubuntu; Linux x86_64; rv:89.0) Gecko/20100101 Firefox/89.0');

    -- Tạo bảng tạm chứa địa chỉ IP
    CREATE TABLE #IPAddresses (
        ID INT IDENTITY(1,1) PRIMARY KEY,
        IP NVARCHAR(50)
    );

    -- Thêm dữ liệu địa chỉ IP
    INSERT INTO #IPAddresses (IP) VALUES 
    (N'192.168.1.1'), (N'10.0.0.1'), (N'172.16.0.1'), (N'203.162.0.1'), (N'115.73.0.1'),
    (N'27.72.98.55'), (N'113.161.35.43'), (N'171.244.173.2'), (N'103.7.36.245'), (N'14.161.22.33'),
    (N'42.112.213.84'), (N'222.252.30.25'), (N'123.24.206.11'), (N'183.80.130.1'), (N'117.0.37.182');

    WHILE @Counter <= @SessionCount
    BEGIN
        BEGIN TRANSACTION;
        
        DECLARE @CurrentSessionBatch INT = 0;
        
        WHILE @CurrentSessionBatch < @BatchSize AND @Counter <= @SessionCount
        BEGIN
            -- Tạo ID khách hàng ngẫu nhiên
            DECLARE @RandomKhachHangID INT = FLOOR(RAND() * @MaxKhachHangID) + 1;
            
            -- Tạo thời gian bắt đầu ngẫu nhiên trong 90 ngày gần đây
            DECLARE @RandomStartTime DATETIME = DATEADD(MINUTE, -FLOOR(RAND() * 129600), GETDATE()); -- 129600 phút = 90 ngày
            
            -- Tạo thời gian kết thúc (5-120 phút sau thời gian bắt đầu)
            DECLARE @RandomEndTime DATETIME = DATEADD(MINUTE, FLOOR(RAND() * 115) + 5, @RandomStartTime);
            
            -- Tạo SessionID với thêm nhiều yếu tố ngẫu nhiên để đảm bảo tính duy nhất
            DECLARE @SessionID VARCHAR(100);
            DECLARE @SessionExists INT = 1;
            
            -- Lặp cho đến khi tạo được SessionID duy nhất
            WHILE @SessionExists = 1
            BEGIN
                SET @SessionID = 'S' + CAST(@RandomKhachHangID AS VARCHAR(10)) + '_' + 
                                CAST(YEAR(@RandomStartTime) AS VARCHAR(4)) + 
                                RIGHT('0' + CAST(MONTH(@RandomStartTime) AS VARCHAR(2)), 2) + 
                                RIGHT('0' + CAST(DAY(@RandomStartTime) AS VARCHAR(2)), 2) + '_' + 
                                CAST(@Counter AS VARCHAR(10)) + '_' +
                                CAST(CHECKSUM(NEWID()) AS VARCHAR(10));
                
                -- Kiểm tra xem SessionID đã tồn tại chưa
                IF NOT EXISTS (SELECT 1 FROM #ExistingSessions WHERE SessionID = @SessionID)
                    SET @SessionExists = 0;
            END
            
            -- Thêm SessionID vào bảng tạm để tránh trùng lặp
            INSERT INTO #ExistingSessions (SessionID) VALUES (@SessionID);
            
            -- Chọn User Agent và IP ngẫu nhiên
            DECLARE @RandomUserAgentID INT = FLOOR(RAND() * 8) + 1;
            DECLARE @RandomIPID INT = FLOOR(RAND() * 15) + 1;
            DECLARE @UserAgent NVARCHAR(500);
            DECLARE @IP NVARCHAR(50);
            
            SELECT @UserAgent = Agent FROM #UserAgents WHERE ID = @RandomUserAgentID;
            SELECT @IP = IP FROM #IPAddresses WHERE ID = @RandomIPID;
            
            -- Thêm phiên làm việc
            INSERT INTO UserSession (SessionID, KhachHangID, ThoiGianBatDau, ThoiGianKetThuc, UserAgent, IPAddress)
            VALUES (@SessionID, @RandomKhachHangID, @RandomStartTime, @RandomEndTime, @UserAgent, @IP);
            
            -- Tạo dữ liệu hành vi người dùng và trình tự xem sản phẩm
            -- Số lượng sản phẩm xem trong phiên (1-5) - Giảm từ 10 xuống 5
            DECLARE @ViewCount INT = FLOOR(RAND() * 5) + 1;
            DECLARE @ViewCounter INT = 1;
            DECLARE @CurrentTime DATETIME = @RandomStartTime;
            
            -- Bảng tạm để lưu các sản phẩm đã xem trong phiên
            CREATE TABLE #SessionProducts (SanPhamID INT, ThuTu INT);
            
            WHILE @ViewCounter <= @ViewCount
            BEGIN
                -- Chọn sản phẩm ngẫu nhiên
                DECLARE @RandomSanPhamID INT = FLOOR(RAND() * @MaxSanPhamID) + 1;
                
                -- Kiểm tra xem sản phẩm đã được xem trong phiên chưa
                IF NOT EXISTS (SELECT 1 FROM #SessionProducts WHERE SanPhamID = @RandomSanPhamID)
                BEGIN
                    -- Thời gian xem (10-300 giây)
                    DECLARE @ViewDuration INT = FLOOR(RAND() * 290) + 10;
                    
                    -- Cập nhật thời gian hiện tại
                    SET @CurrentTime = DATEADD(SECOND, FLOOR(RAND() * 60) + 10, @CurrentTime); -- 10-70 giây giữa các lần xem
                    
                    -- Thêm vào bảng UserBehavior
                    INSERT INTO UserBehavior (KhachHangID, SanPhamID, SessionID, HanhDong, ThoiGianBatDau, ThoiGianKetThuc, ThoiLuong, TrangThai)
                    VALUES (@RandomKhachHangID, @RandomSanPhamID, @SessionID, 'View', @CurrentTime, DATEADD(SECOND, @ViewDuration, @CurrentTime), @ViewDuration, 'Completed');
                    
                    -- Thêm vào bảng ProductSequence
                    INSERT INTO ProductSequence (SessionID, SanPhamID, ThuTu, ThoiGian)
                    VALUES (@SessionID, @RandomSanPhamID, @ViewCounter, @CurrentTime);
                    
                    -- Thêm sản phẩm vào bảng tạm
                    INSERT INTO #SessionProducts (SanPhamID, ThuTu) VALUES (@RandomSanPhamID, @ViewCounter);
                    
                    -- Có xác suất 30% thêm sản phẩm vào giỏ hàng
                    IF RAND() < 0.3
                    BEGIN
                        SET @CurrentTime = DATEADD(SECOND, FLOOR(RAND() * 30) + 5, @CurrentTime); -- 5-35 giây sau khi xem
                        
                        INSERT INTO UserBehavior (KhachHangID, SanPhamID, SessionID, HanhDong, ThoiGianBatDau, ThoiGianKetThuc, ThoiLuong, TrangThai)
                        VALUES (@RandomKhachHangID, @RandomSanPhamID, @SessionID, 'AddToCart', @CurrentTime, @CurrentTime, 0, 'Completed');
                        
                        -- Có xác suất 50% mua sản phẩm
                        IF RAND() < 0.5
                        BEGIN
                            SET @CurrentTime = DATEADD(SECOND, FLOOR(RAND() * 120) + 30, @CurrentTime); -- 30-150 giây sau khi thêm vào giỏ
                            
                            INSERT INTO UserBehavior (KhachHangID, SanPhamID, SessionID, HanhDong, ThoiGianBatDau, ThoiGianKetThuc, ThoiLuong, TrangThai)
                            VALUES (@RandomKhachHangID, @RandomSanPhamID, @SessionID, 'Purchase', @CurrentTime, @CurrentTime, 0, 'Completed');
                        END
                        -- Có xác suất 20% xóa khỏi giỏ hàng
                        ELSE IF RAND() < 0.2
                        BEGIN
                            SET @CurrentTime = DATEADD(SECOND, FLOOR(RAND() * 60) + 20, @CurrentTime); -- 20-80 giây sau khi thêm vào giỏ
                            
                            INSERT INTO UserBehavior (KhachHangID, SanPhamID, SessionID, HanhDong, ThoiGianBatDau, ThoiGianKetThuc, ThoiLuong, TrangThai)
                            VALUES (@RandomKhachHangID, @RandomSanPhamID, @SessionID, 'RemoveFromCart', @CurrentTime, @CurrentTime, 0, 'Completed');
                        END
                    END
                    
                    SET @ViewCounter = @ViewCounter + 1;
                END
            END
            
            -- Xóa bảng tạm
            DROP TABLE #SessionProducts;
            
            SET @Counter = @Counter + 1;
            SET @CurrentSessionBatch = @CurrentSessionBatch + 1;
        END
        
        COMMIT TRANSACTION;
        PRINT 'Đã tạo ' + CAST(@Counter - 1 AS VARCHAR(10)) + ' phiên làm việc...';
        SET @CurrentBatch = @CurrentBatch + 1;
    END

    -- Xóa bảng tạm
    DROP TABLE #UserAgents;
    DROP TABLE #IPAddresses;
    DROP TABLE #ExistingSessions;
    
    PRINT 'Hoàn thành tạo ' + CAST(@SessionCount AS VARCHAR(10)) + ' phiên làm việc.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    PRINT 'Lỗi khi tạo dữ liệu hành vi người dùng: ' + ERROR_MESSAGE();
    
    -- Xóa bảng tạm nếu tồn tại
    IF OBJECT_ID('tempdb..#UserAgents') IS NOT NULL
        DROP TABLE #UserAgents;
    IF OBJECT_ID('tempdb..#IPAddresses') IS NOT NULL
        DROP TABLE #IPAddresses;
    IF OBJECT_ID('tempdb..#SessionProducts') IS NOT NULL
        DROP TABLE #SessionProducts;
    IF OBJECT_ID('tempdb..#ExistingSessions') IS NOT NULL
        DROP TABLE #ExistingSessions;
END CATCH
GO

-- Phần 6: Tạo dữ liệu sản phẩm yêu thích (1,000 bản ghi)
PRINT 'Bắt đầu tạo dữ liệu sản phẩm yêu thích...';

-- Tạo bảng tạm để lưu trữ các cặp khách hàng-sản phẩm đã tồn tại ----------------------------------------------------------------
CREATE TABLE #ExistingFavorites (
    KhachHangID INT,
    SanPhamID INT
);

-- Chèn dữ liệu hiện có vào bảng tạm
INSERT INTO #ExistingFavorites (KhachHangID, SanPhamID)
SELECT KhachHangID, SanPhamID FROM SanPhamYeuThich;

-- Tạo dữ liệu sản phẩm yêu thích mới
DECLARE @FavoriteCount INT = 10000; -- Số lượng sản phẩm yêu thích cần tạo
DECLARE @Counter INT = 1;
DECLARE @MaxKhachHangID INT = (SELECT MAX(KhachHangID) FROM KhachHang);
DECLARE @MaxSanPhamID INT = (SELECT MAX(SanPhamID) FROM SanPham);
DECLARE @BatchSize INT = 200; -- Số lượng bản ghi mỗi lần insert
DECLARE @CurrentBatch INT = 1;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Tạo bảng tạm để lưu trữ dữ liệu trước khi insert hàng loạt
    CREATE TABLE #TempFavorites (
        KhachHangID INT,
        SanPhamID INT,
        NgayThem DATETIME
    );

    WHILE @Counter <= @FavoriteCount
    BEGIN
        -- Tạo ID khách hàng và sản phẩm ngẫu nhiên
        DECLARE @RandomKhachHangID INT = FLOOR(RAND() * @MaxKhachHangID) + 1;
        DECLARE @RandomSanPhamID INT = FLOOR(RAND() * @MaxSanPhamID) + 1;
        
        -- Kiểm tra xem cặp khách hàng-sản phẩm đã tồn tại chưa
        IF NOT EXISTS (SELECT 1 FROM #ExistingFavorites 
                      WHERE KhachHangID = @RandomKhachHangID AND SanPhamID = @RandomSanPhamID)
        BEGIN
            -- Tạo ngày thêm ngẫu nhiên trong 1 năm gần đây
            DECLARE @RandomDate DATETIME = DATEADD(DAY, -FLOOR(RAND() * 365), GETDATE());
            
            -- Thêm vào bảng tạm
            INSERT INTO #TempFavorites (KhachHangID, SanPhamID, NgayThem)
            VALUES (@RandomKhachHangID, @RandomSanPhamID, @RandomDate);
            
            -- Thêm vào bảng tạm để tránh trùng lặp
            INSERT INTO #ExistingFavorites (KhachHangID, SanPhamID)
            VALUES (@RandomKhachHangID, @RandomSanPhamID);
            
            SET @Counter = @Counter + 1;
            
            -- Insert hàng loạt sau mỗi @BatchSize bản ghi
            IF @Counter % @BatchSize = 0
            BEGIN
                INSERT INTO SanPhamYeuThich (KhachHangID, SanPhamID, NgayThem)
                SELECT KhachHangID, SanPhamID, NgayThem FROM #TempFavorites;
                
                -- Xóa dữ liệu trong bảng tạm
                TRUNCATE TABLE #TempFavorites;
                
                PRINT 'Đã tạo ' + CAST(@Counter AS NVARCHAR(10)) + ' sản phẩm yêu thích...';
                SET @CurrentBatch = @CurrentBatch + 1;
            END
        END
    END

    -- Insert các bản ghi còn lại
    IF EXISTS (SELECT 1 FROM #TempFavorites)
    BEGIN
        INSERT INTO SanPhamYeuThich (KhachHangID, SanPhamID, NgayThem)
        SELECT KhachHangID, SanPhamID, NgayThem FROM #TempFavorites;
    END

    -- Xóa bảng tạm
    DROP TABLE #TempFavorites;
    DROP TABLE #ExistingFavorites;
    
    COMMIT TRANSACTION;
    PRINT 'Hoàn thành tạo ' + CAST(@FavoriteCount AS NVARCHAR(10)) + ' sản phẩm yêu thích.';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'Lỗi khi tạo sản phẩm yêu thích: ' + ERROR_MESSAGE();
    
    -- Xóa bảng tạm nếu tồn tại
    IF OBJECT_ID('tempdb..#TempFavorites') IS NOT NULL
        DROP TABLE #TempFavorites;
    IF OBJECT_ID('tempdb..#ExistingFavorites') IS NOT NULL
        DROP TABLE #ExistingFavorites;
END CATCH
GO


------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

-- Phần 7: Cập nhật các chỉ số Apriori (Support, Confidence, Lift) cho SanPhamThuongMuaCung
PRINT 'Bắt đầu cập nhật các chỉ số Apriori...';

-- Thêm cột Confidence, Lift, Support vào bảng SanPhamThuongMuaCung nếu chưa có
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SanPhamThuongMuaCung') AND name = 'Confidence')
BEGIN
    ALTER TABLE SanPhamThuongMuaCung ADD Confidence FLOAT NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SanPhamThuongMuaCung') AND name = 'Lift')
BEGIN
    ALTER TABLE SanPhamThuongMuaCung ADD Lift FLOAT NULL;
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('SanPhamThuongMuaCung') AND name = 'Support')
BEGIN
    ALTER TABLE SanPhamThuongMuaCung ADD Support FLOAT NULL;
END
GO

-- Thêm cột DaLoc và NgayLoc vào bảng SanPhamThuongMuaCung nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SanPhamThuongMuaCung]') AND name = 'DaLoc')
BEGIN
    ALTER TABLE [dbo].[SanPhamThuongMuaCung] ADD DaLoc BIT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SanPhamThuongMuaCung]') AND name = 'NgayLoc')
BEGIN
    ALTER TABLE [dbo].[SanPhamThuongMuaCung] ADD NgayLoc DATETIME NULL;
END	



-- Khai báo tham số ngưỡng (có thể truyền vào stored procedure)
DECLARE @MinSupport FLOAT = 0.01; 
DECLARE @MinConfidence FLOAT = 0.01;
DECLARE @MinLift FLOAT = 0.5; 

-- Đếm tổng số đơn hàng đã hoàn thành
DECLARE @TotalOrders INT = (SELECT COUNT(DISTINCT DonHangID) FROM DonHang WHERE TrangThaiDonHang = N'Completed');

-- Tạo bảng tạm chứa số lần xuất hiện của từng sản phẩm
CREATE TABLE #ItemCounts (
    SanPhamID INT,
    OrderCount INT
);

INSERT INTO #ItemCounts
SELECT SanPhamID, COUNT(DISTINCT DonHangID) AS OrderCount
FROM ChiTietDonHang
WHERE DonHangID IN (SELECT DonHangID FROM DonHang WHERE TrangThaiDonHang = N'Completed')
GROUP BY SanPhamID;

-- Cập nhật Support, Confidence, Lift với điều kiện lọc
UPDATE sptmc
SET 
    Support = CAST(sptmc.SoLanMuaCung AS FLOAT) / @TotalOrders,
    Confidence = CAST(sptmc.SoLanMuaCung AS FLOAT) / ic1.OrderCount,
    Lift = (CAST(sptmc.SoLanMuaCung AS FLOAT) / @TotalOrders) / (
        (ic1.OrderCount / CAST(@TotalOrders AS FLOAT)) * (ic2.OrderCount / CAST(@TotalOrders AS FLOAT))
    )
FROM SanPhamThuongMuaCung sptmc
JOIN #ItemCounts ic1 ON sptmc.SanPhamID1 = ic1.SanPhamID
JOIN #ItemCounts ic2 ON sptmc.SanPhamID2 = ic2.SanPhamID
WHERE 
    CAST(sptmc.SoLanMuaCung AS FLOAT) / @TotalOrders >= @MinSupport
    AND CAST(sptmc.SoLanMuaCung AS FLOAT) / ic1.OrderCount >= @MinConfidence
    AND (CAST(sptmc.SoLanMuaCung AS FLOAT) / @TotalOrders) / (
        (ic1.OrderCount / CAST(@TotalOrders AS FLOAT)) * (ic2.OrderCount / CAST(@TotalOrders AS FLOAT))
    ) >= @MinLift;

-- Xóa các bản ghi không thỏa mãn ngưỡng sau khi cập nhật
DELETE FROM SanPhamThuongMuaCung
WHERE Support IS NULL OR Confidence IS NULL OR Lift IS NULL
   OR Support < @MinSupport
   OR Confidence < @MinConfidence
   OR Lift < @MinLift;

-- Xóa bảng tạm
DROP TABLE #ItemCounts;

PRINT 'Hoàn thành cập nhật các chỉ số Apriori.';
GO
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

-- Phần 8: Hiển thị thống kê dữ liệu đã tạo
PRINT 'Hiển thị thống kê dữ liệu đã tạo:';

SELECT 'KhachHang' AS TableName, COUNT(*) AS RecordCount FROM KhachHang
UNION ALL
SELECT 'UserViews', COUNT(*) FROM UserViews
UNION ALL
SELECT 'DanhGia', COUNT(*) FROM DanhGia
UNION ALL
SELECT 'DonHang', COUNT(*) FROM DonHang
UNION ALL
SELECT 'ChiTietDonHang', COUNT(*) FROM ChiTietDonHang
UNION ALL
SELECT 'SanPhamThuongMuaCung', COUNT(*) FROM SanPhamThuongMuaCung
UNION ALL
--SELECT 'UserBehavior', COUNT(*) FROM UserBehavior
--UNION ALL
--SELECT 'UserSession', COUNT(*) FROM UserSession
--UNION ALL
--SELECT 'ProductSequence', COUNT(*) FROM ProductSequence
--UNION ALL
SELECT 'SanPhamYeuThich', COUNT(*) FROM SanPhamYeuThich;
GO

PRINT 'Hoàn thành tạo dữ liệu mẫu lớn cho CuaHangMayTinh2.';
GO



--------------------------------------------------------------------------------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------------
-- Tạo bảng SanPhamGoiYTheoDanhGia để lưu trữ các gợi ý sản phẩm
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SanPhamGoiYTheoDanhGia')
BEGIN
    CREATE TABLE SanPhamGoiYTheoDanhGia (
        GoiYID INT IDENTITY(1,1) PRIMARY KEY,
        SanPhamIDGoc INT NOT NULL,
        SanPhamIDGoiY INT NOT NULL,
        ItemsetSize INT NOT NULL, -- Số hạng mục trong itemset
        Support FLOAT NOT NULL,
        Confidence FLOAT NOT NULL,
        Lift FLOAT NOT NULL,
        DiemDanhGia FLOAT NULL,
        LuotXem INT NULL,
        DiemPhoBien FLOAT NULL, -- Điểm tổng hợp dựa trên đánh giá và lượt xem
        NgayCapNhat DATETIME DEFAULT GETDATE(),
        CONSTRAINT FK_SanPhamGoiYTheoDanhGia_SanPhamGoc FOREIGN KEY (SanPhamIDGoc) REFERENCES SanPham(SanPhamID),
        CONSTRAINT FK_SanPhamGoiYTheoDanhGia_SanPhamGoiY FOREIGN KEY (SanPhamIDGoiY) REFERENCES SanPham(SanPhamID),
        CONSTRAINT UQ_SanPhamGoiYTheoDanhGia UNIQUE (SanPhamIDGoc, SanPhamIDGoiY, ItemsetSize)
    );
END
GO
--------------------------------------------------------------------------------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------------
--------------------------------------------------------------------------------------------------------------------------------
-- Tạo stored procedure để cập nhật gợi ý sản phẩm dựa trên đánh giá và mức độ phổ biến
CREATE OR ALTER PROCEDURE sp_CapNhatGoiYTheoDanhGia
    @MinSupport FLOAT = 0.001,
    @MinConfidence FLOAT = 0.02,
    @MinLift FLOAT = 0.5,
    @MinDanhGia FLOAT = 4.5,
    @MinLuotXem INT = 100,
    @MaxItemsetSize INT = 3,
    @TrongSoDanhGia FLOAT = 0.7,
    @TrongSoLuotXem FLOAT = 0.3
AS
BEGIN
    SET NOCOUNT ON;
    
    PRINT N'Bắt đầu cập nhật gợi ý sản phẩm dựa trên Apriori...';

    -- Kiểm tra số đơn hàng
    DECLARE @TotalOrders INT = (SELECT COUNT(DISTINCT DonHangID) FROM DonHang WHERE TrangThaiDonHang = N'Completed');
    IF @TotalOrders = 0
    BEGIN
        RAISERROR(N'Không có đơn hàng hoàn thành để phân tích.', 16, 1);
        RETURN;
    END

    -- Xóa dữ liệu cũ
    TRUNCATE TABLE SanPhamGoiYTheoDanhGia;
    PRINT N'Đã xóa dữ liệu cũ trong bảng SanPhamGoiYTheoDanhGia.';

    -- Tạo bảng tạm chứa thông tin sản phẩm đủ điều kiện
    CREATE TABLE #QualifiedProducts (
        SanPhamID INT PRIMARY KEY,
        DanhGia FLOAT,
        LuotXem INT,
        DanhMucID INT, -- Thêm cột DanhMucID
        OrderCount INT,
        IsFrequent BIT DEFAULT 0
    );

    -- Chèn sản phẩm thỏa mãn điều kiện
    INSERT INTO #QualifiedProducts (SanPhamID, DanhGia, LuotXem, DanhMucID, OrderCount)
    SELECT 
        sp.SanPhamID,
        sp.DanhGia,
        sp.LuotXem,
        sp.DanhMucID,
        COUNT(DISTINCT ctdh.DonHangID) AS OrderCount
    FROM 
        SanPham sp
        LEFT JOIN ChiTietDonHang ctdh ON sp.SanPhamID = ctdh.SanPhamID
        LEFT JOIN DonHang dh ON ctdh.DonHangID = dh.DonHangID AND dh.TrangThaiDonHang = N'Completed'
    WHERE 
        sp.DanhGia >= @MinDanhGia
        AND sp.LuotXem >= @MinLuotXem
    GROUP BY 
        sp.SanPhamID, sp.DanhGia, sp.LuotXem, sp.DanhMucID;

    -- Đánh dấu các sản phẩm frequent (k = 1)
    UPDATE #QualifiedProducts
    SET IsFrequent = 1
    WHERE OrderCount / CAST(@TotalOrders AS FLOAT) >= @MinSupport;

    -- Tạo bảng tạm lưu frequent itemsets
    CREATE TABLE #FrequentItemsets (
        ItemsetIDs NVARCHAR(1000), -- Danh sách ID cách nhau bằng dấu phẩy
        ItemCount INT,
        Frequency INT,
        Support FLOAT
    );

    -- Bắt đầu với k = 1
    INSERT INTO #FrequentItemsets (ItemsetIDs, ItemCount, Frequency, Support)
    SELECT 
        CAST(qp.SanPhamID AS NVARCHAR(10)),
        1,
        qp.OrderCount,
        CAST(qp.OrderCount AS FLOAT) / @TotalOrders
    FROM #QualifiedProducts qp
    WHERE qp.IsFrequent = 1;

    -- Lặp để tìm frequent itemsets với k > 1
    DECLARE @k INT = 2;
    WHILE @k <= @MaxItemsetSize
    BEGIN
        PRINT N'Tìm Frequent Itemsets cho k = ' + CAST(@k AS NVARCHAR(2)) + '...';

        -- Tạo ứng viên (candidate itemsets)
        CREATE TABLE #Candidates (
            ItemsetIDs NVARCHAR(1000),
            ItemCount INT
        );

        IF @k = 2
        BEGIN
            INSERT INTO #Candidates (ItemsetIDs, ItemCount)
            SELECT 
                CAST(f1.SanPhamID AS NVARCHAR(10)) + ',' + CAST(f2.SanPhamID AS NVARCHAR(10)),
                2
            FROM #QualifiedProducts f1
            CROSS JOIN #QualifiedProducts f2
            WHERE f1.SanPhamID < f2.SanPhamID
            AND f1.IsFrequent = 1 AND f2.IsFrequent = 1;
        END
        ELSE
        BEGIN
            INSERT INTO #Candidates (ItemsetIDs, ItemCount)
            SELECT 
                f1.ItemsetIDs + ',' + 
                SUBSTRING(f2.ItemsetIDs, 
                    CASE 
                        WHEN CHARINDEX(',', f2.ItemsetIDs, CHARINDEX(',', f2.ItemsetIDs, 1) + 1) = 0 
                        THEN LEN(f2.ItemsetIDs) + 1
                        ELSE CHARINDEX(',', f2.ItemsetIDs, CHARINDEX(',', f2.ItemsetIDs, 1) + 1) + 1 
                    END, 
                    LEN(f2.ItemsetIDs)),
                @k
            FROM #FrequentItemsets f1
            JOIN #FrequentItemsets f2 ON f1.ItemCount = @k - 1 AND f2.ItemCount = @k - 1
            WHERE 
                (LEN(f1.ItemsetIDs) - LEN(REPLACE(f1.ItemsetIDs, ',', ''))) >= (@k - 2)
                AND (LEN(f2.ItemsetIDs) - LEN(REPLACE(f2.ItemsetIDs, ',', ''))) >= (@k - 2)
                AND LEFT(f1.ItemsetIDs, 
                    CASE 
                        WHEN CHARINDEX(',', f1.ItemsetIDs, CHARINDEX(',', f1.ItemsetIDs, 1) + 1) = 0 
                        THEN LEN(f1.ItemsetIDs) 
                        ELSE CHARINDEX(',', f1.ItemsetIDs, CHARINDEX(',', f1.ItemsetIDs, 1) + 1) - 1 
                    END) = 
                LEFT(f2.ItemsetIDs, 
                    CASE 
                        WHEN CHARINDEX(',', f2.ItemsetIDs, CHARINDEX(',', f2.ItemsetIDs, 1) + 1) = 0 
                        THEN LEN(f2.ItemsetIDs) 
                        ELSE CHARINDEX(',', f2.ItemsetIDs, CHARINDEX(',', f2.ItemsetIDs, 1) + 1) - 1 
                    END)
                AND RIGHT(f1.ItemsetIDs, 
                    CASE 
                        WHEN CHARINDEX(',', f1.ItemsetIDs, 1) = 0 
                        THEN LEN(f1.ItemsetIDs) 
                        ELSE LEN(f1.ItemsetIDs) - CHARINDEX(',', f1.ItemsetIDs, 1) 
                    END) < 
                RIGHT(f2.ItemsetIDs, 
                    CASE 
                        WHEN CHARINDEX(',', f2.ItemsetIDs, 1) = 0 
                        THEN LEN(f2.ItemsetIDs) 
                        ELSE LEN(f2.ItemsetIDs) - CHARINDEX(',', f2.ItemsetIDs, 1) 
                    END);
        END

        -- Tính frequency cho các ứng viên
        INSERT INTO #FrequentItemsets (ItemsetIDs, ItemCount, Frequency, Support)
        SELECT 
            c.ItemsetIDs,
            c.ItemCount,
            COUNT(DISTINCT t.DonHangID) AS Frequency,
            CAST(COUNT(DISTINCT t.DonHangID) AS FLOAT) / @TotalOrders AS Support
        FROM #Candidates c
        CROSS APPLY (
            SELECT t.DonHangID
            FROM ChiTietDonHang t
            JOIN DonHang dh ON t.DonHangID = dh.DonHangID AND dh.TrangThaiDonHang = N'Completed'
            WHERE t.SanPhamID IN (
                SELECT CAST(s.value AS INT)
                FROM STRING_SPLIT(c.ItemsetIDs, ',') s
                WHERE CAST(s.value AS INT) IN (SELECT SanPhamID FROM #QualifiedProducts)
            )
            GROUP BY t.DonHangID
            HAVING COUNT(DISTINCT t.SanPhamID) = (SELECT COUNT(*) FROM STRING_SPLIT(c.ItemsetIDs, ','))
        ) t
        GROUP BY c.ItemsetIDs, c.ItemCount
        HAVING CAST(COUNT(DISTINCT t.DonHangID) AS FLOAT) / @TotalOrders >= @MinSupport;

        DROP TABLE #Candidates;
        SET @k = @k + 1;
    END

    -- Tạo bảng tạm để lưu các luật kết hợp
    CREATE TABLE #AssociationRules (
        ItemsetIDs NVARCHAR(1000),
        ItemCount INT,
        SanPhamIDGoc INT,
        SanPhamIDGoiY INT,
        Support FLOAT,
        Confidence FLOAT,
        Lift FLOAT
    );

    -- Tạo luật kết hợp từ frequent itemsets (chỉ xét k = 2 cho đơn giản)
    INSERT INTO #AssociationRules (ItemsetIDs, ItemCount, SanPhamIDGoc, SanPhamIDGoiY, Support, Confidence, Lift)
    SELECT 
        fi.ItemsetIDs,
        fi.ItemCount,
        CAST(PARSENAME(REPLACE(SUBSTRING(fi.ItemsetIDs, 1, CHARINDEX(',', fi.ItemsetIDs) - 1), ',', '.'), 1) AS INT) AS SanPhamIDGoc,
        CAST(PARSENAME(REPLACE(SUBSTRING(fi.ItemsetIDs, CHARINDEX(',', fi.ItemsetIDs) + 1, LEN(fi.ItemsetIDs)), ',', '.'), 1) AS INT) AS SanPhamIDGoiY,
        fi.Support,
        CAST(fi.Frequency AS FLOAT) / (SELECT OrderCount FROM #QualifiedProducts WHERE SanPhamID = CAST(PARSENAME(REPLACE(SUBSTRING(fi.ItemsetIDs, 1, CHARINDEX(',', fi.ItemsetIDs) - 1), ',', '.'), 1) AS INT)) AS Confidence,
        fi.Support / (
            (SELECT OrderCount FROM #QualifiedProducts WHERE SanPhamID = CAST(PARSENAME(REPLACE(SUBSTRING(fi.ItemsetIDs, 1, CHARINDEX(',', fi.ItemsetIDs) - 1), ',', '.'), 1) AS INT)) / CAST(@TotalOrders AS FLOAT) * 
            (SELECT OrderCount FROM #QualifiedProducts WHERE SanPhamID = CAST(PARSENAME(REPLACE(SUBSTRING(fi.ItemsetIDs, CHARINDEX(',', fi.ItemsetIDs) + 1, LEN(fi.ItemsetIDs)), ',', '.'), 1) AS INT)) / CAST(@TotalOrders AS FLOAT)
        ) AS Lift
    FROM #FrequentItemsets fi
    WHERE fi.ItemCount = 2
    AND CHARINDEX(',', fi.ItemsetIDs) > 0;

    -- Chèn vào bảng gợi ý
    INSERT INTO SanPhamGoiYTheoDanhGia (
        SanPhamIDGoc, SanPhamIDGoiY, ItemsetSize, Support, Confidence, Lift, DiemDanhGia, LuotXem, DiemPhoBien, NgayCapNhat
    )
    SELECT 
        ar.SanPhamIDGoc,
        ar.SanPhamIDGoiY,
        ar.ItemCount,
        ar.Support,
        ar.Confidence,
        ar.Lift,
        qp.DanhGia,
        qp.LuotXem,
        (@TrongSoDanhGia * (qp.DanhGia / (SELECT MAX(DanhGia) FROM #QualifiedProducts)) + 
         @TrongSoLuotXem * (CAST(qp.LuotXem AS FLOAT) / (SELECT MAX(LuotXem) FROM #QualifiedProducts))) * 10,
        GETDATE()
    FROM #AssociationRules ar
    JOIN #QualifiedProducts qp ON ar.SanPhamIDGoiY = qp.SanPhamID
    WHERE ar.Lift >= @MinLift AND ar.Confidence >= @MinConfidence;

    -- Thêm gợi ý fallback dựa trên danh mục (không sử dụng Apriori)
    INSERT INTO SanPhamGoiYTheoDanhGia (
        SanPhamIDGoc, SanPhamIDGoiY, ItemsetSize, Support, Confidence, Lift, DiemDanhGia, LuotXem, DiemPhoBien, NgayCapNhat
    )
    SELECT 
        a.SanPhamID,
        b.SanPhamID,
        1,
        0,
        0,
        0,
        b.DanhGia,
        b.LuotXem,
        (@TrongSoDanhGia * (b.DanhGia / (SELECT MAX(DanhGia) FROM #QualifiedProducts)) + 
         @TrongSoLuotXem * (CAST(b.LuotXem AS FLOAT) / (SELECT MAX(LuotXem) FROM #QualifiedProducts))) * 10,
        GETDATE()
    FROM #QualifiedProducts a
    JOIN #QualifiedProducts b ON a.DanhMucID = b.DanhMucID
        AND a.SanPhamID <> b.SanPhamID
    WHERE NOT EXISTS (
        SELECT 1 FROM SanPhamGoiYTheoDanhGia g
        WHERE g.SanPhamIDGoc = a.SanPhamID AND g.SanPhamIDGoiY = b.SanPhamID
    )
    ORDER BY b.DanhGia DESC, b.LuotXem DESC;

    -- Xóa bảng tạm
    DROP TABLE #QualifiedProducts;
    DROP TABLE #FrequentItemsets;
    DROP TABLE #AssociationRules;

    -- Trả về số lượng gợi ý
    DECLARE @CountRecommendations INT = (SELECT COUNT(*) FROM SanPhamGoiYTheoDanhGia);
    PRINT N'Hoàn thành cập nhật gợi ý sản phẩm. Đã tạo ' + CAST(@CountRecommendations AS NVARCHAR(10)) + N' gợi ý.';

    RETURN @CountRecommendations;
END
GO

-- Tạo stored procedure để lấy gợi ý sản phẩm theo đánh giá
CREATE OR ALTER PROCEDURE sp_LayGoiYTheoDanhGia
    @SanPhamID INT,
    @SoLuong INT = 10,
    @DanhMucID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Lấy danh sách sản phẩm gợi ý, ưu tiên itemset lớn hơn
    SELECT TOP (@SoLuong)
        g.SanPhamIDGoc,
        g.SanPhamIDGoiY,
        sp.TenSP AS SanPhamGoiY,
        sp.Gia,
        sp.DanhGia,
        sp.LuotXem,
        g.Support,
        g.Confidence,
        g.Lift,
        g.DiemPhoBien,
        (SELECT TOP 1 DuongDanAnh FROM HinhAnhSanPham WHERE SanPhamID = sp.SanPhamID AND LaHinhChinh = 1) AS HinhAnh
    FROM SanPhamGoiYTheoDanhGia g
    JOIN SanPham sp ON g.SanPhamIDGoiY = sp.SanPhamID
    WHERE g.SanPhamIDGoc = @SanPhamID
        AND (@DanhMucID IS NULL OR sp.DanhMucID = @DanhMucID)
        AND g.Support > 0 -- Loại bỏ gợi ý fallback
    ORDER BY 
        g.ItemsetSize DESC,
        g.DiemPhoBien DESC;

    -- Nếu không có gợi ý từ Apriori, sử dụng fallback
    IF @@ROWCOUNT = 0
    BEGIN
        SELECT TOP (@SoLuong)
            g.SanPhamIDGoc,
            g.SanPhamIDGoiY,
            sp.TenSP AS SanPhamGoiY,
            sp.Gia,
            sp.DanhGia,
            sp.LuotXem,
            g.Support,
            g.Confidence,
            g.Lift,
            g.DiemPhoBien,
            (SELECT TOP 1 DuongDanAnh FROM HinhAnhSanPham WHERE SanPhamID = sp.SanPhamID AND LaHinhChinh = 1) AS HinhAnh
        FROM SanPhamGoiYTheoDanhGia g
        JOIN SanPham sp ON g.SanPhamIDGoiY = sp.SanPhamID
        WHERE g.SanPhamIDGoc = @SanPhamID
            AND (@DanhMucID IS NULL OR sp.DanhMucID = @DanhMucID)
        ORDER BY g.DiemPhoBien DESC;
    END
END
GO





-- Thêm cột DaLoc và NgayLoc vào bảng SanPhamGoiYTheoDanhGia nếu chưa có
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SanPhamGoiYTheoDanhGia]') AND name = 'DaLoc')
BEGIN
    ALTER TABLE [dbo].[SanPhamGoiYTheoDanhGia] ADD DaLoc BIT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[SanPhamGoiYTheoDanhGia]') AND name = 'NgayLoc')
BEGIN
    ALTER TABLE [dbo].[SanPhamGoiYTheoDanhGia] ADD NgayLoc DATETIME NULL;
END	
GO


-- Thực thi stored procedure để cập nhật gợi ý sản phẩm
EXEC sp_CapNhatGoiYTheoDanhGia 
    @MinSupport = 0.001, 
    @MinConfidence = 0.002, 
    @MinLift = 0.05, 
    @MinDanhGia = 4.0, 
    @MinLuotXem = 100,
    @MaxItemsetSize = 3,
    @TrongSoDanhGia = 0.7,
    @TrongSoLuotXem = 0.3;
GO









--------------------------------------------------------------------------------
-- Thống kê tổng quan về khách hàng
SELECT 
    COUNT(*) AS TongSoKhachHang,
    SUM(CASE WHEN LaAdmin = 1 THEN 1 ELSE 0 END) AS SoLuongAdmin,
    SUM(CASE WHEN LaAdmin = 0 THEN 1 ELSE 0 END) AS SoLuongKhachHang,
    COUNT(DISTINCT DiaChi) AS SoLuongDiaChi
FROM KhachHang;



-- Phân tích phân bố địa lý của khách hàng
SELECT 
    SUBSTRING(DiaChi, CHARINDEX(N', ', DiaChi) + 1, LEN(DiaChi)) AS TinhThanh,
    COUNT(*) AS SoLuongKhachHang,
    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM KhachHang) AS DECIMAL(5,2)) AS PhanTram
FROM KhachHang
GROUP BY SUBSTRING(DiaChi, CHARINDEX(N', ', DiaChi) + 1, LEN(DiaChi))
ORDER BY SoLuongKhachHang DESC;


-- Top 10 sản phẩm được xem nhiều nhất
SELECT TOP 10
    sp.SanPhamID,
    sp.TenSP,
    sp.LuotXem,
    dm.TenDanhMuc,
    sp.Gia,
    sp.DanhGia
FROM SanPham sp
JOIN DanhMuc dm ON sp.DanhMucID = dm.DanhMucID
ORDER BY sp.LuotXem DESC;



-- Phân tích thời gian xem sản phẩm theo giờ trong ngày
SELECT 
    DATEPART(HOUR, NgayXem) AS Gio,
    COUNT(*) AS SoLuotXem,
    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM UserViews) AS DECIMAL(5,2)) AS PhanTram
FROM UserViews
GROUP BY DATEPART(HOUR, NgayXem)
ORDER BY Gio;


-- Phân tích số lượng sản phẩm xem trong một phiên
SELECT 
    SessionCount,
    COUNT(*) AS SoLuongPhien,
    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM UserSession) AS DECIMAL(5,2)) AS PhanTram
FROM (
    SELECT 
        us.SessionID,
        COUNT(DISTINCT ps.SanPhamID) AS SessionCount
    FROM UserSession us
    LEFT JOIN ProductSequence ps ON us.SessionID = ps.SessionID
    GROUP BY us.SessionID
) AS SessionCounts
GROUP BY SessionCount
ORDER BY SessionCount;


-- Tỷ lệ chuyển đổi từ xem sang mua theo danh mục sản phẩm
SELECT 
    dm.DanhMucID,
    dm.TenDanhMuc,
    COUNT(DISTINCT uv.KhachHangID) AS SoNguoiXem,
    COUNT(DISTINCT dh.KhachHangID) AS SoNguoiMua,
    CASE 
        WHEN COUNT(DISTINCT uv.KhachHangID) > 0 
        THEN CAST(COUNT(DISTINCT dh.KhachHangID) * 100.0 / COUNT(DISTINCT uv.KhachHangID) AS DECIMAL(5,2))
        ELSE 0 
    END AS TyLeChuyenDoi
FROM DanhMuc dm
JOIN SanPham sp ON dm.DanhMucID = sp.DanhMucID
LEFT JOIN UserViews uv ON sp.SanPhamID = uv.SanPhamID
LEFT JOIN ChiTietDonHang ctdh ON sp.SanPhamID = ctdh.SanPhamID
LEFT JOIN DonHang dh ON ctdh.DonHangID = dh.DonHangID AND dh.TrangThaiDonHang = N'Completed'
GROUP BY dm.DanhMucID, dm.TenDanhMuc
ORDER BY TyLeChuyenDoi DESC;







-- Phân tích giá trị đơn hàng trung bình theo tháng
SELECT 
    YEAR(NgayDatHang) AS Nam,
    MONTH(NgayDatHang) AS Thang,
    COUNT(*) AS SoDonHang,
    FORMAT(AVG(TongTien), 'N0') AS GiaTriTrungBinh,
    FORMAT(MIN(TongTien), 'N0') AS GiaTriThapNhat,
    FORMAT(MAX(TongTien), 'N0') AS GiaTriCaoNhat
FROM DonHang
WHERE TrangThaiDonHang = N'Completed'
GROUP BY YEAR(NgayDatHang), MONTH(NgayDatHang)
ORDER BY Nam, Thang;






-- Phân tích tần suất mua hàng của khách hàng
WITH CustomerOrders AS (
    SELECT 
        KhachHangID,
        COUNT(*) AS SoDonHang,
        MIN(NgayDatHang) AS NgayDatHangDauTien,
        MAX(NgayDatHang) AS NgayDatHangGanNhat,
        DATEDIFF(DAY, MIN(NgayDatHang), MAX(NgayDatHang)) AS SoNgayMuaHang
    FROM DonHang
    WHERE TrangThaiDonHang = N'Completed'
    GROUP BY KhachHangID
    HAVING COUNT(*) > 1
)
SELECT 
    CASE 
        WHEN SoNgayMuaHang / (SoDonHang - 1) <= 30 THEN N'Thường xuyên (< 30 ngày)'
        WHEN SoNgayMuaHang / (SoDonHang - 1) <= 90 THEN N'Định kỳ (30-90 ngày)'
        WHEN SoNgayMuaHang / (SoDonHang - 1) <= 180 THEN N'Thỉnh thoảng (90-180 ngày)'
        ELSE N'Hiếm khi (> 180 ngày)'
    END AS TanSuatMuaHang,
    COUNT(*) AS SoLuongKhachHang,
    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM CustomerOrders) AS DECIMAL(5,2)) AS PhanTram,
    AVG(SoDonHang) AS SoDonHangTrungBinh
FROM CustomerOrders
WHERE SoNgayMuaHang > 0 AND SoDonHang > 1
GROUP BY 
    CASE 
        WHEN SoNgayMuaHang / (SoDonHang - 1) <= 30 THEN N'Thường xuyên (< 30 ngày)'
        WHEN SoNgayMuaHang / (SoDonHang - 1) <= 90 THEN N'Định kỳ (30-90 ngày)'
        WHEN SoNgayMuaHang / (SoDonHang - 1) <= 180 THEN N'Thỉnh thoảng (90-180 ngày)'
        ELSE N'Hiếm khi (> 180 ngày)'
    END
ORDER BY 
    CASE 
        WHEN CASE 
                WHEN SoNgayMuaHang / (SoDonHang - 1) <= 30 THEN N'Thường xuyên (< 30 ngày)'
                WHEN SoNgayMuaHang / (SoDonHang - 1) <= 90 THEN N'Định kỳ (30-90 ngày)'
                WHEN SoNgayMuaHang / (SoDonHang - 1) <= 180 THEN N'Thỉnh thoảng (90-180 ngày)'
                ELSE N'Hiếm khi (> 180 ngày)'
             END = N'Thường xuyên (< 30 ngày)' THEN 1
        WHEN CASE 
                WHEN SoNgayMuaHang / (SoDonHang - 1) <= 30 THEN N'Thường xuyên (< 30 ngày)'
                WHEN SoNgayMuaHang / (SoDonHang - 1) <= 90 THEN N'Định kỳ (30-90 ngày)'
                WHEN SoNgayMuaHang / (SoDonHang - 1) <= 180 THEN N'Thỉnh thoảng (90-180 ngày)'
                ELSE N'Hiếm khi (> 180 ngày)'
             END = N'Định kỳ (30-90 ngày)' THEN 2
        WHEN CASE 
                WHEN SoNgayMuaHang / (SoDonHang - 1) <= 30 THEN N'Thường xuyên (< 30 ngày)'
                WHEN SoNgayMuaHang / (SoDonHang - 1) <= 90 THEN N'Định kỳ (30-90 ngày)'
                WHEN SoNgayMuaHang / (SoDonHang - 1) <= 180 THEN N'Thỉnh thoảng (90-180 ngày)'
                ELSE N'Hiếm khi (> 180 ngày)'
             END = N'Thỉnh thoảng (90-180 ngày)' THEN 3
        ELSE 4
    END;


-- Top 10 cặp sản phẩm thường mua cùng nhau
SELECT TOP 10
    sptmc.SanPhamID1,
    sp1.TenSP AS SanPham1,
    sptmc.SanPhamID2,
    sp2.TenSP AS SanPham2,
    sptmc.SoLanMuaCung,
    sptmc.Support,
    sptmc.Confidence,
    sptmc.Lift
FROM SanPhamThuongMuaCung sptmc
JOIN SanPham sp1 ON sptmc.SanPhamID1 = sp1.SanPhamID
JOIN SanPham sp2 ON sptmc.SanPhamID2 = sp2.SanPhamID
ORDER BY sptmc.Lift DESC, sptmc.Confidence DESC;





-- Phân tích hiệu quả của gợi ý sản phẩm
WITH RecommendationEffectiveness AS (
    SELECT 
        spgy.SanPhamIDGoc,
        spgy.SanPhamIDGoiY,
        COUNT(DISTINCT ctdh1.DonHangID) AS SoLanMuaCung,
        spgy.Support,
        spgy.Confidence,
        spgy.Lift
    FROM SanPhamGoiYTheoDanhGia spgy
    LEFT JOIN ChiTietDonHang ctdh1 ON spgy.SanPhamIDGoc = ctdh1.SanPhamID
    LEFT JOIN ChiTietDonHang ctdh2 ON spgy.SanPhamIDGoiY = ctdh2.SanPhamID AND ctdh1.DonHangID = ctdh2.DonHangID
    LEFT JOIN DonHang dh ON ctdh1.DonHangID = dh.DonHangID AND dh.TrangThaiDonHang = N'Completed'
    GROUP BY spgy.SanPhamIDGoc, spgy.SanPhamIDGoiY, spgy.Support, spgy.Confidence, spgy.Lift
)
SELECT 
    sp1.TenSP AS SanPhamGoc,
    sp2.TenSP AS SanPhamGoiY,
    re.SoLanMuaCung,
    re.Support,
    re.Confidence,
    re.Lift,
    CASE 
        WHEN re.Lift >= 3 THEN N'Rất hiệu quả'
        WHEN re.Lift >= 2 THEN N'Hiệu quả'
        WHEN re.Lift >= 1 THEN N'Khá hiệu quả'
        ELSE N'Ít hiệu quả'
    END AS MucDoHieuQua
FROM RecommendationEffectiveness re
JOIN SanPham sp1 ON re.SanPhamIDGoc = sp1.SanPhamID
JOIN SanPham sp2 ON re.SanPhamIDGoiY = sp2.SanPhamID
WHERE re.SoLanMuaCung > 0
ORDER BY re.Lift DESC, re.SoLanMuaCung DESC;




-- Phân tích RFM (Recency, Frequency, Monetary)
WITH RFM_Base AS (
    SELECT 
        kh.KhachHangID,
        kh.TenKhachHang,
        DATEDIFF(DAY, MAX(dh.NgayDatHang), GETDATE()) AS Recency,
        COUNT(DISTINCT dh.DonHangID) AS Frequency,
        SUM(dh.TongTien) AS Monetary
    FROM KhachHang kh
    LEFT JOIN DonHang dh ON kh.KhachHangID = dh.KhachHangID AND dh.TrangThaiDonHang = N'Completed'
    GROUP BY kh.KhachHangID, kh.TenKhachHang
),
RFM_Scores AS (
    SELECT 
        *,
        NTILE(5) OVER (ORDER BY Recency DESC) AS R_Score,
        NTILE(5) OVER (ORDER BY Frequency) AS F_Score,
        NTILE(5) OVER (ORDER BY Monetary) AS M_Score
    FROM RFM_Base
),
RFM_Final AS (
    SELECT 
        *,
        CONCAT(R_Score, F_Score, M_Score) AS RFM_Score,
        CASE 
            WHEN R_Score >= 4 AND F_Score >= 4 AND M_Score >= 4 THEN N'Khách hàng VIP'
            WHEN R_Score >= 3 AND F_Score >= 3 AND M_Score >= 3 THEN N'Khách hàng trung thành'
            WHEN R_Score >= 3 AND F_Score >= 1 AND M_Score >= 2 THEN N'Khách hàng tiềm năng'
            WHEN R_Score >= 4 AND F_Score <= 2 AND M_Score <= 2 THEN N'Khách hàng mới'
            WHEN R_Score <= 2 AND F_Score >= 3 AND M_Score >= 3 THEN N'Khách hàng cần giữ chân'
            WHEN R_Score <= 2 AND F_Score <= 2 AND M_Score <= 2 THEN N'Khách hàng ngủ đông'
            ELSE N'Khách hàng thông thường'
        END AS PhanKhuc
    FROM RFM_Scores
)
SELECT 
    PhanKhuc,
    COUNT(*) AS SoLuongKhachHang,
    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM RFM_Final) AS DECIMAL(5,2)) AS PhanTram,
    FORMAT(AVG(Monetary), 'N0') AS ChiTieuTrungBinh,
    AVG(Frequency) AS TanSuatMuaHangTrungBinh,
    AVG(Recency) AS SoNgayTuLanCuoiTrungBinh
FROM RFM_Final
GROUP BY PhanKhuc
ORDER BY AVG(Monetary) DESC;