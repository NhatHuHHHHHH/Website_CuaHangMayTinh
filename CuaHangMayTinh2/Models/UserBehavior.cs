//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CuaHangMayTinh2.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserBehavior
    {
        public int BehaviorID { get; set; }
        public Nullable<int> KhachHangID { get; set; }
        public Nullable<int> SanPhamID { get; set; }
        public string SessionID { get; set; }
        public string HanhDong { get; set; }
        public Nullable<System.DateTime> ThoiGianBatDau { get; set; }
        public Nullable<System.DateTime> ThoiGianKetThuc { get; set; }
        public Nullable<int> ThoiLuong { get; set; }
        public string TrangThai { get; set; }
    
        public virtual KhachHang KhachHang { get; set; }
        public virtual SanPham SanPham { get; set; }
    }
}
