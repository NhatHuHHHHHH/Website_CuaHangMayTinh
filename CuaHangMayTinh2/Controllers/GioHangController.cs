using System;
using System.Linq;
using System.Web.Mvc;
using CuaHangMayTinh2.Models;
using System.Data.Entity;
using System.Data.SqlClient;

namespace CuaHangMayTinh2.Controllers
{
    public class GioHangController : Controller
    {
        private CuaHangMayTinh2Entities db = new CuaHangMayTinh2Entities();

        // GET: GioHang/CheckoutForm
        [AuthorizeUser]
        public ActionResult CheckoutForm()
        {
            int khachHangId = Convert.ToInt32(Session["KhachHangID"]);
            var gioHangList = db.GioHang
                .Include(gh => gh.SanPham)
                .Where(gh => gh.KhachHangID == khachHangId)
                .ToList();

            if (!gioHangList.Any())
            {
                TempData["Error"] = "Giỏ hàng trống!";
                return RedirectToAction("Index");
            }

            return View(gioHangList);
        }

        // POST: GioHang/Checkout
        [HttpPost]
        [AuthorizeUser]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(string diaChiGiaoHang, string phuongThucThanhToan)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (Session["KhachHangID"] == null)
                {
                    TempData["Error"] = "Vui lòng đăng nhập!";
                    return RedirectToAction("Login", "MayTinh", new { returnUrl = Url.Action("CheckoutForm", "GioHang") });
                }

                int khachHangId = Convert.ToInt32(Session["KhachHangID"]);
                // Kiểm tra KhachHang tồn tại
                var khachHang = db.KhachHang.Find(khachHangId);
                if (khachHang == null)
                {
                    TempData["Error"] = "Khách hàng không tồn tại.";
                    return RedirectToAction("CheckoutForm");
                }

                var gioHangList = db.GioHang
                    .Include(gh => gh.SanPham)
                    .Where(gh => gh.KhachHangID == khachHangId)
                    .ToList();

                // Kiểm tra giỏ hàng rỗng
                if (!gioHangList.Any())
                {
                    TempData["Error"] = "Giỏ hàng trống!";
                    return RedirectToAction("Index");
                }

                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrEmpty(diaChiGiaoHang) || string.IsNullOrEmpty(phuongThucThanhToan))
                {
                    TempData["Error"] = "Vui lòng nhập đầy đủ thông tin thanh toán.";
                    return RedirectToAction("CheckoutForm");
                }

                // Kiểm tra sản phẩm và số lượng kho
                foreach (var item in gioHangList)
                {
                    if (item.SanPham == null)
                    {
                        TempData["Error"] = "Sản phẩm với ID " + item.SanPhamID + " không tồn tại.";
                        return RedirectToAction("CheckoutForm");
                    }

                    if (item.SanPham.SoLuongKho < item.SoLuong)
                    {
                        TempData["Error"] = "Sản phẩm " + item.SanPham.TenSP + " chỉ còn " + item.SanPham.SoLuongKho + " trong kho, không đủ để thanh toán.";
                        return RedirectToAction("CheckoutForm");
                    }

                    // Kiểm tra số lượng kho sau khi trừ
                    if (item.SanPham.SoLuongKho - item.SoLuong < 0)
                    {
                        TempData["Error"] = "Số lượng kho không đủ sau khi kiểm tra lại cho sản phẩm " + item.SanPham.TenSP + ".";
                        return RedirectToAction("CheckoutForm");
                    }

                    // Kiểm tra SoLuong và Gia không null
                    if (item.SoLuong <= 0)
                    {
                        TempData["Error"] = "Số lượng của sản phẩm " + item.SanPham.TenSP + " không hợp lệ.";
                        return RedirectToAction("CheckoutForm");
                    }

                    if (item.SanPham.Gia <= 0)
                    {
                        TempData["Error"] = "Giá của sản phẩm " + item.SanPham.TenSP + " không hợp lệ.";
                        return RedirectToAction("CheckoutForm");
                    }
                }

