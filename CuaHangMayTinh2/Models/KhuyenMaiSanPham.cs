using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuaHangMayTinh2.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class KhuyenMaiSanPham
    {
        [Key]
        [Column(Order = 0)]
        public int KhuyenMaiID { get; set; }

        [Key]
        [Column(Order = 1)]
        public int SanPhamID { get; set; }

        public virtual KhuyenMai KhuyenMai { get; set; }
        public virtual SanPham SanPham { get; set; }
    }
}