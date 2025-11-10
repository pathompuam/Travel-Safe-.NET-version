using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Travel_Safe.Models;
using System.Net;
using System.Threading.Tasks;




namespace Travel_Safe.Controllers
{
    public class UserController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: User
        public ActionResult UserIndex()
        {
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

        // GET: User/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: User/CreateReview
        public ActionResult CreateReview()
        {
            // ส่งข้อมูลสถานที่ให้ผู้ใช้เลือก
            ViewBag.Locations = new SelectList(db.Locations, "LocationId", "Name");
            return View();
        }


        // POST: User/CreateReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateReview(Review model, IEnumerable<HttpPostedFileBase> images)
        {
            if (ModelState.IsValid)
            {
                model.CreatedDate = DateTime.Now;
                model.UserId = User.Identity.GetUserId(); // รับ ID ของผู้ใช้ที่ล็อกอิน

                // ดึงชื่อผู้ใช้จาก UserId
                var user = db.Users.FirstOrDefault(u => u.Id == model.UserId);
                if (user != null)
                {
                    model.UserName = user.UserName; // กำหนดชื่อผู้ใช้
                }

                db.Reviews.Add(model);
                db.SaveChanges();

                // อัปโหลดรูปภาพ
                foreach (var image in images)
                {
                    if (image != null)
                    {
                        var imageUrl = SaveImage(image); // ฟังก์ชันในการบันทึกรูป
                        db.ReviewImages.Add(new ReviewImage
                        {
                            ReviewId = model.ReviewId,
                            ImageUrl = imageUrl
                        });
                    }
                }
                db.SaveChanges();

                return RedirectToAction("ReviewDetail", new { id = model.ReviewId });
            }

            ViewBag.Locations = new SelectList(db.Locations, "LocationId", "Name", model.LocationId);
            return View(model);
        }



        public ActionResult ReviewDetail(int id)
        {
            var review = db.Reviews.Include(r => r.ReviewImages) // ใช้ Include เพื่อดึงข้อมูล ReviewImages
                                   .FirstOrDefault(r => r.ReviewId == id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        public ActionResult UserReviews()
        {
            var userId = User.Identity.GetUserId();
            var reviews = db.Reviews.Where(r => r.UserId == userId).Include(r => r.ReviewImages).ToList();
            return View(reviews);
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




        // GET: EditReview
        public ActionResult EditReview(int id)
        {
            var review = db.Reviews.Include(r => r.ReviewImages).FirstOrDefault(r => r.ReviewId == id);
            if (review == null)
            {
                return HttpNotFound();
            }

            if (review.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View(review);
        }


        // POST: EditReview
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

                // ตรวจสอบว่าเป็นเจ้าของรีวิว
                if (existingReview.UserId != User.Identity.GetUserId())
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                // อัปเดตข้อมูลรีวิว
                existingReview.Title = review.Title;
                existingReview.Content = review.Content;
                existingReview.SafetyType = review.SafetyType;
                existingReview.SafetyType2 = review.SafetyType2;
                existingReview.SafetyType3 = review.SafetyType3;
                existingReview.AdditionalTips = review.AdditionalTips;

                // อัปโหลดรูปภาพใหม่
                if (ReviewImages != null && ReviewImages.Length > 0)
                {
                    foreach (var file in ReviewImages)
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            // บันทึกรูปภาพ
                            string imagePath = SaveImage(file);

                            // สร้าง ReviewImage ใหม่ พร้อมตั้งค่า ReviewId
                            var newImage = new ReviewImage
                            {
                                ReviewId = existingReview.ReviewId, // เชื่อมโยงกับ ReviewId ที่กำลังแก้ไข
                                ImageUrl = imagePath
                            };

                            // เพิ่ม ReviewImage ลงฐานข้อมูล
                            db.ReviewImages.Add(newImage);
                        }
                    }
                }

                // บันทึกการเปลี่ยนแปลงทั้งหมดในฐานข้อมูล
                db.SaveChanges();

                // กลับไปยังหน้ารายการรีวิว
                return RedirectToAction("UserReviews");
            }

            // หากมีปัญหา ให้กลับไปยัง View เดิม
            return View(review);
        }








        // GET: DeleteReview
        public ActionResult DeleteReview(int id)
        {
            var review = db.Reviews.Find(id); // ค้นหาข้อมูลรีวิวที่ต้องการลบ
            if (review == null)
            {
                return HttpNotFound();
            }

            // ตรวจสอบว่ารีวิวเป็นของผู้ใช้นี้
            if (review.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            return View(review); // แสดงหน้า Confirm Delete
        }

        // POST: DeleteReview
        [HttpPost, ActionName("DeleteReview")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var review = db.Reviews.Include(r => r.ReviewImages).FirstOrDefault(r => r.ReviewId == id);
            if (review == null)
            {
                return HttpNotFound();
            }

            // ตรวจสอบว่ารีวิวเป็นของผู้ใช้นี้
            if (review.UserId != User.Identity.GetUserId())
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            // ลบภาพที่เกี่ยวข้องกับรีวิว
            db.ReviewImages.RemoveRange(review.ReviewImages);

            // ลบรีวิว
            db.Reviews.Remove(review);
            db.SaveChanges();

            return RedirectToAction("UserReviews"); // กลับไปยังหน้าที่แสดงรีวิวของผู้ใช้
        }


        public ActionResult DeleteImage(int reviewId, int imageId)
        {
            var review = db.Reviews.Include(r => r.ReviewImages).FirstOrDefault(r => r.ReviewId == reviewId);
            if (review == null)
            {
                return HttpNotFound();
            }

            var image = review.ReviewImages.FirstOrDefault(i => i.ReviewImageId == imageId);
            if (image == null)
            {
                return HttpNotFound();
            }

            // ลบไฟล์รูปภาพจากเซิร์ฟเวอร์
            string path = Server.MapPath(image.ImageUrl);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }

            // ลบข้อมูลรูปภาพจากฐานข้อมูลโดยตรง
            db.ReviewImages.Remove(image);
            db.SaveChanges();  // ควรจะไม่เกิดข้อผิดพลาดอีก

            return RedirectToAction("EditReview", new { id = reviewId });
        }



        // GET: ReviewDetails
        public ActionResult ReviewDetails(int id)
        {
            var review = db.Reviews.Include(r => r.ReviewImages).FirstOrDefault(r => r.ReviewId == id);
            if (review == null)
            {
                return HttpNotFound();
            }

            return View(review);
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

        // Action สำหรับแสดงสถานที่ชื่นชอบและอยากไป
        public async Task<ActionResult> FavoritesAndWishlists()
        {
            var userId = User.Identity.GetUserId(); // ดึง UserId ของผู้ใช้ที่ล็อกอินอยู่
            var favorites = await db.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Location)  // รวมข้อมูล Location จากตาราง Locations
                .ToListAsync();

            var wishlists = await db.Wishlists
                .Where(w => w.UserId == userId)
                .Include(w => w.Location)  // รวมข้อมูล Location จากตาราง Locations
                .ToListAsync();

            var model = new FavoritesAndWishlistsViewModel
            {
                Favorites = favorites,
                Wishlists = wishlists
            };

            return View(model);
        }

        // Action สำหรับลบสถานที่จาก Favorites
        [HttpPost]
        public async Task<ActionResult> RemoveFavorite(int locationId)
        {
            var userId = User.Identity.GetUserId();
            var favorite = await db.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.LocationId == locationId);

            if (favorite != null)
            {
                db.Favorites.Remove(favorite);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("FavoritesAndWishlists");
        }

        // Action สำหรับลบสถานที่จาก Wishlists
        [HttpPost]
        public async Task<ActionResult> RemoveWishlist(int locationId)
        {
            var userId = User.Identity.GetUserId();
            var wishlist = await db.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.LocationId == locationId);

            if (wishlist != null)
            {
                db.Wishlists.Remove(wishlist);
                await db.SaveChangesAsync();
            }

            return RedirectToAction("FavoritesAndWishlists");
        }

        [Authorize]
        public ActionResult Manage()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);

            if (user == null)
            {
                return HttpNotFound();
            }

            var model = new ManageUserViewModel
            {
                UserName = user.UserName,
                Email = user.Email
            };

            return View(model);
        }

        // POST: User/Manage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Manage(ManageUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var user = db.Users.Find(userId);

                if (user == null)
                {
                    return HttpNotFound();
                }

                // Update Username and Email
                user.UserName = model.UserName;
                user.Email = model.Email;

                // Change Password if provided
                if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
                {
                    var passwordHasher = new Microsoft.AspNet.Identity.PasswordHasher();

                    // Verify current password
                    var result = passwordHasher.VerifyHashedPassword(user.PasswordHash, model.CurrentPassword);
                    if (result == PasswordVerificationResult.Failed)
                    {
                        ModelState.AddModelError("CurrentPassword", "The current password is incorrect.");
                        return View(model);
                    }

                    // Update password
                    user.PasswordHash = passwordHasher.HashPassword(model.NewPassword);
                }

                db.SaveChanges();
                ViewBag.Message = "Profile updated successfully.";
            }

            return View(model);
        }





    }
}