                // Sử dụng giao dịch để đảm bảo tính toàn vẹn dữ liệu
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Tạo mã đơn hàng duy nhất
                        string maDonHang;
                        int maxAttempts = 10;
                        int attempt = 0;
                        do
                        {
                            if (attempt >= maxAttempts)
                            {
                                throw new Exception("Không thể tạo mã đơn hàng duy nhất sau nhiều lần thử.");
                            }
                            maDonHang = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss") + new Random().Next(1000, 9999);
                            attempt++;
                        } while (db.DonHang.Any(dh => dh.MaDonHang == maDonHang));

                        // Tính tổng tiền
                        decimal tongTien = gioHangList.Sum(gh => gh.SanPham.Gia * gh.SoLuong);

                        // Tạo đơn hàng mới
                        var donHang = new DonHang
                        {
                            KhachHangID = khachHangId,
                            NgayDatHang = DateTime.Now,
                            TongTien = tongTien,
                            TrangThaiDonHang = "Pending",
                            PhuongThucThanhToan = phuongThucThanhToan,
                            DiaChiGiaoHang = diaChiGiaoHang,
                            MaDonHang = maDonHang,
                            NgayCapNhat = DateTime.Now
                        };

                        System.Diagnostics.Debug.WriteLine("Thêm DonHang: " + donHang.MaDonHang);
                        db.DonHang.Add(donHang);
                        db.SaveChanges(); // Lưu đơn hàng để lấy DonHangID
                        System.Diagnostics.Debug.WriteLine("DonHangID: " + donHang.DonHangID);

                        // Tạo chi tiết đơn hàng
                        foreach (var item in gioHangList)
                        {
                            var chiTietDonHang = new ChiTietDonHang
                            {
                                DonHangID = donHang.DonHangID,
                                SanPhamID = item.SanPhamID.Value,
                                SoLuong = item.SoLuong,
                                Gia = item.SanPham.Gia
                            };
                            System.Diagnostics.Debug.WriteLine("Thêm ChiTietDonHang: DonHangID=" + chiTietDonHang.DonHangID + ", SanPhamID=" + chiTietDonHang.SanPhamID);
                            db.ChiTietDonHang.Add(chiTietDonHang);

                            // Cập nhật số lượng kho
                            System.Diagnostics.Debug.WriteLine("Cập nhật SoLuongKho: SanPhamID=" + item.SanPhamID + ", SoLuongKho trước=" + item.SanPham.SoLuongKho);
                            item.SanPham.SoLuongKho -= item.SoLuong;
                            System.Diagnostics.Debug.WriteLine("SoLuongKho sau=" + item.SanPham.SoLuongKho);
                        }

                        // Xóa giỏ hàng
                        System.Diagnostics.Debug.WriteLine("Xóa GioHang: Số lượng bản ghi=" + gioHangList.Count);
                        db.GioHang.RemoveRange(gioHangList);
                        db.SaveChanges();

                        // Gọi stored procedure
                        try
                        {
                            System.Diagnostics.Debug.WriteLine("Gọi stored procedure CapNhatKhoDuaTrenDonHang: DonHangID=" + donHang.DonHangID);
                            db.Database.ExecuteSqlCommand("EXEC CapNhatKhoDuaTrenDonHang @DonHangID",
                                new SqlParameter("@DonHangID", donHang.DonHangID));
                            System.Diagnostics.Debug.WriteLine("Gọi stored procedure CapNhatSanPhamThuongMuaCung: DonHangID=" + donHang.DonHangID);
                            db.Database.ExecuteSqlCommand("EXEC CapNhatSanPhamThuongMuaCung @DonHangID, @SupportThreshold",
                                new SqlParameter("@DonHangID", donHang.DonHangID),
                                new SqlParameter("@SupportThreshold", 2));
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine("Error in stored procedure: " + ex.ToString());
                            TempData["Error"] = "Đã xảy ra lỗi khi gọi stored procedure: " + ex.Message;
                            transaction.Rollback();
                            return RedirectToAction("CheckoutForm");
                        }

                        // Commit giao dịch
                        transaction.Commit();

                        // Cập nhật số lượng sản phẩm trong session
                        Session["soluongsanpham"] = 0;

