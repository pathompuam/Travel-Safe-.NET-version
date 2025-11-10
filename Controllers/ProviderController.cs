using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Travel_Safe.Models;

namespace Travel_Safe.Controllers
{
    public class ProviderController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Provider
        public ActionResult ProviderIndex()
        {
            return View();
        }

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

        // GET: Provider/Create
        public ActionResult Create()
        {
            ViewBag.LocationTypes = new SelectList(new List<string>
    {
        "ธรรมชาติ",
        "วัฒนธรรมและประวัติศาสตร์",
        "พักผ่อนและบันเทิง",
        "เชิงเกษตร",
        "เชิงสุขภาพ",
        "ผจญภัยและกิจกรรมกลางแจ้ง",
        "เชิงวิทยาศาสตร์และการศึกษา",
        "อื่น ๆ"
    });


            return View();
        }

        // POST: Provider/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Location model, HttpPostedFileBase[] Images)
        {
            if (ModelState.IsValid)
            {
                // อัปโหลดรูปภาพ
                List<string> imageUrls = new List<string>();

                if (Images != null && Images.Length > 0)
                {
                    foreach (var image in Images)
                    {
                        if (image.ContentLength > 0)
                        {
                            var fileName = Path.GetFileName(image.FileName);
                            var path = Path.Combine(Server.MapPath("~/Content/Images"), fileName);
                            image.SaveAs(path);

                            // เพิ่ม URL ของภาพลงในลิสต์
                            imageUrls.Add("/Content/Images/" + fileName);
                        }
                    }

                    // บันทึก URL ของรูปภาพทั้งหมด
                    model.ImageUrls = string.Join(",", imageUrls);
                }

                // ตั้งค่า CreatedDate และ UpdatedDate
                model.CreatedDate = DateTime.Now;
                model.UpdatedDate = DateTime.Now;  // ตั้งค่า UpdatedDate

                model.CreatorId = User.Identity.GetUserId();

                // บันทึกข้อมูลสถานที่ในฐานข้อมูล
                db.Locations.Add(model);
                db.SaveChanges();

                return RedirectToAction("Details", new { id = model.LocationId });
            }

            // โหลดข้อมูลสำหรับ DropDownList ใหม่
            ViewBag.LocationTypes = new SelectList(new List<string>
    {
        "ธรรมชาติ",
        "วัฒนธรรมและประวัติศาสตร์",
        "พักผ่อนและบันเทิง",
        "เชิงเกษตร",
        "เชิงสุขภาพ",
        "ผจญภัยและกิจกรรมกลางแจ้ง",
        "เชิงวิทยาศาสตร์และการศึกษา",
        "อื่น ๆ"
    }, "");

            return View(model);
        }




        // GET: Provider/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Provider/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Provider/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Provider/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


        public ActionResult ViewReviews(string search)
        {
            var providerId = User.Identity.GetUserId();
            var locations = db.Locations
                              .Where(l => l.CreatorId == providerId)
                              .ToList();

            var reviewsQuery = db.Reviews
                                 .Where(r => locations.Any(l => l.LocationId == r.LocationId));

            if (!string.IsNullOrEmpty(search))
            {
                reviewsQuery = reviewsQuery.Where(r => r.Title.Contains(search) || r.Content.Contains(search)); // ค้นหาชื่อหรือเนื้อหา
            }

            var reviews = reviewsQuery.ToList();

            return View(reviews);
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
