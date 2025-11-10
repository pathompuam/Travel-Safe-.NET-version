using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Travel_Safe.Models
{
    public class LocationDetailsViewModel
    {
        public Location Location { get; set; }
        public List<Review> RecentReviews { get; set; }
    }
}