using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Travel_Safe.Models
{
    public class ReviewReport
    {
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public string Description { get; set; }
        public string ReportedBy { get; set; }
        public DateTime ReportDate { get; set; }

        // การเชื่อมโยงกับ Review
        public virtual Review Review { get; set; }
    }

}