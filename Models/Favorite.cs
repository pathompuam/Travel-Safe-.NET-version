using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Travel_Safe.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string UserId { get; set; }
        public DateTime AddedDate { get; set; }

        public virtual Location Location { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

}