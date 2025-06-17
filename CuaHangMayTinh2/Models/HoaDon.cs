using System;
using System.Collections.Generic;

namespace CuaHangMayTinh2.Models
{
    public class HoaDon
    {
        public int HoaDonID { get; set; }
        public string UserID { get; set; }
        public DateTime NgayDatHang { get; set; }
        public string DiaChiGiaoHang { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public decimal TongTien { get; set; }
        public string TrangThai { get; set; }

        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
    }
}