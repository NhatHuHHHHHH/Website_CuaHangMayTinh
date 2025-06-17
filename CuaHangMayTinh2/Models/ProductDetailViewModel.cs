using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuaHangMayTinh2.Models
{
    public class ProductDetailViewModel
    {
        public SanPham SanPham { get; set; }
        public List<HinhAnhSanPham> HinhAnhList { get; set; }
        public string MainImageUrl { get; set; }
        public string DanhMuc { get; set; }
    }
}