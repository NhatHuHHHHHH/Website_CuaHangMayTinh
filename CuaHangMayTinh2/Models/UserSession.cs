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
    
    public partial class UserSession
    {
        public string SessionID { get; set; }
        public Nullable<int> KhachHangID { get; set; }
        public Nullable<System.DateTime> ThoiGianBatDau { get; set; }
        public Nullable<System.DateTime> ThoiGianKetThuc { get; set; }
        public string UserAgent { get; set; }
        public string IPAddress { get; set; }
    
        public virtual KhachHang KhachHang { get; set; }
    }
}
