using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Travel_Safe.Models;

namespace Travel_Safe.Models
{
    public class FavoritesAndWishlistsViewModel
    {
        public List<Favorite> Favorites { get; set; }
        public List<Wishlist> Wishlists { get; set; }
    }
}