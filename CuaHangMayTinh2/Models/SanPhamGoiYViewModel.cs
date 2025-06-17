using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuaHangMayTinh2.Models
{
    public class SanPhamGoiYViewModel
    {
        public int SanPhamIDGoc { get; set; }
        public int SanPhamIDGoiY { get; set; }
        public string TenSPGoc { get; set; }
        public string TenSPGoiY { get; set; }
        public float Support { get; set; }
        public float Confidence { get; set; }
        public float Lift { get; set; }
        public float DiemDanhGia { get; set; }
        public int LuotXem { get; set; }
        public float DiemPhoBien { get; set; }
        public string HinhAnhGoc { get; set; }
        public string HinhAnhGoiY { get; set; }
    }
}