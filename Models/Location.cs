using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Travel_Safe.Models
{
    public class Location
    {
        public int LocationId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string OpeningHours { get; set; }
        public string EntryFee { get; set; }
        public string Contact { get; set; }
        public string Description { get; set; }
        public string ImageUrls { get; set; }
        public string LocationType { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string CreatorId { get; set; } // ใช้ string เพื่อเก็บ ID ของผู้ใช้ (เช่น User.Identity.GetUserId())
        public virtual ApplicationUser Creator { get; set; }
    }

}