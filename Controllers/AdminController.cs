using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Travel_Safe.Models;
using System.Data.Entity;
using System.Net;
using System;
using System.IO;
using System.Web;

namespace Travel_Safe.Controllers
{
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserManager<ApplicationUser> UserManager;
        private RoleManager<IdentityRole> RoleManager;

        public AdminController()
        {
            UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
        }

        public List<string> GetUserRoles(string username)
        {
            List<string> ListOfRoleNames = new List<string>();
            var ListOfRoleIds = UserManager.FindByName(username).Roles.Select(x => x.RoleId).ToList();
            foreach (string id in ListOfRoleIds)
            {
                string rolename = RoleManager.FindById(id).Name;
                ListOfRoleNames.Add(rolename);
            }
            return ListOfRoleNames;
        }

        // GET: Admin
        public ActionResult AdminIndex(string search, string roleFilter)
        {
            var users = db.Users.ToList();

            // สร้าง dictionary เพื่อเก็บชื่อผู้ใช้และ role ของผู้ใช้
            Dictionary<string, List<string>> userRoles = new Dictionary<string, List<string>>();

            foreach (var user in users)
            {
                var roles = GetUserRoles(user.UserName); // ใช้ฟังก์ชัน GetUserRoles ที่ได้เขียนไว้
                userRoles.Add(user.UserName, roles);
            }

            // ส่งข้อมูลผู้ใช้และบทบาทไปยัง View
            ViewBag.UserRoles = userRoles;

            // กรองตามประเภทผู้ใช้ (Role)
            if (!string.IsNullOrEmpty(roleFilter))
            {
                users = users.Where(u => userRoles[u.UserName].Contains(roleFilter)).ToList();
            }

            // ค้นหาผู้ใช้ตาม Email หรือ Id
            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(u => u.Email.Contains(search) || u.Id.Contains(search)).ToList();
            }

