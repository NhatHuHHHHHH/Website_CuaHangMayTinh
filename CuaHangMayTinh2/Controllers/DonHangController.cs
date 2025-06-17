// DonHangController.cs
using System;
using System.Linq;
using System.Web.Mvc;
using CuaHangMayTinh2.Models;
using System.Data.Entity;

namespace CuaHangMayTinh2.Controllers
{
    public class DonHangController : Controller
    {
        private CuaHangMayTinh2Entities db = new CuaHangMayTinh2Entities();

        // GET: DonHang/PurchaseSuccess
        public ActionResult PurchaseSuccess(int? donHangId)
        {
            var donHang = db.DonHang
                .Include(dh => dh.ChiTietDonHang.Select(ct => ct.SanPham))
                .FirstOrDefault(dh => dh.DonHangID == donHangId);

            if (donHang == null)
            {
                return HttpNotFound();
            }

            return View(donHang);
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