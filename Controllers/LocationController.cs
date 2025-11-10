using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using Travel_Safe.Models;
using Microsoft.AspNet.Identity;

namespace Travel_Safe.Controllers
{
    public class LocationController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Location/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }

            // ค้นหาสถานที่จากฐานข้อมูล
            var location = db.Locations.SingleOrDefault(l => l.LocationId == id);
            if (location == null)
            {
                return HttpNotFound("ไม่พบข้อมูลสถานที่นี้");
            }

            // ดึงรีวิวล่าสุด 3 อันของสถานที่นี้
            var reviews = db.Reviews
                            .Where(r => r.LocationId == id)
                            .OrderByDescending(r => r.CreatedDate)
                            .Take(3)
                            .ToList();

            // สร้าง ViewModel ส่งข้อมูล
            var viewModel = new LocationDetailsViewModel
            {
                Location = location,
                RecentReviews = reviews
            };

            return View(viewModel);
        }

        public ActionResult AllReviews(int id)
        {
            var location = db.Locations.SingleOrDefault(l => l.LocationId == id);
            if (location == null)
            {
                return HttpNotFound("ไม่พบสถานที่นี้");
            }

            var reviews = db.Reviews
                            .Where(r => r.LocationId == id)
                            .OrderByDescending(r => r.CreatedDate)
                            .ToList();

            ViewBag.LocationName = location.Name;
            return View(reviews);
        }

        [HttpPost]
        [Authorize] // ให้เฉพาะผู้ใช้ที่ล็อกอินเท่านั้น
        public JsonResult AddToFavorites(int locationId)
        {
            var userId = User.Identity.GetUserId(); // ใช้ UserManager ในการดึง UserId
            var existingFavorite = db.Favorites.FirstOrDefault(f => f.LocationId == locationId && f.UserId == userId);

            if (existingFavorite != null)
            {
                return Json(new { success = false, message = "สถานที่นี้ถูกเพิ่มไว้ในรายการที่ชอบแล้ว" });
            }

            var favorite = new Favorite
            {
                LocationId = locationId,
                UserId = userId,
                AddedDate = DateTime.Now
            };

            db.Favorites.Add(favorite);
            db.SaveChanges();

            return Json(new { success = true, message = "เพิ่มสถานที่นี้ไปยังรายการที่ชอบเรียบร้อยแล้ว" });
        }

        [HttpPost]
        [Authorize] // ให้เฉพาะผู้ใช้ที่ล็อกอินเท่านั้น
        public JsonResult AddToWishlist(int locationId)
        {
            var userId = User.Identity.GetUserId(); // ใช้ UserManager ในการดึง UserId
            var existingWishlist = db.Wishlists.FirstOrDefault(w => w.LocationId == locationId && w.UserId == userId);

            if (existingWishlist != null)
            {
                return Json(new { success = false, message = "สถานที่นี้ถูกเพิ่มไว้ในรายการที่อยากไปแล้ว" });
            }

            var wishlist = new Wishlist
            {
                LocationId = locationId,
                UserId = userId,
                AddedDate = DateTime.Now
            };

            db.Wishlists.Add(wishlist);
            db.SaveChanges();

            return Json(new { success = true, message = "เพิ่มสถานที่นี้ไปยังรายการที่อยากไปเรียบร้อยแล้ว" });
        }

        // GET: Admin/EditLocation/5
        public ActionResult EditLocation(int id)
        {
            var location = db.Locations.Find(id);
            if (location == null)
            {
                return HttpNotFound();
            }
            return View(location);
        }

        // POST: Admin/EditLocation/5
        // POST: Admin/EditLocation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditLocation(Location updatedLocation)
        {
            // แสดงค่าของ updatedLocation.LocationId เพื่อดีบั๊ก
            System.Diagnostics.Debug.WriteLine("Updated Location ID: " + updatedLocation.LocationId);

            if (ModelState.IsValid)
            {
                // แสดงการตรวจสอบค่าที่ได้รับมา
                var location = db.Locations.FirstOrDefault(l => l.LocationId == updatedLocation.LocationId);

                // ตรวจสอบผลลัพธ์จากการค้นหา
                if (location == null)
                {
                    // แสดงการดีบั๊กว่าหา location ไม่เจอ
                    System.Diagnostics.Debug.WriteLine("Location not found for ID: " + updatedLocation.LocationId);
                    return HttpNotFound();
                }

                // ถ้ามี location ก็ทำการอัปเดต
                location.Name = updatedLocation.Name;
                location.Description = updatedLocation.Description;
                location.Address = updatedLocation.Address;
                location.OpeningHours = updatedLocation.OpeningHours;
                location.Contact = updatedLocation.Contact;

                // บันทึกการเปลี่ยนแปลง
                db.SaveChanges();
                return RedirectToAction("Details", "Provider", new { id = location.LocationId });
            }

            return View(updatedLocation);
        }

    }
}