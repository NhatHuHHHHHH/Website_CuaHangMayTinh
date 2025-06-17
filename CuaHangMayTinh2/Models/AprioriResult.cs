using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuaHangMayTinh2.Models
{
    public class AprioriResult
    {
        public int SanPhamID1 { get; set; }
        public int SanPhamID2 { get; set; }
        public string TenSP1 { get; set; }
        public string TenSP2 { get; set; }
        public int SoLanMuaCung { get; set; }
        public double Support { get; set; }
        public double Confidence { get; set; }
        public double Lift { get; set; }
        public string HinhAnh1 { get; set; }
        public string HinhAnh2 { get; set; }
    }
}