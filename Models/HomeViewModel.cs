using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Travel_Safe.Models
{
    public class HomeViewModel
    {
        public List<Article> Articles { get; set; }
        public List<Review> Reviews { get; set; }
    }
}