            return View(users);
        }


        // GET: Admin/Details/5
        public ActionResult Details(string id)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // GET: Admin/Edit/5
        public ActionResult Edit(string id)
        {
            var user = db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationUser updatedUser)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.FirstOrDefault(u => u.Id == updatedUser.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                // อัปเดตข้อมูล
                user.Email = updatedUser.Email;
                user.UserName = updatedUser.UserName;

                db.SaveChanges();
                return RedirectToAction("AdminIndex");
            }

            return View(updatedUser);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // ลบผู้ใช้จากฐานข้อมูล
            db.Users.Remove(user);
            db.SaveChanges();

            return RedirectToAction("AdminIndex");
        }


        // Action สำหรับดูรายละเอียดของ User
        public ActionResult UserDetails(string id)
        {
            var user = db.Users.Include(u => u.Reviews).FirstOrDefault(u => u.Id == id);
            if (user == null)
                {
                return HttpNotFound();
        }

        // ดึงรีวิวทั้งหมดของ User
        var reviews = user.Reviews.ToList();
            return View(reviews);
    }

    public ActionResult ReviewDetail(int? id)
    {
        if (id == null)
        {
            return HttpNotFound(); // หรือจัดการกรณี id เป็น null
        }

        // ดึงข้อมูลรีวิวจากฐานข้อมูลโดยใช้ ReviewId
        var review = db.Reviews.Include(r => r.ReviewImages).FirstOrDefault(r => r.ReviewId == id.Value);

        if (review == null)
        {
            return HttpNotFound();
        }

        return View(review); // ส่งข้อมูลรีวิวไปที่ View
    }

    private string SaveImage(HttpPostedFileBase image)
    {
        // ตรวจสอบประเภทไฟล์ (Optional)
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(image.FileName)?.ToLower();
        if (!allowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Invalid file type.");
        }

        // สร้างชื่อไฟล์ใหม่เพื่อป้องกันการซ้ำกัน
        string fileName = Guid.NewGuid().ToString() + extension;
        string path = Path.Combine(Server.MapPath("~/Content/Images/Reviews"), fileName);

        // บันทึกไฟล์ลงในเซิร์ฟเวอร์
        image.SaveAs(path);

        return "/Content/Images/Reviews/" + fileName;
    }

    public ActionResult EditReview(int id)
    {
        var review = db.Reviews.Include(r => r.ReviewImages).FirstOrDefault(r => r.ReviewId == id);
        if (review == null)
        {
            return HttpNotFound();
        }

        return View(review);
    }

    // Action สำหรับการแก้ไขรีวิว
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult EditReview(Review review, HttpPostedFileBase[] ReviewImages)
    {
        if (ModelState.IsValid)
        {
            // ดึงข้อมูลรีวิวเดิมจากฐานข้อมูล
            var existingReview = db.Reviews.Include(r => r.ReviewImages).FirstOrDefault(r => r.ReviewId == review.ReviewId);
            if (existingReview == null)
            {
                return HttpNotFound();
            }

            // อัปเดตข้อมูลรีวิว
            existingReview.Title = review.Title;
            existingReview.Content = review.Content;
            existingReview.SafetyType = review.SafetyType;
            existingReview.SafetyType2 = review.SafetyType2;
            existingReview.SafetyType3 = review.SafetyType3;
            existingReview.AdditionalTips = review.AdditionalTips;

            // บันทึกการเปลี่ยนแปลงทั้งหมดในฐานข้อมูล
            db.SaveChanges();

            // กลับไปยังหน้ารายการรีวิว
            return RedirectToAction("AdminIndex");
        }

        // หากมีปัญหา ให้กลับไปยัง View เดิม
        return View(review);
    }


    // Action สำหรับหน้า Delete
    public ActionResult DeleteReview(int id)
    {
        var review = db.Reviews.Find(id);  // ค้นหาด้วย ReviewId
        if (review == null)
        {
            return HttpNotFound();
        }
        return View(review);  // ส่งข้อมูลรีวิวไปที่หน้า View
    }

    [HttpPost, ActionName("DeleteReview")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteConfirmed(int id)
    {
        // ค้นหารีวิว
        var review = db.Reviews.Include(r => r.ReviewImages).FirstOrDefault(r => r.ReviewId == id);
        if (review == null)
        {
            return HttpNotFound();
        }

        // ลบ ReviewImages ที่เกี่ยวข้อง
        db.ReviewImages.RemoveRange(review.ReviewImages);

        // ลบรีวิว
        db.Reviews.Remove(review);

        // บันทึกการเปลี่ยนแปลง
        db.SaveChanges();

        return RedirectToAction("AdminIndex");
    }




    // Action สำหรับดูรายละเอียดของ Provider (สถานที่)
    public ActionResult ProviderDetails(string id)
    {
        var provider = db.Users.FirstOrDefault(u => u.Id == id);
        if (provider == null)
        {
            return HttpNotFound();
        }

        // ดึงข้อมูลสถานที่ที่ Provider ดูแล
        var locations = db.Locations.Where(l => l.CreatorId == id).ToList();
        return View(locations);
    }

    public ActionResult LocationDetail(int id)
    {
        // หาสถานที่จาก ID ที่ได้รับ
        var location = db.Locations.Find(id);
        if (location == null)
        {
            return HttpNotFound();
        }
        return View(location);
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
            return RedirectToAction("ProviderDetails", new { id = location.CreatorId });
        }

        return View(updatedLocation);
    }





    // GET: Admin/DeleteLocation/5
    public ActionResult DeleteLocation(int id)
    {
        var location = db.Locations.Find(id);
        if (location == null)
        {
            return HttpNotFound();
        }

        return View(location);
    }

    // POST: Admin/DeleteLocation/5
    [HttpPost, ActionName("DeleteLocation")]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteLocationConfirmed(int id)
    {
        var location = db.Locations.Find(id);
        if (location == null)
        {
            return HttpNotFound();
        }

        // ลบสถานที่
        db.Locations.Remove(location);
        db.SaveChanges();

        return RedirectToAction("AdminIndex");
    }

    // GET: Admin/ManageReviews
    public ActionResult ManageReviews(string search)
    {
        var reviews = db.Reviews.Include(r => r.Location).ToList();

        if (!string.IsNullOrEmpty(search))
        {
            reviews = reviews.Where(r => r.Title.Contains(search) || r.Content.Contains(search)).ToList();
        }

        return View(reviews);
    }

    // GET: Admin/ManagePlaces
    public ActionResult ManagePlaces(string search)
    {
        var locations = db.Locations.Include(l => l.Creator).ToList();

        if (!string.IsNullOrEmpty(search))
        {
            locations = locations.Where(l => l.Name.Contains(search) || l.Address.Contains(search)).ToList();
        }

        return View(locations);
    }

    public ActionResult Notifications()
    {
        // ดึงข้อมูลการแจ้งเตือนทั้งหมด
        var notifications = db.ReviewReports.Include(r => r.Review).ToList();
        return View(notifications);
    }

    // ลบการแจ้งเตือน
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult DeleteNotification(int id)
    {
        var report = db.ReviewReports.Find(id);
        if (report != null)
        {
            db.ReviewReports.Remove(report);
            db.SaveChanges();
        }
        return RedirectToAction("Notifications");
    }

    public ActionResult Dashboard()
    {
        using (var db = new ApplicationDbContext())
        {
            // ดึง RoleId ของแต่ละบทบาท
            var userRoleId = db.Roles.Where(r => r.Name == "User").Select(r => r.Id).FirstOrDefault();
            var providerRoleId = db.Roles.Where(r => r.Name == "Provider").Select(r => r.Id).FirstOrDefault();

            // 1. จำนวนผู้ใช้ทั้งหมด
            var totalUsers = db.Users.Count();

            // 2. แยกจำนวนผู้ใช้ปกติและผู้ให้บริการ
            var normalUsers = db.Users.Where(u => u.Roles.Any(r => r.RoleId == userRoleId)).Count();
            var providerUsers = db.Users.Where(u => u.Roles.Any(r => r.RoleId == providerRoleId)).Count();

            // 3. จำนวนสถานที่ทั้งหมด
            var totalLocations = db.Locations.Count();

            // 4. จำนวนรีวิวทั้งหมด
            var totalReviews = db.Reviews.Count();

            // 5. รีวิวที่ถูกรายงาน
            var reportedReviews = db.ReviewReports.Count();

            // 6. สถานที่ที่มีจำนวนรีวิวมากที่สุด
            var mostReviewedLocations = db.Reviews
                .GroupBy(r => r.LocationId)
                .Select(g => new { LocationId = g.Key, Count = g.Count() })
                .Join(db.Locations,
                    reviewGroup => reviewGroup.LocationId,
                    location => location.LocationId,
                    (reviewGroup, location) => new LocationStat
                    {
                        LocationName = location.Name,
                        Count = reviewGroup.Count
                    })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            // 7. สถานที่ที่ผู้ใช้กด Favorites มากที่สุด
            var mostFavoritedLocations = db.Favorites
                .GroupBy(f => f.LocationId)
                .Select(g => new { LocationId = g.Key, Count = g.Count() })
                .Join(db.Locations,
                    favoriteGroup => favoriteGroup.LocationId,
                    location => location.LocationId,
                    (favoriteGroup, location) => new LocationStat
                    {
                        LocationName = location.Name,
                        Count = favoriteGroup.Count
                    })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();

            // 8. สถานที่ที่ผู้ใช้กด Wishlist มากที่สุด
            var mostWishlistedLocations = db.Wishlists
                .GroupBy(w => w.LocationId)
                .Select(g => new { LocationId = g.Key, Count = g.Count() })
                .Join(db.Locations,
                    wishlistGroup => wishlistGroup.LocationId,
                    location => location.LocationId,
                    (wishlistGroup, location) => new LocationStat
                    {
                        LocationName = location.Name,
                        Count = wishlistGroup.Count
                    })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToList();


            // 9. Top 5 ผู้ใช้ที่เขียนรีวิวมากที่สุด
            var topReviewers = db.Reviews
                .GroupBy(r => r.UserId)
                .Select(g => new UserStat
                {
                        UserName = db.Users.Where(u => u.Id == g.Key).Select(u => u.UserName).FirstOrDefault(),
                    ReviewCount = g.Count()
                })
                .OrderByDescending(x => x.ReviewCount)
                .Take(5)
                .ToList();

                // 10. คะแนนเฉลี่ยรีวิวของแต่ละสถานที่
                var locations = db.Locations.ToList();
                var reviews = db.Reviews
                    .GroupBy(r => r.LocationId)
                    .Select(g => new
                    {
                        LocationId = g.Key,
                        AverageRating = g.Average(r => (double?)r.Rating) ?? 0
                    }).ToList();

                var averageRatings = locations
                    .Select(loc => new LocationAverageRatingViewModel
                    {
                        LocationName = loc.Name,
                        AverageRating = reviews.FirstOrDefault(r => r.LocationId == loc.LocationId)?.AverageRating ?? 0
                    })
                    .OrderByDescending(x => x.AverageRating)
                    .ToList();


                // ส่งข้อมูลไปยัง View
                var dashboardViewModel = new AdminDashboardViewModel
            {
                TotalUsers = totalUsers,
                NormalUsers = normalUsers,
                ProviderUsers = providerUsers,
                TotalLocations = totalLocations,
                TotalReviews = totalReviews,
                ReportedReviews = reportedReviews,
                MostReviewedLocations = mostReviewedLocations,
                MostFavoritedLocations = mostFavoritedLocations,
                MostWishlistedLocations = mostWishlistedLocations,
                TopReviewers = topReviewers,
                AverageRatings = averageRatings,
                };

            return View(dashboardViewModel);
        }
    }






}
}
