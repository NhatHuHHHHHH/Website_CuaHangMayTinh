using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CuaHangMayTinh2.Models
{
    public class AprioriParameters
    {
        [Display(Name = "Ngưỡng Support (%)")]
        [Required(ErrorMessage = "Vui lòng nhập ngưỡng Support")]
        [Range(0, 100, ErrorMessage = "Ngưỡng Support phải từ 0% đến 100%")]
        public float MinSupport { get; set; }

        [Display(Name = "Ngưỡng Confidence (%)")]
        [Required(ErrorMessage = "Vui lòng nhập ngưỡng Confidence")]
        [Range(0, 100, ErrorMessage = "Ngưỡng Confidence phải từ 0% đến 100%")]
        public float MinConfidence { get; set; }

        [Display(Name = "Ngưỡng Lift")]
        [Required(ErrorMessage = "Vui lòng nhập ngưỡng Lift")]
        [Range(0, float.MaxValue, ErrorMessage = "Ngưỡng Lift phải lớn hơn hoặc bằng 0")]
        public float MinLift { get; set; }

        [Display(Name = "Danh mục")]
        public int? SelectedCategoryId { get; set; }
    }
}