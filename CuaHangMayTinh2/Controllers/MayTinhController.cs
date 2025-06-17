using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CuaHangMayTinh2.Models;
using System.IO;
using System.Data.Entity;
using System.Net.Mail;
using System.Net;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;

namespace CuaHangMayTinh2.Controllers
{
    public class MayTinhController : Controller
    {
        private CuaHangMayTinh2Entities db = new CuaHangMayTinh2Entities();

        // GET: MayTinh/Index
        public ActionResult Index(string danhMucId = null)
        {
            // Lấy danh sách sản phẩm
            var sanPhamList = string.IsNullOrEmpty(danhMucId)
                ? db.SanPham.Take(8).ToList()
                : db.SanPham.Where(sp => sp.DanhMucID.ToString() == danhMucId).Take(20).ToList();

            // Lấy danh sách sản phẩm nổi bật
            var sanPhamNoiBat = db.SanPham
                .OrderByDescending(sp => sp.DanhGia)
                .Take(5)
                .ToList();

            // Lấy danh sách sản phẩm được yêu thích nhiều (dựa trên bảng SanPhamYeuThich)
            var sanPhamYeuThich = db.SanPhamYeuThich
                 .GroupBy(spyt => spyt.SanPhamID)
                 .Select(g => new
                 {
                     SanPhamID = g.Key,
                     SoLuongYeuThich = g.Count()
                 })
                 .OrderByDescending(x => x.SoLuongYeuThich)
                 .Take(5)
                 .Join(db.SanPham,
                       spyt => spyt.SanPhamID,
                       sp => sp.SanPhamID,
                       (spyt, sp) => sp)
                 .ToList();

            // Lấy danh sách sản phẩm xem nhiều
            var sanPhamXemNhieu = db.SanPham
                .OrderByDescending(sp => sp.LuotXem)
                .Take(5)
                .ToList();

            // Lấy danh sách sản phẩm yêu thích của khách hàng hiện tại
            int? khachHangId = Session["KhachHangID"] != null ? Convert.ToInt32(Session["KhachHangID"]) : (int?)null;
            var sanPhamYeuThichList = khachHangId.HasValue
                ? db.SanPhamYeuThich.Where(spyt => spyt.KhachHangID == khachHangId.Value).ToList()
                : new List<SanPhamYeuThich>();

            // Lấy tất cả ID sản phẩm để truy vấn hình ảnh chính
            var allSanPhamIds = sanPhamList.Select(sp => sp.SanPhamID)
                .Union(sanPhamNoiBat.Select(sp => sp.SanPhamID))
                .Union(sanPhamYeuThich.Select(sp => sp.SanPhamID))
                .Union(sanPhamXemNhieu.Select(sp => sp.SanPhamID))
                .Distinct()
                .ToList();

            // Lấy hình ảnh chính cho các sản phẩm
            var hinhAnhChinhDict = db.HinhAnhSanPham
                .Where(ha => ha.SanPhamID.HasValue && allSanPhamIds.Contains(ha.SanPhamID.Value) && ha.LaHinhChinh == true)
                .ToDictionary(ha => ha.SanPhamID.Value, ha => ha.DuongDanAnh ?? "/images/default.jpg");

            // Truyền dữ liệu vào ViewBag
            ViewBag.DanhMucList = db.DanhMuc.ToList();
            ViewBag.SanPhamNoiBat = sanPhamNoiBat;
            ViewBag.SanPhamYeuThich = sanPhamYeuThich;
            ViewBag.SanPhamXemNhieu = sanPhamXemNhieu;
            ViewBag.HinhAnhChinhDict = hinhAnhChinhDict;
            ViewBag.SanPhamYeuThichList = sanPhamYeuThichList; // Danh sách sản phẩm yêu thích của khách hàng hiện tại

            return View(sanPhamList);
        }

