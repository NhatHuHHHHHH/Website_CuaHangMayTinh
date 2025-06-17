using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuaHangMayTinh2.Models
{
    public class SanPhamDetailsViewModel
    {
        public SanPham SanPham { get; set; }
        public List<SanPham> SanPhamThuongMuaCung { get; set; }
        public List<SanPham> SanPhamGoiYTheoDanhGia { get; set; }
        public Dictionary<int, Dictionary<string, object>> ProductMetrics { get; set; }
        public Dictionary<int, Dictionary<string, object>> DanhGiaMetrics { get; set; }

        public SanPhamDetailsViewModel()
        {
            SanPhamThuongMuaCung = new List<SanPham>();
            SanPhamGoiYTheoDanhGia = new List<SanPham>();
            ProductMetrics = new Dictionary<int, Dictionary<string, object>>();
            DanhGiaMetrics = new Dictionary<int, Dictionary<string, object>>();
        }
    }
}