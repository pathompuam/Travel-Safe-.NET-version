using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using Travel_Safe.Models;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace Travel_Safe.Controllers
{
    public class ReviewController : Controller
    {

        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult ReviewDetails(int id)
        {
            // ไม่ใช้ 'using' ที่นี่
            var review = db.Reviews
                           .Include(r => r.Location)
                           .Include(r => r.ReviewImages) // รวมข้อมูลรูปภาพด้วย
                           .SingleOrDefault(r => r.ReviewId == id);

            if (review == null)
            {
                return HttpNotFound("ไม่พบข้อมูลรีวิวนี้");
            }

            return View(review);
        }

        [HttpPost]
        public async Task<ActionResult> ReportReview(int reviewId, string description)
        {
            if (string.IsNullOrEmpty(description))
            {
                TempData["Error"] = "กรุณากรอกเหตุผลในการรายงานปัญหา.";
                return RedirectToAction("Details", new { id = reviewId });
            }

            // ค้นหา UserName จาก UserId
            var userId = User.Identity.GetUserId();
            var userName = await db.Users
                .Where(u => u.Id == userId)
                .Select(u => u.UserName)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(userName))
            {
                TempData["Error"] = "ไม่สามารถระบุผู้รายงานได้.";
                return RedirectToAction("Details", new { id = reviewId });
            }

            var report = new ReviewReport
            {
                ReviewId = reviewId,
                Description = description,
                ReportedBy = userName, // บันทึก UserName แทน UserId
                ReportDate = DateTime.UtcNow
            };

            db.ReviewReports.Add(report);
            await db.SaveChangesAsync();

            TempData["Success"] = "การรายงานปัญหาเสร็จสิ้น. เราจะตรวจสอบรีวิวนี้.";
            return RedirectToAction("ReviewDetails", new { id = reviewId });
        }




    }
}