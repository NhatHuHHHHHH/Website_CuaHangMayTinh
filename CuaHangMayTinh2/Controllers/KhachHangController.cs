using System;
using System.Linq;
using System.Web.Mvc;
using CuaHangMayTinh2.Models;
using System.Data.Entity;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace CuaHangMayTinh2.Controllers
{
    public class KhachHangController : Controller
    {
        private CuaHangMayTinh2Entities db = new CuaHangMayTinh2Entities();

        // GET: Hiển thị thông tin tài khoản khách hàng
        public ActionResult ThongTin()
        {
            if (Session["KhachHangID"] == null)
            {
                return RedirectToAction("Login", "MayTinh");
            }

            int khachHangId = (int)Session["KhachHangID"];
            var khachHang = db.KhachHang.Find(khachHangId);

            if (khachHang == null)
            {
                return HttpNotFound("Không tìm thấy thông tin khách hàng.");
            }

            return View(khachHang);
        }

        // POST: Cập nhật thông tin khách hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Update(KhachHang khachHang)
        {
            try
            {
                if (Session["KhachHangID"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện thao tác này." });
                }

                ModelState.Remove("MatKhau");
                if (ModelState.IsValid)
                {
                    var existingKhachHang = db.KhachHang.Find(khachHang.KhachHangID);
                    if (existingKhachHang == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy khách hàng." });
                    }

                    int currentKhachHangId = (int)Session["KhachHangID"];
                    if (existingKhachHang.KhachHangID != currentKhachHangId)
                    {
                        return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa thông tin này." });
                    }

                    existingKhachHang.TenKhachHang = khachHang.TenKhachHang;
                    existingKhachHang.Email = khachHang.Email;
                    existingKhachHang.SoDienThoai = khachHang.SoDienThoai;
                    existingKhachHang.DiaChi = khachHang.DiaChi;
                    existingKhachHang.NgayCapNhat = DateTime.Now;

                    db.Entry(existingKhachHang).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new
                    {
                        success = true,
                        message = "Cập nhật thông tin thành công!"
                    });
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ: " + string.Join(", ", errors) });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi cập nhật khách hàng: " + ex.ToString());
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // GET: Hiển thị giao diện đổi mật khẩu
        public ActionResult ChangePassword()
        {
            if (Session["KhachHangID"] == null)
            {
                return RedirectToAction("Login", "MayTinh", new { returnUrl = Url.Action("ChangePassword", "KhachHang") });
            }

            return View();
        }

        // POST: Thay đổi mật khẩu (dòng 104)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string oldPassword, string newPassword, string confirmNewPassword)
        {
            if (Session["KhachHangID"] == null)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để thực hiện thao tác này.";
                return RedirectToAction("Login", "MayTinh", new { returnUrl = Url.Action("ChangePassword", "KhachHang") });
            }

            int khachHangId = (int)Session["KhachHangID"];
            var khachHang = db.KhachHang.Find(khachHangId);

            if (khachHang == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng.";
                return RedirectToAction("ChangePassword");
            }

            // Kiểm tra mật khẩu cũ
            string hashedOldPassword = HashPassword(oldPassword);
            if (khachHang.MatKhau != hashedOldPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu cũ không chính xác.";
                return RedirectToAction("ChangePassword");
            }

            // Kiểm tra mật khẩu mới và xác nhận mật khẩu mới
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmNewPassword))
            {
                TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không được để trống.";
                return RedirectToAction("ChangePassword");
            }

            if (newPassword != confirmNewPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                return RedirectToAction("ChangePassword");
            }

            // Kiểm tra độ mạnh của mật khẩu mới
            string passwordError = ValidatePassword(newPassword);
            if (!string.IsNullOrEmpty(passwordError))
            {
                TempData["ErrorMessage"] = passwordError;
                return RedirectToAction("ChangePassword");
            }

            try
            {
                // Cập nhật mật khẩu mới
                khachHang.MatKhau = HashPassword(newPassword);
                khachHang.NgayCapNhat = DateTime.Now;
                khachHang.SoLanDangNhapSai = 0; // Reset số lần đăng nhập sai
                khachHang.ThoiGianKhoa = null;  // Bỏ khóa tài khoản nếu có

                db.Entry(khachHang).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = "Thay đổi mật khẩu thành công!";
                return RedirectToAction("ChangePassword");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi thay đổi mật khẩu: " + ex.ToString());
                TempData["ErrorMessage"] = "Đã xảy ra lỗi: " + ex.Message;
                return RedirectToAction("ChangePassword");
            }
        }

        // GET: Hiển thị danh sách sản phẩm yêu thích
        public ActionResult SanPhamYeuThich()
        {
            if (Session["KhachHangID"] == null)
            {
                return RedirectToAction("Login", "MayTinh", new { returnUrl = Url.Action("SanPhamYeuThich", "KhachHang") });
            }

            int khachHangId = (int)Session["KhachHangID"];
            var sanPhamYeuThichList = db.SanPhamYeuThich
                .Include(spyt => spyt.SanPham)
                .Where(spyt => spyt.KhachHangID == khachHangId)
                .Select(spyt => spyt.SanPham)
                .ToList();

            return View(sanPhamYeuThichList);
        }

        // POST: Thêm hoặc xóa sản phẩm khỏi danh sách yêu thích
        [HttpPost]
        public JsonResult ToggleWishlist(int sanPhamId)
        {
            try
            {
                if (Session["KhachHangID"] == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thêm sản phẩm vào danh sách yêu thích.", redirect = Url.Action("Login", "MayTinh", new { returnUrl = Request.UrlReferrer?.ToString() }) });
                }

                int khachHangId = (int)Session["KhachHangID"];
                var existing = db.SanPhamYeuThich
                    .FirstOrDefault(spyt => spyt.KhachHangID == khachHangId && spyt.SanPhamID == sanPhamId);

                if (existing != null)
                {
                    // Nếu sản phẩm đã có trong danh sách yêu thích, xóa nó
                    db.SanPhamYeuThich.Remove(existing);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Đã xóa sản phẩm khỏi danh sách yêu thích.", liked = false });
                }
                else
                {
                    // Nếu sản phẩm chưa có, thêm vào danh sách yêu thích
                    var sanPhamYeuThich = new SanPhamYeuThich
                    {
                        KhachHangID = khachHangId,
                        SanPhamID = sanPhamId,
                        NgayThem = DateTime.Now
                    };
                    db.SanPhamYeuThich.Add(sanPhamYeuThich);
                    db.SaveChanges();
                    return Json(new { success = true, message = "Đã thêm sản phẩm vào danh sách yêu thích.", liked = true });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Lỗi khi xử lý yêu thích: " + ex.ToString());
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }

        // GET: KhachHang/MyOrders
        [AuthorizeUser]
        public ActionResult MyOrders()
        {
            // Kiểm tra đăng nhập
            if (Session["KhachHangID"] == null)
            {
                return RedirectToAction("Login", "MayTinh", new { returnUrl = Url.Action("MyOrders", "KhachHang") });
            }

            int khachHangId = Convert.ToInt32(Session["KhachHangID"]);

            // Lấy danh sách đơn hàng của khách hàng hiện tại, bao gồm chi tiết đơn hàng và sản phẩm
            var donHangList = db.DonHang
                .Include(dh => dh.ChiTietDonHang.Select(ct => ct.SanPham))
                .Where(dh => dh.KhachHangID == khachHangId)
                .OrderByDescending(dh => dh.NgayDatHang)
                .ToList();

            return View(donHangList);
        }

        // Hàm kiểm tra độ mạnh mật khẩu
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

        // Hàm mã hóa mật khẩu
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

        // Giải phóng tài nguyên
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