using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Travel_Safe.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int NormalUsers { get; set; }
        public int ProviderUsers { get; set; }
        public int TotalLocations { get; set; }
        public int TotalReviews { get; set; }
        public int ReportedReviews { get; set; }
        public List<LocationStat> MostReviewedLocations { get; set; }
        public List<LocationStat> MostFavoritedLocations { get; set; }
        public List<LocationStat> MostWishlistedLocations { get; set; }
        public List<UserStat> TopReviewers { get; set; }

        public List<LocationAverageRatingViewModel> AverageRatings { get; set; }
    }

    

}