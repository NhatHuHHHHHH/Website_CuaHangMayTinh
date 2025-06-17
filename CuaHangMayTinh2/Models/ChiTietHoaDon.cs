using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuaHangMayTinh2.Models
{
    public class ChiTietHoaDon
    {
        public int ChiTietHoaDonID { get; set; }
        public int HoaDonID { get; set; }
        public int SanPhamID { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }

        public virtual HoaDon HoaDon { get; set; }
        public virtual SanPham SanPham { get; set; }
    }
}