                        // Chuyển hướng đến PurchaseSuccess
                        return RedirectToAction("PurchaseSuccess", "DonHang", new { donHangId = donHang.DonHangID });
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        var innerException = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        System.Diagnostics.Debug.WriteLine("Error in Checkout: " + ex.ToString());
                        System.Diagnostics.Debug.WriteLine("Inner Exception: " + innerException);
                        TempData["Error"] = "Đã xảy ra lỗi: " + innerException;
                        return RedirectToAction("CheckoutForm");
                    }
                }
            }
            catch (Exception ex)
            {
                var innerException = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                System.Diagnostics.Debug.WriteLine("Error in Checkout: " + ex.ToString());
                System.Diagnostics.Debug.WriteLine("Inner Exception: " + innerException);
                TempData["Error"] = "Đã xảy ra lỗi: " + innerException;
                return RedirectToAction("CheckoutForm");
            }
        }

        // POST: GioHang/RemoveItemFromCart
        [HttpPost]
        [AuthorizeUser]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveItemFromCart(int gioHangId)
        {
            System.Diagnostics.Debug.WriteLine("RemoveItemFromCart called with ID: " + gioHangId);

            try
            {
                if (Session["KhachHangID"] == null)
                {
                    TempData["Error"] = "Vui lòng đăng nhập!";
                    return RedirectToAction("Login", "MayTinh", new { returnUrl = Url.Action("Index", "GioHang") });
                }

                int khachHangId = Convert.ToInt32(Session["KhachHangID"]);
                var gioHangItem = db.GioHang.FirstOrDefault(gh => gh.GioHangID == gioHangId && gh.KhachHangID == khachHangId);
                if (gioHangItem == null)
                {
                    System.Diagnostics.Debug.WriteLine("Item not found or does not belong to user: KhachHangID=" + khachHangId + ", GioHangID=" + gioHangId);
                    TempData["Error"] = "Sản phẩm không tồn tại trong giỏ hàng hoặc không thuộc về bạn!";
                    return RedirectToAction("Index");
                }

                db.GioHang.Remove(gioHangItem);
                db.SaveChanges();

                var gioHangList = db.GioHang.Where(gh => gh.KhachHangID == khachHangId).ToList();
                Session["soluongsanpham"] = gioHangList.Sum(gh => gh.SoLuong);

                System.Diagnostics.Debug.WriteLine("Item removed successfully: GioHangID=" + gioHangId);
                TempData["Message"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in RemoveItemFromCart: " + ex.ToString());
                TempData["Error"] = "Đã xảy ra lỗi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: GioHang/Index
        [AuthorizeUser]
        public ActionResult Index()
        {
            int khachHangId = Convert.ToInt32(Session["KhachHangID"]);
            var gioHangList = db.GioHang
                .Include(gh => gh.SanPham)
                .Where(gh => gh.KhachHangID == khachHangId)
                .ToList();
            return View(gioHangList);
        }

        // POST: GioHang/AddToCart
        [HttpPost]
        [AuthorizeUser]
        public ActionResult AddToCart(int id)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (Session["KhachHangID"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng!", redirect = Url.Action("Login", "MayTinh", new { returnUrl = Url.Action("Index", "MayTinh") }) });
                }

                int khachHangId = Convert.ToInt32(Session["KhachHangID"]);

                // Kiểm tra sản phẩm có tồn tại không
                var sanPham = db.SanPham.Find(id);
                if (sanPham == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
                }

                // Kiểm tra số lượng kho
                if (sanPham.SoLuongKho <= 0)
                {
                    return Json(new { success = false, message = "Sản phẩm đã hết hàng!" });
                }

                // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
                var existingItem = db.GioHang.FirstOrDefault(gh => gh.KhachHangID == khachHangId && gh.SanPhamID == id);
                if (existingItem != null)
                {
                    // Nếu sản phẩm đã có trong giỏ, tăng số lượng
                    existingItem.SoLuong += 1;
                    // Kiểm tra số lượng kho
                    if (existingItem.SoLuong > sanPham.SoLuongKho)
                    {
                        return Json(new { success = false, message = "Số lượng trong kho không đủ! Chỉ còn " + sanPham.SoLuongKho + " sản phẩm." });
                    }
                }
                else
                {
                    // Nếu sản phẩm chưa có trong giỏ, thêm mới
                    var gioHangItem = new GioHang
                    {
                        KhachHangID = khachHangId,
                        SanPhamID = id,
                        SoLuong = 1
                    };
                    db.GioHang.Add(gioHangItem);
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();

                // Cập nhật số lượng sản phẩm trong session
                var gioHangList = db.GioHang.Where(gh => gh.KhachHangID == khachHangId).ToList();
                Session["soluongsanpham"] = gioHangList.Sum(gh => gh.SoLuong);

                return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng!", totalItems = Session["soluongsanpham"] });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in AddToCart: " + ex.ToString());
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // POST: GioHang/UpdateCart
        [HttpPost]
        [AuthorizeUser]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCart(int gioHangId, int soLuong)
        {
            try
            {
                if (Session["KhachHangID"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập!" });
                }

                int khachHangId = Convert.ToInt32(Session["KhachHangID"]);
                var gioHangItem = db.GioHang.FirstOrDefault(gh => gh.GioHangID == gioHangId && gh.KhachHangID == khachHangId);
                if (gioHangItem == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại trong giỏ hàng!" });
                }

                var sanPham = db.SanPham.Find(gioHangItem.SanPhamID);
                if (sanPham == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
                }

                if (soLuong > sanPham.SoLuongKho)
                {
                    return Json(new { success = false, message = "Số lượng vượt quá số lượng trong kho! Chỉ còn " + sanPham.SoLuongKho + " sản phẩm." });
                }

                if (soLuong < 1)
                {
                    return Json(new { success = false, message = "Số lượng phải lớn hơn 0!" });
                }

                gioHangItem.SoLuong = soLuong;
                db.SaveChanges();

                var gioHangList = db.GioHang.Where(gh => gh.KhachHangID == khachHangId).ToList();
                Session["soluongsanpham"] = gioHangList.Sum(gh => gh.SoLuong);

                return Json(new { success = true, message = "Cập nhật số lượng thành công!", totalItems = Session["soluongsanpham"] });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in UpdateCart: " + ex.ToString());
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult AddToCartWithQuantity(int productId, int quantity)
        {
            try
            {
                // Kiểm tra đăng nhập
                if (Session["KhachHangID"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng!", redirect = Url.Action("Login", "MayTinh", new { returnUrl = Url.Action("Index", "MayTinh") }) });
                }

                int khachHangId = Convert.ToInt32(Session["KhachHangID"]);

                // Kiểm tra sản phẩm có tồn tại không
                var sanPham = db.SanPham.Find(productId);
                if (sanPham == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
                }

                // Kiểm tra số lượng kho
                if (sanPham.SoLuongKho < quantity)
                {
                    return Json(new { success = false, message = $"Số lượng vượt quá tồn kho! Chỉ còn {sanPham.SoLuongKho} sản phẩm." });
                }

                // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
                var existingItem = db.GioHang.FirstOrDefault(gh => gh.KhachHangID == khachHangId && gh.SanPhamID == productId);
                if (existingItem != null)
                {
                    // Nếu sản phẩm đã có trong giỏ, tăng số lượng
                    existingItem.SoLuong += quantity;
                }
                else
                {
                    // Nếu sản phẩm chưa có trong giỏ, thêm mới
                    var gioHangItem = new GioHang
                    {
                        KhachHangID = khachHangId,
                        SanPhamID = productId,
                        SoLuong = quantity
                    };
                    db.GioHang.Add(gioHangItem);
                }

                // Lưu thay đổi vào cơ sở dữ liệu
                db.SaveChanges();

                // Cập nhật số lượng sản phẩm trong session
                var gioHangList = db.GioHang.Where(gh => gh.KhachHangID == khachHangId).ToList();
                Session["soluongsanpham"] = gioHangList.Sum(gh => gh.SoLuong);

                return Json(new { success = true, message = $"Đã thêm {quantity} sản phẩm vào giỏ hàng!", totalItems = Session["soluongsanpham"] });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in AddToCartWithQuantity: " + ex.ToString());
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

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