using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Travel_Safe.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int LocationId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string SafetyType { get; set; }
        public string SafetyType2 { get; set; }
        public string SafetyType3 { get; set; }
        public string AdditionalTips { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserId { get; set; }
        public int Rating { get; set; } // เพิ่ม Rating สำหรับคะแนน
        public string UserName { get; set; } // เพิ่ม UserName เพื่อเก็บชื่อผู้ใช้

        public virtual Location Location { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<ReviewImage> ReviewImages { get; set; }
    }

}