        // GET: LoadMoreProducts
        public ActionResult LoadMoreProducts(int skip, int take, string danhMucId = null)
        {
            try
            {
                // Lấy danh sách sản phẩm
                IQueryable<SanPham> query = db.SanPham.OrderBy(sp => sp.SanPhamID);

                if (!string.IsNullOrEmpty(danhMucId))
                {
                    if (int.TryParse(danhMucId, out int danhMucIdInt))
                    {
                        query = query.Where(sp => sp.DanhMucID == danhMucIdInt);
                    }
                    else
                    {
                        return new EmptyResult();
                    }
                }

                var sanPhamList = query.Skip(skip).Take(take).ToList();

                if (!sanPhamList.Any())
                {
                    return new EmptyResult();
                }

                // Lấy danh sách sản phẩm yêu thích của khách hàng hiện tại
                int? khachHangId = Session["KhachHangID"] != null ? Convert.ToInt32(Session["KhachHangID"]) : (int?)null;
                var sanPhamYeuThichList = khachHangId.HasValue
                    ? db.SanPhamYeuThich.Where(spyt => spyt.KhachHangID == khachHangId.Value).ToList()
                    : new List<SanPhamYeuThich>();

                // Lấy hình ảnh chính cho các sản phẩm
                var sanPhamIds = sanPhamList.Select(sp => sp.SanPhamID).ToList();
                var hinhAnhChinhDict = db.HinhAnhSanPham
                    .Where(ha => ha.SanPhamID.HasValue && sanPhamIds.Contains(ha.SanPhamID.Value) && ha.LaHinhChinh == true)
                    .ToDictionary(
                        ha => ha.SanPhamID.Value,
                        ha => ha.DuongDanAnh ?? "/images/default.jpg"
                    );

                ViewBag.HinhAnhChinhDict = hinhAnhChinhDict;

                // Truyền cả danh sách sản phẩm và danh sách yêu thích qua Partial View
                return PartialView("_SanPhamPartial", new Tuple<List<SanPham>, List<SanPhamYeuThich>>(sanPhamList, sanPhamYeuThichList));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in LoadMoreProducts: " + ex.ToString());
                return new HttpStatusCodeResult(500, "Internal Server Error: " + ex.Message);
            }
        }

        // GET: MayTinh/BuyNow
        public ActionResult BuyNow(int id)
        {
            int? khachHangId = (int?)Session["KhachHangID"];
            if (!khachHangId.HasValue)
            {
                return RedirectToAction("Login", "MayTinh", new { returnUrl = Url.Action("BuyNow", new { id = id }) });
            }

            var existingItem = db.GioHang.FirstOrDefault(gh => gh.KhachHangID == khachHangId.Value && gh.SanPhamID == id);
            if (existingItem == null)
            {
                var gioHangItem = new GioHang
                {
                    KhachHangID = khachHangId.Value,
                    SanPhamID = id,
                    SoLuong = 1
                };
                db.GioHang.Add(gioHangItem);
                db.SaveChanges();
            }

            return RedirectToAction("Checkout", "Cart");
        }
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var sanPham = db.SanPham.Find(id);
            if (sanPham == null)
            {
                return HttpNotFound();
            }

