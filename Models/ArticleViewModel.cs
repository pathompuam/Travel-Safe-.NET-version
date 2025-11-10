using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Travel_Safe.Models
{
    public class ArticleViewModel
    {
        public int ArticleId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImagePath { get; set; }
    }
}