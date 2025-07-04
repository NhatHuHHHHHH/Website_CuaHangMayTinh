﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class CuaHangMayTinh2Entities : DbContext
    {
        public CuaHangMayTinh2Entities()
            : base("name=CuaHangMayTinh2Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<ChiTietDonHang> ChiTietDonHang { get; set; }
        public virtual DbSet<DanhGia> DanhGia { get; set; }
        public virtual DbSet<DanhMuc> DanhMuc { get; set; }
        public virtual DbSet<DonHang> DonHang { get; set; }
        public virtual DbSet<GioHang> GioHang { get; set; }
        public virtual DbSet<HinhAnhSanPham> HinhAnhSanPham { get; set; }
        public virtual DbSet<KhachHang> KhachHang { get; set; }
        public virtual DbSet<KhuyenMai> KhuyenMai { get; set; }
        public virtual DbSet<SanPham> SanPham { get; set; }
        public virtual DbSet<SanPhamLienQuan> SanPhamLienQuan { get; set; }
        public virtual DbSet<SanPhamThuongMuaCung> SanPhamThuongMuaCung { get; set; }
        public virtual DbSet<UserViews> UserViews { get; set; }
        public virtual DbSet<SanPhamYeuThich> SanPhamYeuThich { get; set; }
        public virtual DbSet<ProductSequence> ProductSequence { get; set; }
        public virtual DbSet<UserBehavior> UserBehavior { get; set; }
        public virtual DbSet<UserSession> UserSession { get; set; }
        public virtual DbSet<SanPhamGoiYTheoDanhGia> SanPhamGoiYTheoDanhGia { get; set; }
    
        public virtual int CapNhatKhoDuaTrenDonHang(Nullable<int> donHangID)
        {
            var donHangIDParameter = donHangID.HasValue ?
                new ObjectParameter("DonHangID", donHangID) :
                new ObjectParameter("DonHangID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("CapNhatKhoDuaTrenDonHang", donHangIDParameter);
        }
    
        public virtual int CapNhatSanPhamThuongMuaCung(Nullable<int> donHangID, Nullable<int> supportThreshold)
        {
            var donHangIDParameter = donHangID.HasValue ?
                new ObjectParameter("DonHangID", donHangID) :
                new ObjectParameter("DonHangID", typeof(int));
    
            var supportThresholdParameter = supportThreshold.HasValue ?
                new ObjectParameter("SupportThreshold", supportThreshold) :
                new ObjectParameter("SupportThreshold", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("CapNhatSanPhamThuongMuaCung", donHangIDParameter, supportThresholdParameter);
        }
    
        public virtual int sp_UpdateAprioriMetrics(Nullable<double> minSupport, Nullable<double> minConfidence, Nullable<double> minLift, Nullable<int> categoryID)
        {
            var minSupportParameter = minSupport.HasValue ?
                new ObjectParameter("MinSupport", minSupport) :
                new ObjectParameter("MinSupport", typeof(double));
    
            var minConfidenceParameter = minConfidence.HasValue ?
                new ObjectParameter("MinConfidence", minConfidence) :
                new ObjectParameter("MinConfidence", typeof(double));
    
            var minLiftParameter = minLift.HasValue ?
                new ObjectParameter("MinLift", minLift) :
                new ObjectParameter("MinLift", typeof(double));
    
            var categoryIDParameter = categoryID.HasValue ?
                new ObjectParameter("CategoryID", categoryID) :
                new ObjectParameter("CategoryID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_UpdateAprioriMetrics", minSupportParameter, minConfidenceParameter, minLiftParameter, categoryIDParameter);
        }
    
        public virtual int sp_CapNhatGoiYTheoDanhGia(Nullable<double> minSupport, Nullable<double> minConfidence, Nullable<double> minLift, Nullable<double> minDanhGia, Nullable<int> minLuotXem, Nullable<int> maxItemsetSize, Nullable<double> trongSoDanhGia, Nullable<double> trongSoLuotXem)
        {
            var minSupportParameter = minSupport.HasValue ?
                new ObjectParameter("MinSupport", minSupport) :
                new ObjectParameter("MinSupport", typeof(double));
    
            var minConfidenceParameter = minConfidence.HasValue ?
                new ObjectParameter("MinConfidence", minConfidence) :
                new ObjectParameter("MinConfidence", typeof(double));
    
            var minLiftParameter = minLift.HasValue ?
                new ObjectParameter("MinLift", minLift) :
                new ObjectParameter("MinLift", typeof(double));
    
            var minDanhGiaParameter = minDanhGia.HasValue ?
                new ObjectParameter("MinDanhGia", minDanhGia) :
                new ObjectParameter("MinDanhGia", typeof(double));
    
            var minLuotXemParameter = minLuotXem.HasValue ?
                new ObjectParameter("MinLuotXem", minLuotXem) :
                new ObjectParameter("MinLuotXem", typeof(int));
    
            var maxItemsetSizeParameter = maxItemsetSize.HasValue ?
                new ObjectParameter("MaxItemsetSize", maxItemsetSize) :
                new ObjectParameter("MaxItemsetSize", typeof(int));
    
            var trongSoDanhGiaParameter = trongSoDanhGia.HasValue ?
                new ObjectParameter("TrongSoDanhGia", trongSoDanhGia) :
                new ObjectParameter("TrongSoDanhGia", typeof(double));
    
            var trongSoLuotXemParameter = trongSoLuotXem.HasValue ?
                new ObjectParameter("TrongSoLuotXem", trongSoLuotXem) :
                new ObjectParameter("TrongSoLuotXem", typeof(double));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("sp_CapNhatGoiYTheoDanhGia", minSupportParameter, minConfidenceParameter, minLiftParameter, minDanhGiaParameter, minLuotXemParameter, maxItemsetSizeParameter, trongSoDanhGiaParameter, trongSoLuotXemParameter);
        }
    
        public virtual ObjectResult<sp_LayGoiYTheoDanhGia_Result> sp_LayGoiYTheoDanhGia(Nullable<int> sanPhamID, Nullable<int> soLuong, Nullable<int> danhMucID)
        {
            var sanPhamIDParameter = sanPhamID.HasValue ?
                new ObjectParameter("SanPhamID", sanPhamID) :
                new ObjectParameter("SanPhamID", typeof(int));
    
            var soLuongParameter = soLuong.HasValue ?
                new ObjectParameter("SoLuong", soLuong) :
                new ObjectParameter("SoLuong", typeof(int));
    
            var danhMucIDParameter = danhMucID.HasValue ?
                new ObjectParameter("DanhMucID", danhMucID) :
                new ObjectParameter("DanhMucID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_LayGoiYTheoDanhGia_Result>("sp_LayGoiYTheoDanhGia", sanPhamIDParameter, soLuongParameter, danhMucIDParameter);
        }
    }
}