            try
            {
                // Tăng lượt xem
                sanPham.LuotXem = (sanPham.LuotXem ?? 0) + 1;
                db.SaveChanges();

                // Lấy danh mục
                ViewBag.DanhMuc = sanPham.DanhMuc != null ? sanPham.DanhMuc.TenDanhMuc : "Không xác định";

                // Lấy hình ảnh
                var hinhAnhList = db.HinhAnhSanPham.Where(h => h.SanPhamID == id).ToList();
                ViewBag.HinhAnhList = hinhAnhList;
                ViewBag.MainImageUrl = hinhAnhList.Any() ? hinhAnhList.FirstOrDefault(h => h.DuongDanAnh != null)?.DuongDanAnh : "/images/default.jpg";

                // Kiểm tra sản phẩm yêu thích - Đảm bảo ViewBag.IsLiked luôn là bool, không phải null
                bool isLiked = false;
                if (Session["KhachHangID"] != null)
                {
                    int khachHangId;
                    if (int.TryParse(Session["KhachHangID"].ToString(), out khachHangId))
                    {
                        isLiked = db.SanPhamYeuThich.Any(s => s.KhachHangID == khachHangId && s.SanPhamID == id);
                    }
                }
                ViewBag.IsLiked = isLiked;

                // PHẦN 1: Lấy sản phẩm gợi ý mua kèm từ kết quả Apriori đã lọc
                var goiYMuaKemList = new List<SanPham>();
                var muaKemMetrics = new Dictionary<int, Dictionary<string, object>>();

                // Lấy các sản phẩm thường mua cùng từ kết quả đã lọc (DaLoc = true)
                var sanPhamThuongMuaCung = db.SanPhamThuongMuaCung
                    .Where(s => (s.SanPhamID1 == id || s.SanPhamID2 == id) && s.DaLoc == true)
                    .OrderByDescending(s => s.Confidence)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Tìm thấy {sanPhamThuongMuaCung.Count} sản phẩm thường mua cùng đã lọc");

                foreach (var item in sanPhamThuongMuaCung)
                {
                    // Không cần kiểm tra null vì SanPhamID1 và SanPhamID2 là int không nullable
                    int relatedProductId = item.SanPhamID1 == id.Value ? item.SanPhamID2 : item.SanPhamID1;

                    var relatedProduct = db.SanPham.Find(relatedProductId);

                    if (relatedProduct != null && !goiYMuaKemList.Any(p => p.SanPhamID == relatedProductId))
                    {
                        goiYMuaKemList.Add(relatedProduct);

                        // Lưu thông tin metrics với loại gợi ý
                        muaKemMetrics[relatedProductId] = new Dictionary<string, object>
                {
                    { "Confidence", item.Confidence.HasValue ? item.Confidence.Value : 0 },
                    { "Lift", item.Lift.HasValue ? item.Lift.Value : 0 },
                    { "SoLanMuaCung", item.SoLanMuaCung.HasValue ? item.SoLanMuaCung.Value : 0 },
                    { "LoaiGoiY", "Mua cùng" }
                };
                    }
                }

                // PHẦN 2: Lấy sản phẩm gợi ý theo đánh giá từ bảng SanPhamGoiYTheoDanhGia đã lọc
                var goiYDanhGiaCaoList = new List<SanPham>();
                var danhGiaMetrics = new Dictionary<int, Dictionary<string, object>>();

                // Lấy các sản phẩm gợi ý theo đánh giá từ kết quả đã lọc (DaLoc = true)
                var sanPhamGoiYTheoDanhGia = db.SanPhamGoiYTheoDanhGia
                    .Where(s => s.SanPhamIDGoc == id && s.DaLoc == true)
                    .OrderByDescending(s => s.DiemPhoBien)
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"Tìm thấy {sanPhamGoiYTheoDanhGia.Count} sản phẩm gợi ý theo đánh giá đã lọc");

                // In ra thông tin chi tiết để debug
                foreach (var item in sanPhamGoiYTheoDanhGia)
                {
                    System.Diagnostics.Debug.WriteLine($"SanPhamIDGoc: {item.SanPhamIDGoc}, SanPhamIDGoiY: {item.SanPhamIDGoiY}, DaLoc: {item.DaLoc}, DiemPhoBien: {item.DiemPhoBien}");
                }

                foreach (var item in sanPhamGoiYTheoDanhGia)
                {
                    // Không cần kiểm tra null vì SanPhamIDGoiY là int không nullable
                    int relatedProductId = item.SanPhamIDGoiY;

                    var relatedProduct = db.SanPham.Find(relatedProductId);

                    if (relatedProduct != null && !goiYDanhGiaCaoList.Any(p => p.SanPhamID == relatedProductId))
                    {
                        goiYDanhGiaCaoList.Add(relatedProduct);

                        // Lưu thông tin metrics với loại gợi ý
                        danhGiaMetrics[relatedProductId] = new Dictionary<string, object>
                {
                    { "DiemDanhGia", item.DiemDanhGia.HasValue ? item.DiemDanhGia.Value : 0 },
                    { "DiemPhoBien", item.DiemPhoBien.HasValue ? item.DiemPhoBien.Value : 0 },
                    { "LuotXem", item.LuotXem.HasValue ? item.LuotXem.Value : 0 },
                    { "LoaiGoiY", "Đánh giá" }
                };
                    }
                }

