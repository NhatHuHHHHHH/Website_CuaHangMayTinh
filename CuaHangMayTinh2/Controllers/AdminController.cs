using CuaHangMayTinh2.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Concurrent;
using System.Data.SqlClient;

namespace CuaHangMayTinh2.Controllers
{
    public class AdminController : Controller
    {
        private readonly CuaHangMayTinh2Entities db = new CuaHangMayTinh2Entities();

        // Kiểm tra quyền admin
        private bool CheckAdmin()
        {
            return Session["LaAdmin"] != null && (bool)Session["LaAdmin"];
        }

        // GET: Admin
        public ActionResult Index()
        {
            if (!CheckAdmin())
            {
                return RedirectToAction("Login", "MayTinh");
            }
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login", "MayTinh");
        }

        // Quản lý danh mục
        public ActionResult DanhMuc(int page = 1, string search = "")
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            int pageSize = 10;
            var danhMucs = db.DanhMuc.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                danhMucs = danhMucs.Where(d => d.TenDanhMuc.Contains(search));
            }

            var totalItems = danhMucs.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            danhMucs = danhMucs.OrderBy(d => d.DanhMucID).Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return View(danhMucs.ToList());
        }

        [HttpGet]
        public ActionResult CreateDanhMuc()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");
            return View();
        }

        [HttpPost]
        public ActionResult CreateDanhMuc(DanhMuc danhMuc)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            if (ModelState.IsValid)
            {
                db.DanhMuc.Add(danhMuc);
                db.SaveChanges();
                return RedirectToAction("DanhMuc");
            }
            return View(danhMuc);
        }

        [HttpGet]
        public ActionResult EditDanhMuc(int? id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "ID danh mục không hợp lệ.";
                return RedirectToAction("DanhMuc");
            }

            var danhMuc = db.DanhMuc.Find(id);
            if (danhMuc == null) return HttpNotFound();
            return View(danhMuc);
        }

        [HttpPost]
        public ActionResult EditDanhMuc(DanhMuc danhMuc)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            if (ModelState.IsValid)
            {
                db.Entry(danhMuc).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("DanhMuc");
            }
            return View(danhMuc);
        }

        public ActionResult DeleteDanhMuc(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            var danhMuc = db.DanhMuc.Find(id);
            if (danhMuc == null) return HttpNotFound();

            try
            {
                db.DanhMuc.Remove(danhMuc);
                db.SaveChanges();
                return RedirectToAction("DanhMuc");
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Không thể xóa danh mục vì có sản phẩm liên quan. Vui lòng xóa sản phẩm trước.";
                System.Diagnostics.Debug.WriteLine("Lỗi xóa danh mục: " + ex.ToString());
                return RedirectToAction("DanhMuc");
            }
        }

        // Quản lý sản phẩm
        public ActionResult SanPham(int page = 1, string search = "")
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            int pageSize = 10;
            var sanPhams = db.SanPham
                .Include(sp => sp.DanhMuc)
                .Include(sp => sp.HinhAnhSanPham)
                .Where(sp => string.IsNullOrEmpty(search) || sp.TenSP.Contains(search))
                .OrderBy(sp => sp.SanPhamID);

            int totalItems = sanPhams.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var pagedSanPhams = sanPhams
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return View(pagedSanPhams ?? new List<SanPham>());
        }

        [HttpGet]
        public ActionResult CreateSanPham()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            ViewBag.DanhMucID = new SelectList(db.DanhMuc, "DanhMucID", "TenDanhMuc");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateSanPham(SanPham sanPham, HttpPostedFileBase[] HinhAnhFiles)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            if (ModelState.IsValid)
            {
                try
                {
                    if (sanPham.HinhAnhSanPham == null)
                    {
                        sanPham.HinhAnhSanPham = new List<HinhAnhSanPham>();
                    }

                    if (HinhAnhFiles != null && HinhAnhFiles.Length > 0 && HinhAnhFiles[0] != null)
                    {
                        string relativePath = "images/products/";
                        string absolutePath = Server.MapPath("~/" + relativePath);

                        if (!Directory.Exists(absolutePath))
                        {
                            Directory.CreateDirectory(absolutePath);
                        }

                        foreach (var file in HinhAnhFiles)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                                string extension = Path.GetExtension(file.FileName).ToLower();
                                if (!allowedExtensions.Contains(extension))
                                {
                                    ModelState.AddModelError("HinhAnhFiles", "Chỉ chấp nhận file hình ảnh (jpg, jpeg, png, gif).");
                                    ViewBag.DanhMucID = new SelectList(db.DanhMuc, "DanhMucID", "TenDanhMuc", sanPham.DanhMucID);
                                    return View(sanPham);
                                }

                                if (file.ContentLength > 5 * 1024 * 1024)
                                {
                                    ModelState.AddModelError("HinhAnhFiles", "File " + file.FileName + " quá lớn! Kích thước tối đa là 5MB.");
                                    ViewBag.DanhMucID = new SelectList(db.DanhMuc, "DanhMucID", "TenDanhMuc", sanPham.DanhMucID);
                                    return View(sanPham);
                                }

                                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.FileName);
                                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                                string fileName = fileNameWithoutExtension + "_" + timestamp + extension;
                                string filePath = Path.Combine(absolutePath, fileName);

                                using (var image = System.Drawing.Image.FromStream(file.InputStream))
                                {
                                    int targetSize = 800;
                                    int newWidth, newHeight;
                                    if (image.Width > image.Height)
                                    {
                                        newWidth = targetSize;
                                        newHeight = (int)(image.Height * ((float)targetSize / image.Width));
                                    }
                                    else
                                    {
                                        newHeight = targetSize;
                                        newWidth = (int)(image.Width * ((float)targetSize / image.Height));
                                    }

                                    using (var resizedImage = new System.Drawing.Bitmap(image, newWidth, newHeight))
                                    {
                                        int cropSize = Math.Min(newWidth, newHeight);
                                        int x = (newWidth - cropSize) / 2;
                                        int y = (newHeight - cropSize) / 2;

                                        using (var croppedImage = new System.Drawing.Bitmap(targetSize, targetSize))
                                        {
                                            using (var graphics = System.Drawing.Graphics.FromImage(croppedImage))
                                            {
                                                graphics.DrawImage(resizedImage, new System.Drawing.Rectangle(0, 0, targetSize, targetSize),
                                                    new System.Drawing.Rectangle(x, y, cropSize, cropSize), System.Drawing.GraphicsUnit.Pixel);
                                            }
                                            croppedImage.Save(filePath, image.RawFormat);
                                        }
                                    }
                                }

                                var hinhAnh = new HinhAnhSanPham
                                {
                                    DuongDanAnh = "/" + relativePath + fileName,
                                    LaHinhChinh = sanPham.HinhAnhSanPham.Count == 0
                                };
                                sanPham.HinhAnhSanPham.Add(hinhAnh);
                            }
                        }
                    }
                    else
                    {
                        var hinhAnh = new HinhAnhSanPham
                        {
                            DuongDanAnh = "/images/products/default.jpg",
                            LaHinhChinh = true
                        };
                        sanPham.HinhAnhSanPham.Add(hinhAnh);
                    }

                    sanPham.TrangThai = "Còn hàng";
                    sanPham.NgayCapNhat = DateTime.Now;
                    db.SanPham.Add(sanPham);
                    db.SaveChanges();

                    return RedirectToAction("SanPham");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi tổng thể: " + ex.ToString());
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm sản phẩm: " + ex.Message);
                }
            }

            ViewBag.DanhMucID = new SelectList(db.DanhMuc, "DanhMucID", "TenDanhMuc", sanPham.DanhMucID);
            return View(sanPham);
        }

        public ActionResult ViewSanPham(int? id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "ID sản phẩm không hợp lệ.";
                return RedirectToAction("SanPham");
            }

            var sanPham = db.SanPham
                .Include(sp => sp.DanhMuc)
                .Include(sp => sp.HinhAnhSanPham)
                .FirstOrDefault(sp => sp.SanPhamID == id.Value);

            if (sanPham == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("SanPham");
            }

            if (sanPham.NgayCapNhat == null)
            {
                sanPham.NgayCapNhat = DateTime.Now;
                db.SaveChanges();
            }

            return View(sanPham);
        }

        [HttpGet]
        public ActionResult EditSanPham(int? id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "ID sản phẩm không hợp lệ.";
                return RedirectToAction("SanPham");
            }

            var sanPham = db.SanPham.Find(id);
            if (sanPham == null) return HttpNotFound();

            ViewBag.DanhMucID = new SelectList(db.DanhMuc, "DanhMucID", "TenDanhMuc", sanPham.DanhMucID);
            return View(sanPham);
        }

        [HttpPost]
        public ActionResult EditSanPham(SanPham sanPham)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            if (ModelState.IsValid)
            {
                sanPham.NgayCapNhat = DateTime.Now;
                db.Entry(sanPham).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("SanPham");
            }
            ViewBag.DanhMucID = new SelectList(db.DanhMuc, "DanhMucID", "TenDanhMuc", sanPham.DanhMucID);
            return View(sanPham);
        }

        public ActionResult DeleteSanPham(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            try
            {
                var sanPham = db.SanPham
                    .Include(sp => sp.HinhAnhSanPham)
                    .FirstOrDefault(sp => sp.SanPhamID == id);

                if (sanPham == null) return HttpNotFound();

                if (sanPham.HinhAnhSanPham != null && sanPham.HinhAnhSanPham.Any())
                {
                    foreach (var hinhAnh in sanPham.HinhAnhSanPham.ToList())
                    {
                        if (!string.IsNullOrEmpty(hinhAnh.DuongDanAnh) && hinhAnh.DuongDanAnh != "/images/products/default.jpg")
                        {
                            string filePath = Server.MapPath(hinhAnh.DuongDanAnh);
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }
                        }
                        db.HinhAnhSanPham.Remove(hinhAnh);
                    }
                }

                db.SanPham.Remove(sanPham);
                db.SaveChanges();

                return RedirectToAction("SanPham");
            }
            catch (DbUpdateException ex)
            {
                TempData["ErrorMessage"] = "Không thể xóa sản phẩm vì có dữ liệu liên quan (hình ảnh, đơn hàng, v.v.). Vui lòng xóa dữ liệu liên quan trước.";
                System.Diagnostics.Debug.WriteLine("Lỗi xóa sản phẩm: " + ex.ToString());
                return RedirectToAction("SanPham");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa sản phẩm: " + ex.Message;
                System.Diagnostics.Debug.WriteLine("Lỗi xóa sản phẩm: " + ex.ToString());
                return RedirectToAction("SanPham");
            }
        }

        // Quản lý đơn hàng
        public ActionResult DonHang(int page = 1, string search = "")
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            int pageSize = 10;
            var donHangs = db.DonHang.Include(d => d.KhachHang).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                donHangs = donHangs.Where(d => d.MaDonHang.Contains(search));
            }

            var totalItems = donHangs.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            donHangs = donHangs.OrderByDescending(d => d.NgayDatHang).Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return View(donHangs.ToList());
        }

        [HttpPost]
        public ActionResult UpdateDonHangStatus(int id, string status)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            var donHang = db.DonHang.Find(id);
            if (donHang == null) return HttpNotFound();

            donHang.TrangThaiDonHang = status;
            donHang.NgayCapNhat = DateTime.Now;
            db.SaveChanges();

            if (status == "Completed")
            {
                db.Database.ExecuteSqlCommand("EXEC CapNhatKhoDuaTrenDonHang @DonHangID", new System.Data.SqlClient.SqlParameter("@DonHangID", id));
                db.Database.ExecuteSqlCommand("EXEC CapNhatSanPhamThuongMuaCung @DonHangID, @SupportThreshold",
                    new System.Data.SqlClient.SqlParameter("@DonHangID", id),
                    new System.Data.SqlClient.SqlParameter("@SupportThreshold", 2));
            }

            return RedirectToAction("DonHang");
        }

        // Quản lý khách hàng
        public ActionResult KhachHang(int page = 1, string search = "")
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            int pageSize = 10;
            var khachHangs = db.KhachHang.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                khachHangs = khachHangs.Where(k => k.TenKhachHang.Contains(search) || k.Email.Contains(search));
            }

            var totalItems = khachHangs.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            khachHangs = khachHangs.OrderBy(k => k.KhachHangID).Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return View(khachHangs.ToList());
        }

        public ActionResult LockKhachHang(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            var khachHang = db.KhachHang.Find(id);
            if (khachHang == null) return HttpNotFound();

            khachHang.ThoiGianKhoa = DateTime.Now.AddDays(1);
            db.SaveChanges();
            return RedirectToAction("KhachHang");
        }

        public ActionResult UnlockKhachHang(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            var khachHang = db.KhachHang.Find(id);
            if (khachHang == null) return HttpNotFound();

            khachHang.ThoiGianKhoa = null;
            khachHang.SoLanDangNhapSai = 0;
            db.SaveChanges();
            return RedirectToAction("KhachHang");
        }

        // Quản lý đánh giá
        public ActionResult DanhGia(int page = 1, string search = "")
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            int pageSize = 10;
            var danhGias = db.DanhGia.Include(d => d.KhachHang).Include(d => d.SanPham).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                danhGias = danhGias.Where(d => d.SanPham.TenSP.Contains(search));
            }

            var totalItems = danhGias.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            danhGias = danhGias.OrderByDescending(d => d.NgayDanhGia).Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return View(danhGias.ToList());
        }

        public ActionResult DeleteDanhGia(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            var danhGia = db.DanhGia.Find(id);
            if (danhGia == null) return HttpNotFound();

            db.DanhGia.Remove(danhGia);
            db.SaveChanges();
            return RedirectToAction("DanhGia");
        }

        // Quản lý khuyến mãi
        public ActionResult KhuyenMai(int page = 1, string search = "")
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            int pageSize = 10;
            var khuyenMais = db.KhuyenMai.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                khuyenMais = khuyenMais.Where(k => k.TenKhuyenMai.Contains(search));
            }

            var totalItems = khuyenMais.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            khuyenMais = khuyenMais.OrderBy(k => k.KhuyenMaiID).Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return View(khuyenMais.ToList());
        }

        [HttpGet]
        public ActionResult CreateKhuyenMai()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");
            return View();
        }

        [HttpPost]
        public ActionResult CreateKhuyenMai(KhuyenMai khuyenMai)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            if (ModelState.IsValid)
            {
                if (khuyenMai.NgayBatDau >= khuyenMai.NgayKetThuc)
                {
                    ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau ngày bắt đầu.");
                    return View(khuyenMai);
                }

                db.KhuyenMai.Add(khuyenMai);
                db.SaveChanges();
                return RedirectToAction("KhuyenMai");
            }
            return View(khuyenMai);
        }

        [HttpGet]
        public ActionResult EditKhuyenMai(int? id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = "ID khuyến mãi không hợp lệ.";
                return RedirectToAction("KhuyenMai");
            }

            var khuyenMai = db.KhuyenMai.Find(id);
            if (khuyenMai == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy khuyến mãi.";
                return RedirectToAction("KhuyenMai");
            }

            return View(khuyenMai);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditKhuyenMai(KhuyenMai khuyenMai)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            if (ModelState.IsValid)
            {
                try
                {
                    if (khuyenMai.NgayBatDau >= khuyenMai.NgayKetThuc)
                    {
                        ModelState.AddModelError("NgayKetThuc", "Ngày kết thúc phải sau ngày bắt đầu.");
                        return View(khuyenMai);
                    }

                    db.Entry(khuyenMai).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("KhuyenMai");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Lỗi khi chỉnh sửa khuyến mãi: " + ex.ToString());
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi chỉnh sửa khuyến mãi: " + ex.Message);
                }
            }

            return View(khuyenMai);
        }

        public ActionResult DeleteKhuyenMai(int id)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            var khuyenMai = db.KhuyenMai.Find(id);
            if (khuyenMai == null) return HttpNotFound();

            db.KhuyenMai.Remove(khuyenMai);
            db.SaveChanges();
            return RedirectToAction("KhuyenMai");
        }

        // Quản lý sản phẩm liên quan
        public ActionResult SanPhamLienQuan(int page = 1)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            int pageSize = 10;
            var sanPhamLienQuans = db.SanPhamLienQuan.Include(s => s.SanPham).Include(s => s.SanPham1).AsQueryable();

            var totalItems = sanPhamLienQuans.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            sanPhamLienQuans = sanPhamLienQuans.OrderBy(s => s.SanPhamChinhID).Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(sanPhamLienQuans.ToList());
        }

        [HttpGet]
        public ActionResult CreateSanPhamLienQuan()
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            ViewBag.SanPhamChinhID = new SelectList(db.SanPham, "SanPhamID", "TenSP");
            ViewBag.SanPhamLienQuanID = new SelectList(db.SanPham, "SanPhamID", "TenSP");
            return View();
        }

        [HttpPost]
        public ActionResult CreateSanPhamLienQuan(SanPhamLienQuan sanPhamLienQuan)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            if (ModelState.IsValid)
            {
                if (sanPhamLienQuan.SanPhamChinhID == sanPhamLienQuan.SanPhamLienQuanID)
                {
                    ModelState.AddModelError("SanPhamLienQuanID", "Sản phẩm liên quan không được trùng với sản phẩm chính.");
                    ViewBag.SanPhamChinhID = new SelectList(db.SanPham, "SanPhamID", "TenSP", sanPhamLienQuan.SanPhamChinhID);
                    ViewBag.SanPhamLienQuanID = new SelectList(db.SanPham, "SanPhamID", "TenSP", sanPhamLienQuan.SanPhamLienQuanID);
                    return View(sanPhamLienQuan);
                }

                db.SanPhamLienQuan.Add(sanPhamLienQuan);
                db.SaveChanges();
                return RedirectToAction("SanPhamLienQuan");
            }
            ViewBag.SanPhamChinhID = new SelectList(db.SanPham, "SanPhamID", "TenSP", sanPhamLienQuan.SanPhamChinhID);
            ViewBag.SanPhamLienQuanID = new SelectList(db.SanPham, "SanPhamID", "TenSP", sanPhamLienQuan.SanPhamLienQuanID);
            return View(sanPhamLienQuan);
        }

        public ActionResult DeleteSanPhamLienQuan(int sanPhamChinhID, int sanPhamLienQuanID)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            var sanPhamLienQuan = db.SanPhamLienQuan.Find(sanPhamChinhID, sanPhamLienQuanID);
            if (sanPhamLienQuan == null) return HttpNotFound();

            db.SanPhamLienQuan.Remove(sanPhamLienQuan);
            db.SaveChanges();
            return RedirectToAction("SanPhamLienQuan");
        }

        // Quản lý sản phẩm thường mua cùng
        public ActionResult SanPhamThuongMuaCung(int page = 1)
        {
            if (!CheckAdmin()) return RedirectToAction("Login", "MayTinh");

            int pageSize = 10;
            var sanPhamThuongMuaCungs = db.SanPhamThuongMuaCung.Include(s => s.SanPham).Include(s => s.SanPham1).AsQueryable();

            var totalItems = sanPhamThuongMuaCungs.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            sanPhamThuongMuaCungs = sanPhamThuongMuaCungs.OrderByDescending(s => s.SoLanMuaCung).Skip((page - 1) * pageSize).Take(pageSize);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(sanPhamThuongMuaCungs.ToList());
        }


        // Biến static để lưu trạng thái tiến trình
        private static ConcurrentDictionary<string, AprioriProgressInfo> _progressInfo = new ConcurrentDictionary<string, AprioriProgressInfo>();
        private readonly int _commandTimeout = 1800; // 30 phút
        // Lớp lưu thông tin tiến trình
        public class AprioriProgressInfo
        {
            public string Status { get; set; }
            public string Message { get; set; }
            public int PercentComplete { get; set; }
            public DateTime LastUpdated { get; set; }
        }
        #region Apriori


        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>

        // GET: Admin/RunApriori
        public ActionResult RunApriori()
        {
            if (!CheckAdmin())
                return RedirectToAction("Login");

            // Lấy danh sách danh mục cho dropdown
            ViewBag.Categories = new SelectList(db.DanhMuc, "DanhMucID", "TenDanhMuc");

            // Lấy kết quả Apriori từ bảng SanPhamThuongMuaCung
            var results = db.SanPhamThuongMuaCung
                .Include(s => s.SanPham)
                .Include(s => s.SanPham1)
                .OrderByDescending(s => s.Confidence)
                //.Take(190)
                .ToList()
                .Select(s => new AprioriResult
                {
                    SanPhamID1 = s.SanPhamID1,
                    SanPhamID2 = s.SanPhamID2,
                    TenSP1 = s.SanPham != null ? s.SanPham.TenSP : "",
                    TenSP2 = s.SanPham1 != null ? s.SanPham1.TenSP : "",
                    SoLanMuaCung = s.SoLanMuaCung ?? 0,
                    Support = (float)(s.Support ?? 0),
                    Confidence = (float)(s.Confidence ?? 0),
                    Lift = (float)(s.Lift ?? 0),
                    HinhAnh1 = s.SanPham != null && s.SanPham.HinhAnhSanPham != null && s.SanPham.HinhAnhSanPham.Any(h => h.LaHinhChinh ?? false)
                        ? s.SanPham.HinhAnhSanPham.First(h => h.LaHinhChinh ?? false).DuongDanAnh
                        : "/images/products/default.jpg",
                    HinhAnh2 = s.SanPham1 != null && s.SanPham1.HinhAnhSanPham != null && s.SanPham1.HinhAnhSanPham.Any(h => h.LaHinhChinh ?? false)
                        ? s.SanPham1.HinhAnhSanPham.First(h => h.LaHinhChinh ?? false).DuongDanAnh
                        : "/images/products/default.jpg"
                })
                .ToList();

            ViewBag.Results = results;

            return View();
        }

        // GET: Admin/FilterAprioriResults
        [HttpGet]
        public ActionResult FilterAprioriResults(float minSupport, float minConfidence, float minLift, int? categoryId)
        {
            try
            {
                if (!CheckAdmin())
                {
                    return Json(new { success = false, message = "Chỉ admin mới có thể xem kết quả Apriori!" }, JsonRequestBehavior.AllowGet);
                }

                // Ghi log để debug
                System.Diagnostics.Debug.WriteLine($"Filtering with: minSupport={minSupport}, minConfidence={minConfidence}, minLift={minLift}, categoryId={categoryId}");

                // Đặt tất cả các bản ghi về DaLoc = 0 trước khi lọc
                db.Database.ExecuteSqlCommand("UPDATE SanPhamThuongMuaCung SET DaLoc = 0, NgayLoc = NULL");

                // Lọc kết quả từ bảng SanPhamThuongMuaCung
                var query = db.SanPhamThuongMuaCung
                    .Include(s => s.SanPham)
                    .Include(s => s.SanPham1)
                    .Where(s => s.Support >= minSupport &&
                               s.Confidence >= minConfidence &&
                               s.Lift >= minLift);

                // Lọc theo danh mục nếu có
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    query = query.Where(s =>
                        s.SanPham.DanhMucID == categoryId.Value ||
                        s.SanPham1.DanhMucID == categoryId.Value);
                }

                // Đánh dấu các kết quả đã lọc
                string loaiLoc = $"Support: {minSupport}+, Confidence: {minConfidence}+, Lift: {minLift}+";
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    var danhMuc = db.DanhMuc.Find(categoryId.Value);
                    loaiLoc += $", Danh mục: {(danhMuc != null ? danhMuc.TenDanhMuc : "Không xác định")}";
                }

                // Lấy danh sách ID của các bản ghi thỏa mãn điều kiện lọc
                var filteredIds = query.Select(s => new { s.SanPhamID1, s.SanPhamID2 }).ToList();

                // Cập nhật trạng thái DaLoc cho các bản ghi thỏa mãn điều kiện
                if (filteredIds.Any())
                {
                    // Cập nhật từng batch 1000 bản ghi để tránh quá tải
                    const int batchSize = 1000;
                    for (int i = 0; i < filteredIds.Count; i += batchSize)
                    {
                        var batch = filteredIds.Skip(i).Take(batchSize).ToList();
                        foreach (var item in batch)
                        {
                            var record = db.SanPhamThuongMuaCung.FirstOrDefault(s =>
                                s.SanPhamID1 == item.SanPhamID1 && s.SanPhamID2 == item.SanPhamID2);

                            if (record != null)
                            {
                                record.DaLoc = true;
                                record.NgayLoc = DateTime.Now;
                                record.DuocLoc = loaiLoc;
                            }
                        }
                        db.SaveChanges();
                    }
                }

                // Lấy kết quả
                var results = query
                    .OrderByDescending(s => s.Confidence)
                    .ToList()
                    .Select(s => new AprioriResult
                    {
                        SanPhamID1 = s.SanPhamID1,
                        SanPhamID2 = s.SanPhamID2,
                        TenSP1 = s.SanPham != null ? s.SanPham.TenSP : "",
                        TenSP2 = s.SanPham1 != null ? s.SanPham1.TenSP : "",
                        SoLanMuaCung = s.SoLanMuaCung ?? 0,
                        Support = (float)(s.Support ?? 0),
                        Confidence = (float)(s.Confidence ?? 0),
                        Lift = (float)(s.Lift ?? 0),
                        HinhAnh1 = s.SanPham != null && s.SanPham.HinhAnhSanPham != null && s.SanPham.HinhAnhSanPham.Any(h => h.LaHinhChinh ?? false)
                            ? s.SanPham.HinhAnhSanPham.First(h => h.LaHinhChinh ?? false).DuongDanAnh
                            : "/images/products/default.jpg",
                        HinhAnh2 = s.SanPham1 != null && s.SanPham1.HinhAnhSanPham != null && s.SanPham1.HinhAnhSanPham.Any(h => h.LaHinhChinh ?? false)
                            ? s.SanPham1.HinhAnhSanPham.First(h => h.LaHinhChinh ?? false).DuongDanAnh
                            : "/images/products/default.jpg"
                    })
                    .ToList();

                // Ghi log số lượng kết quả
                System.Diagnostics.Debug.WriteLine($"Found {results.Count} results after filtering");

                return Json(new { success = true, results = results, count = results.Count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi trong FilterAprioriResults: " + ex.ToString());
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>D
       
        // GET: Admin/SanPhamGoiYTheoDanhGia
        public ActionResult SanPhamGoiYTheoDanhGia()
        {
            if (!CheckAdmin())
                return RedirectToAction("Login");

            // Lấy danh sách danh mục cho dropdown
            ViewBag.Categories = new SelectList(db.DanhMuc, "DanhMucID", "TenDanhMuc");

            // Lấy kết quả gợi ý từ bảng SanPhamGoiYTheoDanhGia
            var results = db.SanPhamGoiYTheoDanhGia
                .Include(s => s.SanPham)  
                .Include(s => s.SanPham1) 
                .OrderByDescending(s => s.DiemPhoBien)
                .ToList()
                .Select(s => new SanPhamGoiYViewModel
                {
                    SanPhamIDGoc = s.SanPhamIDGoc,
                    SanPhamIDGoiY = s.SanPhamIDGoiY,
                    TenSPGoc = s.SanPham != null ? s.SanPham.TenSP : "",
                    TenSPGoiY = s.SanPham1 != null ? s.SanPham1.TenSP : "",
                    Support = (float)s.Support,
                    Confidence = (float)s.Confidence,
                    Lift = (float)s.Lift,
                    DiemDanhGia = (float)s.DiemDanhGia,
                    LuotXem = s.LuotXem ?? 0,
                    DiemPhoBien = (float)s.DiemPhoBien,
                    HinhAnhGoc = s.SanPham != null && s.SanPham.HinhAnhSanPham != null && s.SanPham.HinhAnhSanPham.Any(h => h.LaHinhChinh ?? false)
                        ? s.SanPham.HinhAnhSanPham.First(h => h.LaHinhChinh ?? false).DuongDanAnh
                        : "/images/products/default.jpg",
                    HinhAnhGoiY = s.SanPham1 != null && s.SanPham1.HinhAnhSanPham != null && s.SanPham1.HinhAnhSanPham.Any(h => h.LaHinhChinh ?? false)
                        ? s.SanPham1.HinhAnhSanPham.First(h => h.LaHinhChinh ?? false).DuongDanAnh
                        : "/images/products/default.jpg"
                })
                .ToList();

            ViewBag.Results = results;

            return View();
        }

        // AdminController.cs - Phương thức FilterSanPhamGoiY
        [HttpGet]
        public ActionResult FilterSanPhamGoiY(float minDanhGia = 4.5f, int minLuotXem = 100, int? categoryId = null,
            float minSupport = 0.001f, float minConfidence = 0.02f, float minLift = 0.5f)
        {
            try
            {
                if (!CheckAdmin())
                {
                    return Json(new { success = false, message = "Chỉ admin mới có thể xem kết quả gợi ý!" }, JsonRequestBehavior.AllowGet);
                }

                // Ghi log để debug
                System.Diagnostics.Debug.WriteLine($"Filtering with: minDanhGia={minDanhGia}, minLuotXem={minLuotXem}, " +
                    $"minSupport={minSupport}, minConfidence={minConfidence}, minLift={minLift}, categoryId={categoryId}");

                // Đặt tất cả các bản ghi về DaLoc = 0 trước khi lọc
                db.Database.ExecuteSqlCommand("UPDATE SanPhamGoiYTheoDanhGia SET DaLoc = 0, NgayLoc = NULL");

                // Lọc kết quả từ bảng SanPhamGoiYTheoDanhGia
                var query = db.SanPhamGoiYTheoDanhGia
                    .Include(s => s.SanPham)
                    .Include(s => s.SanPham1)
                    .Where(s => s.DiemDanhGia >= minDanhGia &&
                               (s.LuotXem ?? 0) >= minLuotXem &&
                               s.Support >= minSupport &&
                               s.Confidence >= minConfidence &&
                               s.Lift >= minLift);

                // Lọc theo danh mục nếu có
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    query = query.Where(s =>
                        s.SanPham.DanhMucID == categoryId.Value ||
                        s.SanPham1.DanhMucID == categoryId.Value);
                }

                // Đánh dấu các kết quả đã lọc
                string loaiLoc = $"Đánh giá: {minDanhGia}+, Lượt xem: {minLuotXem}+, Support: {minSupport}+, Confidence: {minConfidence}+, Lift: {minLift}+";
                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    var danhMuc = db.DanhMuc.Find(categoryId.Value);
                    loaiLoc += $", Danh mục: {(danhMuc != null ? danhMuc.TenDanhMuc : "Không xác định")}";
                }

                // Lấy danh sách ID của các bản ghi thỏa mãn điều kiện lọc
                var filteredIds = query.Select(s => s.GoiYID).ToList();

                // Cập nhật trạng thái DaLoc cho các bản ghi thỏa mãn điều kiện
                if (filteredIds.Any())
                {
                    // Cập nhật từng batch 1000 bản ghi để tránh quá tải
                    const int batchSize = 1000;
                    for (int i = 0; i < filteredIds.Count; i += batchSize)
                    {
                        var batch = filteredIds.Skip(i).Take(batchSize).ToList();
                        string idList = string.Join(",", batch);
                        db.Database.ExecuteSqlCommand(
                            $"UPDATE SanPhamGoiYTheoDanhGia SET DaLoc = 1, NgayLoc = GETDATE(), DuocLoc = '{loaiLoc}' WHERE GoiYID IN ({idList})");
                    }
                }

                // Lấy kết quả đã lọc
                var results = query
                    .OrderByDescending(s => s.DiemPhoBien)
                    .ToList()
                    .Select(s => new SanPhamGoiYViewModel
                    {
                        SanPhamIDGoc = s.SanPhamIDGoc,
                        SanPhamIDGoiY = s.SanPhamIDGoiY,
                        TenSPGoc = s.SanPham != null ? s.SanPham.TenSP : "",
                        TenSPGoiY = s.SanPham1 != null ? s.SanPham1.TenSP : "",
                        Support = (float)s.Support,
                        Confidence = (float)s.Confidence,
                        Lift = (float)s.Lift,
                        DiemDanhGia = (float)s.DiemDanhGia,
                        LuotXem = s.LuotXem ?? 0,
                        DiemPhoBien = (float)s.DiemPhoBien,
                        HinhAnhGoc = s.SanPham != null && s.SanPham.HinhAnhSanPham != null && s.SanPham.HinhAnhSanPham.Any(h => h.LaHinhChinh ?? false)
                            ? s.SanPham.HinhAnhSanPham.First(h => h.LaHinhChinh ?? false).DuongDanAnh
                            : "/images/products/default.jpg",
                        HinhAnhGoiY = s.SanPham1 != null && s.SanPham1.HinhAnhSanPham != null && s.SanPham1.HinhAnhSanPham.Any(h => h.LaHinhChinh ?? false)
                            ? s.SanPham1.HinhAnhSanPham.First(h => h.LaHinhChinh ?? false).DuongDanAnh
                            : "/images/products/default.jpg"
                    })
                    .ToList();

                // Ghi log số lượng kết quả
                System.Diagnostics.Debug.WriteLine($"Found {results.Count} results after filtering");

                return Json(new { success = true, results = results, count = results.Count }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi trong FilterSanPhamGoiY: " + ex.ToString());
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <returns></returns>

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // Lớp kết quả Apriori
    public class AprioriResult
    {
        public int SanPhamID1 { get; set; }
        public int SanPhamID2 { get; set; }
        public string TenSP1 { get; set; }
        public string TenSP2 { get; set; }
        public int SoLanMuaCung { get; set; }
        public float Support { get; set; }
        public float Confidence { get; set; }
        public float Lift { get; set; }
        public string HinhAnh1 { get; set; }
        public string HinhAnh2 { get; set; }
    }
}