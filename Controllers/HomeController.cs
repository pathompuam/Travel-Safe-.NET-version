using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using System.Diagnostics;
using Travel_Safe.Models;

namespace Travel_Safe.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext db = new ApplicationDbContext();

        [AllowAnonymous]
        [Authorize]
        public ActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("AdminIndex", "Admin");
            }
            else if (User.IsInRole("Provider"))
            {
                // ค้นหาว่าผู้ใช้ใน Role "Provider" เคยสร้างข้อมูลใน Location หรือไม่
                var userId = User.Identity.GetUserId(); // รับ ID ของผู้ใช้งานที่ล็อกอิน
                Debug.WriteLine("User ID: " + userId);
                var location = db.Locations.FirstOrDefault(l => l.CreatorId == userId); // ตรวจสอบว่าเป็นผู้สร้างสถานที่

                if (location != null)
                {
                    // ถ้ามีข้อมูลสถานที่ ให้ redirect ไปยังหน้า Details
                    return RedirectToAction("Details", "Provider", new { id = location.LocationId });
                }
                else
                {
                    // ถ้าไม่มีข้อมูล ให้ไปที่หน้า Create
                    return RedirectToAction("Create", "Provider");
                }
            }
            else if (User.IsInRole("User"))
            {
                return RedirectToAction("UserIndex", "User");
            }

            var latestArticles = db.Articles
                .OrderByDescending(a => a.CreatedAt)
                .Take(3)
                .ToList();

            var latestReviews = db.Reviews.OrderByDescending(r => r.CreatedDate)
                                     .Take(2)
                                     .Include(r => r.Location)
                                     .ToList();

            // ดึงสถานที่ 6 แห่ง
            var recommendedPlaces = db.Locations.Take(6).ToList();

      

            // ส่งข้อมูลไปยัง View
            ViewBag.LatestReviews = latestReviews;
            ViewBag.RecommendedPlaces = recommendedPlaces;

           

            return View(latestArticles);


        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Search(string query, string category)
        {
            using (var db = new ApplicationDbContext())
            {
                // Query สถานที่ตามคำค้นหา
                var locations = db.Locations.AsQueryable();

                if (!string.IsNullOrEmpty(query))
                {
                    locations = locations.Where(l => l.Name.Contains(query));
                }

                if (!string.IsNullOrEmpty(category))
                {
                    locations = locations.Where(l => l.LocationType == category);
                }

                var result = locations.ToList();

                return View(result); 
            }
        }

        public ActionResult LocationDetails(int locationId)
        {
            // ดึงข้อมูลสถานที่
            var location = db.Locations.Find(locationId);
            if (location == null)
            {
                return HttpNotFound();
            }

            // ดึงข้อมูลรีวิว 2 อันล่าสุดสำหรับสถานที่นี้
            var reviews = db.Reviews
                            .Where(r => r.LocationId == locationId)
                            .OrderByDescending(r => r.CreatedDate)
                            .Take(2)
                            .ToList();

            // สร้าง ViewModel
            var viewModel = new LocationDetailsViewModel2
            {
                Location = location,
                Reviews = reviews
            };

            return View(viewModel);
        }


    }
}