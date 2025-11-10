using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Travel_Safe.Models
{
    public class ReviewImage
    {
        public int ReviewImageId { get; set; }  // Primary Key
        public int? ReviewId { get; set; }       // Foreign Key to Review
        public string ImageUrl { get; set; }

        public virtual Review Review { get; set; }
    }

}