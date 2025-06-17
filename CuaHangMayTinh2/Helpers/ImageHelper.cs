using System.Collections.Generic;
using System.Linq;
using CuaHangMayTinh2.Models;

namespace CuaHangMayTinh2.Helpers
{
    public static class ImageHelper
    {
        public static string GetImagePath(ICollection<HinhAnhSanPham> images)
        {
            if (images == null || !images.Any())
            {
                return "/images/default.jpg";
            }

            var mainImage = images.FirstOrDefault(x => x.LaHinhChinh == true);
            if (mainImage != null && !string.IsNullOrEmpty(mainImage.DuongDanAnh))
            {
                return mainImage.DuongDanAnh;
            }

            var firstImage = images.FirstOrDefault();
            return firstImage != null && !string.IsNullOrEmpty(firstImage.DuongDanAnh)
                ? firstImage.DuongDanAnh
                : "/images/default.jpg";
        }
    }
}