                // Truyền danh sách gợi ý riêng biệt sang view
                ViewBag.GoiYMuaKemList = goiYMuaKemList;
                ViewBag.MuaKemMetrics = muaKemMetrics;

                ViewBag.GoiYDanhGiaCaoList = goiYDanhGiaCaoList;
                ViewBag.DanhGiaMetrics = danhGiaMetrics;

                // Gộp tất cả ID sản phẩm để lấy hình ảnh
                var allProductIds = goiYMuaKemList.Select(p => p.SanPhamID)
                                    .Concat(goiYDanhGiaCaoList.Select(p => p.SanPhamID))
                                    .Distinct()
                                    .ToList();

                // Lấy hình ảnh chính cho tất cả sản phẩm gợi ý
                var goiYHinhAnhChinhDict = new Dictionary<int, string>();

                if (allProductIds.Any())
                {
                    var goiYHinhAnhChinh = db.HinhAnhSanPham
                        .Where(h => h.SanPhamID.HasValue && allProductIds.Contains(h.SanPhamID.Value) && (h.LaHinhChinh ?? false))
                        .ToList();

                    foreach (var productId in allProductIds)
                    {
                        var hinhAnh = goiYHinhAnhChinh.FirstOrDefault(h => h.SanPhamID == productId);
                        goiYHinhAnhChinhDict[productId] = hinhAnh?.DuongDanAnh ?? "/images/default.jpg";
                    }
                }

                ViewBag.GoiYHinhAnhChinhDict = goiYHinhAnhChinhDict;

                // In ra số lượng sản phẩm trong mỗi danh sách để debug
                System.Diagnostics.Debug.WriteLine($"Số lượng sản phẩm gợi ý mua kèm: {goiYMuaKemList.Count}");
                System.Diagnostics.Debug.WriteLine($"Số lượng sản phẩm gợi ý đánh giá cao: {goiYDanhGiaCaoList.Count}");

