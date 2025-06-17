using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CuaHangMayTinh2.Models;
using System.Data.Entity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace CuaHangMayTinh2.Controllers
{
    public class SanPhamController : Controller
    {
        private CuaHangMayTinh2Entities db = new CuaHangMayTinh2Entities();

        // GET: SanPham/TimKiem
        public ActionResult TimKiem(string tuKhoa)
        {
            // Khởi tạo mặc định để tránh lỗi null
            ViewBag.TuKhoa = tuKhoa ?? "";
            ViewBag.GoiYSanPham = new List<SanPham>();
            ViewBag.HinhAnhChinhDict = new Dictionary<int, string>();

            if (string.IsNullOrEmpty(tuKhoa))
            {
                return View(new List<SanPham>());
            }

            // Tìm kiếm sản phẩm theo TenSP (không phân biệt hoa thường)
            var sanPhams = db.SanPham
                .Where(s => s.TenSP.ToLower().Contains(tuKhoa.ToLower()))
                .ToList();

            // Lấy danh sách sản phẩm thường mua cùng để gợi ý
            var allSanPhamIds = sanPhams.Select(sp => sp.SanPhamID).ToList();
            var goiYSanPham = new List<SanPham>();
            if (allSanPhamIds.Any())
            {
                goiYSanPham = (from sptmc in db.SanPhamThuongMuaCung
                               where allSanPhamIds.Contains(sptmc.SanPhamID1)
                               orderby sptmc.SoLanMuaCung descending
                               select sptmc.SanPhamID2)
                              .Distinct()
                              .Take(3)
                              .Join(db.SanPham,
                                    sanPhamID2 => sanPhamID2,
                                    sp => sp.SanPhamID,
                                    (sanPhamID2, sp) => sp)
                              .ToList();
            }

            // Lấy hình ảnh chính cho các sản phẩm tìm kiếm và gợi ý
            var combinedSanPhamIds = allSanPhamIds.Union(goiYSanPham.Select(sp => sp.SanPhamID)).ToList();
            var hinhAnhChinhDict = new Dictionary<int, string>();
            if (combinedSanPhamIds.Any())
            {
                hinhAnhChinhDict = db.HinhAnhSanPham
                    .Where(ha => ha.SanPhamID.HasValue && combinedSanPhamIds.Contains(ha.SanPhamID.Value) && ha.LaHinhChinh == true)
                    .ToDictionary(
                        ha => ha.SanPhamID.Value,
                        ha => ha.DuongDanAnh ?? "/Images/default-product.jpg",
                        EqualityComparer<int>.Default
                    );
            }

            // Đảm bảo tất cả sản phẩm đều có hình ảnh trong dictionary
            foreach (var sp in sanPhams.Concat(goiYSanPham))
            {
                if (!hinhAnhChinhDict.ContainsKey(sp.SanPhamID))
                {
                    hinhAnhChinhDict[sp.SanPhamID] = "/Images/default-product.jpg";
                }
            }

            ViewBag.GoiYSanPham = goiYSanPham;
            ViewBag.HinhAnhChinhDict = hinhAnhChinhDict;
            return View(sanPhams);
        }

        // GET: SanPham/TimKiemGoiY
        public ActionResult TimKiemGoiY(string tuKhoa)
        {
            try
            {
                if (string.IsNullOrEmpty(tuKhoa))
                {
                    return Json(new { success = true, ketQua = new object[] { } }, JsonRequestBehavior.AllowGet);
                }

                // Bước 1: Tìm sản phẩm theo từ khóa
                var ketQuaGoc = db.SanPham
                    .Where(s => s.TenSP.ToLower().Contains(tuKhoa.ToLower()))
                    .Take(5)
                    .ToList();

                // Danh sách ID sản phẩm từ khóa
                var idGoc = ketQuaGoc.Select(s => s.SanPhamID).ToList();

                // Bước 2: Gợi ý từ Apriori: tìm các sản phẩm hay được mua cùng các sản phẩm từ khóa
                var goiYTuApriori = new List<SanPham>();
                if (idGoc.Any())
                {
                    var idGoiY = db.SanPhamThuongMuaCung
                        .Where(m => idGoc.Contains(m.SanPhamID1))
                        .GroupBy(m => m.SanPhamID2)
                        .Select(g => new { SanPhamID = g.Key, TongSoLan = g.Sum(x => x.SoLanMuaCung) })
                        .OrderByDescending(g => g.TongSoLan)
                        .Take(5)
                        .Select(g => g.SanPhamID)
                        .ToList();

                    goiYTuApriori = db.SanPham
                        .Where(sp => idGoiY.Contains(sp.SanPhamID))
                        .ToList();
                }

                // Bước 3: Gộp và lọc trùng
                var allKetQua = ketQuaGoc.Concat(goiYTuApriori)
                    .GroupBy(sp => sp.SanPhamID)
                    .Select(g => g.First()) // loại trùng
                    .OrderByDescending(sp => (sp.DanhGia ?? 0) + (sp.LuotXem ?? 0) / 100.0)
                    .Take(10)
                    .Select(sp => new
                    {
                        sp.SanPhamID,
                        sp.TenSP,
                        sp.Gia,
                        sp.DanhGia,
                        DanhMuc = sp.DanhMuc != null ? sp.DanhMuc.TenDanhMuc : "Không xác định",
                        DuongDanAnh = db.HinhAnhSanPham
                            .Where(ha => ha.SanPhamID == sp.SanPhamID && ha.LaHinhChinh == true)
                            .Select(ha => ha.DuongDanAnh)
                            .FirstOrDefault() ?? "/Images/default-product.jpg"
                    })
                    .ToList();

                return Json(new { success = true, ketQua = allKetQua }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // 
        public ActionResult GetSearchSuggestions()
        {
            var searchHistory = Session["SearchHistory"] as List<string>;
            if (searchHistory == null || !searchHistory.Any())
                return Json(new List<SanPham>(), JsonRequestBehavior.AllowGet);

            // Lấy danh sách sản phẩm theo các từ khóa đã tìm
            var allMatches = db.SanPham
                .Where(sp => searchHistory.Any(kw => sp.TenSP.Contains(kw)))
                .OrderByDescending(sp => sp.DanhGia)
                .Take(10)
                .ToList();

            // Trả JSON cho AJAX gọi
            return Json(allMatches.Select(sp => new {
                id = sp.SanPhamID,
                tenSP = sp.TenSP,
                gia = sp.Gia,
                danhGia = sp.DanhGia,
                hinhAnh = db.HinhAnhSanPham
                    .FirstOrDefault(h => h.SanPhamID == sp.SanPhamID && h.LaHinhChinh == true)?.DuongDanAnh
                    ?? "/images/default.jpg"
            }), JsonRequestBehavior.AllowGet);
        }

        // 
        [HttpPost]
        public ActionResult LuuLichSuTimKiem(string tuKhoa)
        {
            try
            {
                var khachHangId = Session["KhachHangID"] as int?;

                if (khachHangId.HasValue && !string.IsNullOrEmpty(tuKhoa))
                {

                    var lichSu = new
                    {
                        KhachHangID = khachHangId.Value,
                        TuKhoa = tuKhoa,
                        ThoiGian = DateTime.Now
                    };

                    var searchHistory = Session["SearchHistory"] as List<string> ?? new List<string>();
                    if (!searchHistory.Contains(tuKhoa))
                    {
                        searchHistory.Add(tuKhoa);
                        if (searchHistory.Count > 10)
                        {
                            searchHistory.RemoveAt(0);
                        }
                        Session["SearchHistory"] = searchHistory;
                    }
                }

                return new HttpStatusCodeResult(200);
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(500, ex.Message);
            }
        }

        // GET: SanPham/SanPhamHot
        public ActionResult SanPhamHot()
        {
            try
            {
                // Lấy 6 sản phẩm có lượt xem cao nhất
                var sanPhamHot = db.SanPham
                    .OrderByDescending(s => s.LuotXem)
                    .ThenByDescending(s => s.DanhGia)
                    .Take(6)
                    .Select(s => new
                    {
                        s.SanPhamID,
                        s.TenSP,
                        s.Gia,
                        s.DanhGia,
                        s.LuotXem,
                        DuongDanAnh = (from ha in db.HinhAnhSanPham
                                       where ha.SanPhamID == s.SanPhamID && ha.LaHinhChinh == true
                                       select ha.DuongDanAnh).FirstOrDefault() ?? "/Images/default-product.jpg"
                    })
                    .ToList();

                return Json(new { success = true, sanPhamHot = sanPhamHot }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: SanPham/ChiTiet/{id}
        public ActionResult ChiTiet(int id)
        {
            var sanPham = db.SanPham
                .Include(sp => sp.HinhAnhSanPham)
                .FirstOrDefault(sp => sp.SanPhamID == id);

            if (sanPham == null)
            {
                return HttpNotFound();
            }

            // Tăng lượt xem
            sanPham.LuotXem = sanPham.LuotXem.HasValue ? sanPham.LuotXem.Value + 1 : 1;
            db.Entry(sanPham).State = EntityState.Modified;
            db.SaveChanges();

            // Lấy hình ảnh chính
            var mainImage = sanPham.HinhAnhSanPham.FirstOrDefault(ha => ha.LaHinhChinh == true);
            ViewBag.MainImageUrl = mainImage != null ? mainImage.DuongDanAnh : "/Images/default-product.jpg";

            return View(sanPham);
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