                return View(sanPham);
            }
            catch (Exception ex)
            {
                // Log lỗi
                System.Diagnostics.Debug.WriteLine("Lỗi trong Details: " + ex.Message);

                // Trả về view với thông báo lỗi
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi tải thông tin sản phẩm. Vui lòng thử lại sau.";
                return View(sanPham);
            }
        }

        // POST: MayTinh/AddOrUpdateReview
        [HttpPost]
        public ActionResult AddOrUpdateReview(int productId, double rating, string comment)
        {
            try
            {
                if (Session["KhachHangID"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để đánh giá!" });
                }

                var khachHangId = (int)Session["KhachHangID"];
                var sanPham = db.SanPham.Find(productId);

                if (sanPham == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
                }

                if (rating < 1 || rating > 5)
                {
                    return Json(new { success = false, message = "Đánh giá phải từ 1 đến 5 sao!" });
                }
                if (string.IsNullOrWhiteSpace(comment))
                {
                    return Json(new { success = false, message = "Vui lòng nhập bình luận!" });
                }

                var existingReview = db.DanhGia.FirstOrDefault(dg => dg.KhachHangID == khachHangId && dg.SanPhamID == productId);
                if (existingReview != null)
                {
                    return Json(new { success = false, message = "Bạn đã đánh giá sản phẩm này rồi!" });
                }

                var danhGia = new DanhGia
                {
                    KhachHangID = khachHangId,
                    SanPhamID = productId,
                    Diem = rating,
                    NhanXet = comment,
                    NgayDanhGia = DateTime.Now,
                    ThoiGianGui = DateTime.Now
                };

                db.DanhGia.Add(danhGia);
                db.SaveChanges();

                var danhGiaList = db.DanhGia.Where(dg => dg.SanPhamID == productId).ToList();
                if (danhGiaList.Any())
                {
                    sanPham.DanhGia = danhGiaList.Average(dg => dg.Diem ?? 0);
                    db.Entry(sanPham).State = EntityState.Modified;
                    db.SaveChanges();
                }

                var khachHang = db.KhachHang.Find(khachHangId);
                var reviewData = new
                {
                    tenKhachHang = khachHang != null ? khachHang.TenKhachHang : "Người dùng ẩn",
                    diem = danhGia.Diem,
                    ngayDanhGia = danhGia.NgayDanhGia.Value.ToString("dd/MM/yyyy HH:mm"),
                    nhanXet = danhGia.NhanXet
                };

                return Json(new { success = true, message = "Đánh giá đã được gửi thành công!", review = reviewData });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi gửi đánh giá: " + ex.Message);
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // POST: MayTinh/AddToCart
        [HttpPost]
        public ActionResult AddToCart(int productId)
        {
            int? khachHangId = (int?)Session["KhachHangID"];
            if (!khachHangId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng.", redirect = Url.Action("Login", "MayTinh", new { returnUrl = Request.UrlReferrer?.ToString() }) });
            }

            var existingItem = db.GioHang.FirstOrDefault(gh => gh.KhachHangID == khachHangId.Value && gh.SanPhamID == productId);
            if (existingItem != null)
            {
                existingItem.SoLuong += 1;
            }
            else
            {
                var gioHangItem = new GioHang
                {
                    KhachHangID = khachHangId.Value,
                    SanPhamID = productId,
                    SoLuong = 1
                };
                db.GioHang.Add(gioHangItem);
            }
            db.SaveChanges();

            // Cập nhật số lượng sản phẩm trong giỏ hàng
            var soLuongSanPham = db.GioHang.Where(gh => gh.KhachHangID == khachHangId.Value)
                                           .Select(gh => gh.SoLuong)
                                           .DefaultIfEmpty(0)
                                           .Sum();
            Session["soluongsanpham"] = soLuongSanPham;

            return Json(new { success = true, message = "Đã thêm vào giỏ hàng thành công!" });
        }

        // POST: MayTinh/AddMultipleToCart
        [HttpPost]
        public ActionResult AddMultipleToCart(List<int> productIds)
        {
            try
            {
                // Kiểm tra đăng nhập
                int? khachHangId = (int?)Session["KhachHangID"];
                if (!khachHangId.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng.", redirect = Url.Action("Login", "MayTinh", new { returnUrl = Request.UrlReferrer?.ToString() }) });
                }

                int addedCount = 0;

                // Thêm từng sản phẩm vào giỏ hàng
                foreach (var productId in productIds)
                {
                    // Kiểm tra sản phẩm tồn tại
                    var sanPham = db.SanPham.FirstOrDefault(s => s.SanPhamID == productId);
                    if (sanPham == null || sanPham.SoLuongKho <= 0)
                    {
                        continue; // Bỏ qua sản phẩm không tồn tại hoặc hết hàng
                    }

                    // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
                    var existingItem = db.GioHang.FirstOrDefault(gh => gh.KhachHangID == khachHangId.Value && gh.SanPhamID == productId);
                    if (existingItem != null)
                    {
                        // Nếu đã có, tăng số lượng
                        existingItem.SoLuong += 1;
                    }
                    else
                    {
                        // Nếu chưa có, thêm mới
                        var gioHangItem = new GioHang
                        {
                            KhachHangID = khachHangId.Value,
                            SanPhamID = productId,
                            SoLuong = 1
                        };
                        db.GioHang.Add(gioHangItem);
                    }

                    addedCount++;
                }

                db.SaveChanges();

                // Cập nhật số lượng sản phẩm trong giỏ hàng
                var soLuongSanPham = db.GioHang.Where(gh => gh.KhachHangID == khachHangId.Value)
                                               .Select(gh => gh.SoLuong)
                                               .DefaultIfEmpty(0)
                                               .Sum();
                Session["soluongsanpham"] = soLuongSanPham;

                return Json(new { success = true, message = $"Đã thêm {addedCount} sản phẩm vào giỏ hàng." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in AddMultipleToCart: " + ex.ToString());
                return Json(new { success = false, message = "Đã xảy ra lỗi khi thêm sản phẩm vào giỏ hàng: " + ex.Message });
            }
        }

        // GET: MayTinh/GioiThieu
        public ActionResult GioiThieu()
        {
            return View();
        }

        // GET: MayTinh/LienHe
        public ActionResult LienHe()
        {
            return View();
        }

        // GET: MayTinh/Login
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: MayTinh/Login
        [HttpPost]
        public ActionResult Login(string email, string matKhau, string returnUrl)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập email";
                return View();
            }

            if (string.IsNullOrEmpty(matKhau))
            {
                ViewBag.Error = "Vui lòng nhập mật khẩu";
                return View();
            }

            if (!IsValidEmail(email))
            {
                ViewBag.Error = "Email không hợp lệ";
                return View();
            }

            var khachHang = db.KhachHang.FirstOrDefault(kh => kh.Email == email);
            if (khachHang == null)
            {
                ViewBag.Error = "Email chưa tồn tại, vui lòng kiểm tra lại";
                return View();
            }

            if (khachHang.ThoiGianKhoa.HasValue && khachHang.ThoiGianKhoa > DateTime.Now)
            {
                ViewBag.Error = "Bạn đã nhập sai quá nhiều lần, vui lòng thử lại sau hoặc đặt lại mật khẩu";
                return View();
            }

            string hashedMatKhau = HashPassword(matKhau);
            if (khachHang.MatKhau != hashedMatKhau)
            {
                khachHang.SoLanDangNhapSai++;
                if (khachHang.SoLanDangNhapSai >= 5)
                {
                    khachHang.ThoiGianKhoa = DateTime.Now.AddMinutes(15);
                    ViewBag.Error = "Bạn đã nhập sai quá nhiều lần, vui lòng thử lại sau hoặc đặt lại mật khẩu";
                }
                else
                {
                    ViewBag.Error = "Mật khẩu không chính xác";
                }

                db.SaveChanges();
                return View();
            }

            khachHang.SoLanDangNhapSai = 0;
            khachHang.ThoiGianKhoa = null;
            db.SaveChanges();

            // Lưu thông tin session
            Session["KhachHangID"] = khachHang.KhachHangID;
            Session["TenKhachHang"] = khachHang.TenKhachHang;
            Session["LaAdmin"] = khachHang.LaAdmin;

            // Cập nhật số lượng sản phẩm trong giỏ hàng
            var soLuongSanPham = db.GioHang.Where(gh => gh.KhachHangID == khachHang.KhachHangID)
                                           .Select(gh => gh.SoLuong)
                                           .DefaultIfEmpty(0)
                                           .Sum();
            Session["soluongsanpham"] = soLuongSanPham;

            // Chuyển hướng dựa trên vai trò
            if (khachHang.LaAdmin.HasValue && khachHang.LaAdmin.Value)
            {
                Session["AdminName"] = khachHang.TenKhachHang;
                return RedirectToAction("Index", "Admin");
            }
            else
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index");
            }
        }

        // GET: MayTinh/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: MayTinh/Register
        [HttpPost]
        public ActionResult Register(KhachHang khachHang, string xacNhanMatKhau)
        {
            if (string.IsNullOrEmpty(khachHang.TenKhachHang))
            {
                ViewBag.Error = "Vui lòng nhập họ và tên";
                return View();
            }

            if (string.IsNullOrEmpty(khachHang.Email))
            {
                ViewBag.Error = "Vui lòng nhập email";
                return View();
            }

            if (string.IsNullOrEmpty(khachHang.MatKhau))
            {
                ViewBag.Error = "Vui lòng nhập mật khẩu";
                return View();
            }

            if (string.IsNullOrEmpty(xacNhanMatKhau))
            {
                ViewBag.Error = "Vui lòng nhập lại mật khẩu";
                return View();
            }

            if (!IsValidEmail(khachHang.Email))
            {
                ViewBag.Error = "Email không hợp lệ";
                return View();
            }

            string passwordError = ValidatePassword(khachHang.MatKhau);
            if (!string.IsNullOrEmpty(passwordError))
            {
                ViewBag.Error = passwordError;
                return View();
            }

            if (khachHang.MatKhau != xacNhanMatKhau)
            {
                ViewBag.Error = "Mật khẩu nhập lại không khớp";
                return View();
            }

            if (db.KhachHang.Any(kh => kh.Email == khachHang.Email))
            {
                ViewBag.Error = "Email đã tồn tại, vui lòng chọn email khác";
                return View();
            }

            string hashedPassword = HashPassword(khachHang.MatKhau);
            khachHang.MatKhau = hashedPassword;
            khachHang.LaAdmin = false;
            khachHang.NgayCapNhat = DateTime.Now;
            khachHang.SoLanDangNhapSai = 0;
            khachHang.ThoiGianKhoa = null;

            khachHang.KhachHangID = db.KhachHang.Any() ? db.KhachHang.Max(kh => kh.KhachHangID) + 1 : 1;
            db.KhachHang.Add(khachHang);
            db.SaveChanges();

            Session["KhachHangID"] = khachHang.KhachHangID;
            Session["TenKhachHang"] = khachHang.TenKhachHang;
            Session["LaAdmin"] = false;
            Session["soluongsanpham"] = 0; // Khởi tạo số lượng sản phẩm trong giỏ hàng

            return RedirectToAction("Index");
        }

        // GET: MayTinh/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index");
        }

        // Helper method to validate email
        private bool IsValidEmail(string email)
        {
            string emailPattern = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";
            return !string.IsNullOrEmpty(email) && Regex.IsMatch(email, emailPattern);
        }

        // Helper method to validate password
        private string ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return "Vui lòng nhập mật khẩu";
            if (password.Length < 8)
                return "Mật khẩu phải có ít nhất 8 ký tự";
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return "Mật khẩu phải chứa ít nhất một chữ cái viết hoa";
            if (!Regex.IsMatch(password, @"[0-9]"))
                return "Mật khẩu phải chứa ít nhất một số";
            if (!Regex.IsMatch(password, @"[@#$%^&+=!]"))
                return "Mật khẩu phải chứa ít nhất một ký tự đặc biệt";
            return string.Empty;
        }

        // Helper method to get customer name
        private string GetTenKhachHang(int khachHangID)
        {
            var khachHang = db.KhachHang.Find(khachHangID);
            if (khachHang != null)
            {
                return khachHang.TenKhachHang;
            }
            return string.Empty;
        }

        // Helper method to hash password
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        // GET: MayTinh/GetProductDetails
        [HttpGet]
        public ActionResult GetProductDetails(int id)
        {
            try
            {
                var sanPham = db.SanPham
                    .Include(sp => sp.HinhAnhSanPham)
                    .FirstOrDefault(sp => sp.SanPhamID == id);

                if (sanPham == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm" }, JsonRequestBehavior.AllowGet);
                }

                // Lấy hình ảnh chính
                var hinhAnhChinh = sanPham.HinhAnhSanPham
                    .FirstOrDefault(ha => ha.LaHinhChinh == true);

                string hinhAnhUrl = hinhAnhChinh != null && !string.IsNullOrEmpty(hinhAnhChinh.DuongDanAnh)
                    ? hinhAnhChinh.DuongDanAnh
                    : "/images/default.jpg";

                var productDetails = new
                {
                    success = true,
                    sanPhamID = sanPham.SanPhamID,
                    tenSP = sanPham.TenSP,
                    gia = sanPham.Gia,
                    giaFormatted = string.Format("{0:N0}", sanPham.Gia) + " đ",
                    moTa = sanPham.MoTa ?? "Không có mô tả",
                    // Sửa các dòng sau để kiểm tra kiểu dữ liệu
                    soLuongKho = sanPham.SoLuongKho, // Nếu là int thì không cần ??
                    danhGia = sanPham.DanhGia, // Nếu là int thì không cần ??
                    luotXem = sanPham.LuotXem, // Nếu là int thì không cần ??
                    hinhAnh = hinhAnhUrl
                };

                return Json(productDetails, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetProductDetails: " + ex.ToString());
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Dispose method
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
