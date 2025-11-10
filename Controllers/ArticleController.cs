using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Travel_Safe.Models;

namespace Travel_Safe.Controllers
{
    public class ArticleController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Article/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Article/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Article article, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                if (image != null && image.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(image.FileName);
                    var filePath = Path.Combine(Server.MapPath("~/Content/images/articles"), fileName);
                    image.SaveAs(filePath);
                    article.ImagePath = "~/Content/images/articles/" + fileName;
                }
                article.CreatedAt = DateTime.Now;
                article.CreatedBy = User.Identity.Name;
                db.Articles.Add(article);
                db.SaveChanges();
                return RedirectToAction("Index", "Article");
            }

            return View(article);
        }

        // GET: Article
        public ActionResult Index()
        {
            var articles = db.Articles.ToList();

            foreach (var article in articles)
            {
                if (article.CreatedAt.HasValue)
                {
                    try
                    {
                        // แปลงปี พ.ศ. เป็น ค.ศ.
                        // ถ้า CreatedAt เป็นวันที่ที่ถูกต้องก็จะทำการแปลง
                        var convertedDate = article.CreatedAt.Value.AddYears(-543);

                        // ตรวจสอบว่า conversion เกิดข้อผิดพลาดหรือไม่
                        if (convertedDate > DateTime.MinValue)
                        {
                            article.CreatedAt = convertedDate;
                        }
                        else
                        {
                            // ถ้า conversion ไม่สำเร็จ ก็สามารถกำหนดให้เป็นวันที่ที่มีความหมาย หรือไม่ทำอะไร
                            article.CreatedAt = DateTime.Now; // หรือจะให้ค่าเป็นวันที่ปัจจุบัน
                        }
                    }
                    catch (ArgumentOutOfRangeException)
                    {
                        // ถ้ามีข้อผิดพลาดในการแปลง
                        article.CreatedAt = DateTime.Now; // หรือจะให้ค่าเป็นวันที่ปัจจุบัน
                    }
                }
            }

            return View(articles);
        }

        // GET: Article/Delete/{id}
        public ActionResult Delete(int id)
        {
            var article = db.Articles.Find(id);
            if (article == null)
            {
                return HttpNotFound();
            }
            return View(article);
        }

        // POST: Article/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var article = db.Articles.Find(id);
            if (article != null)
            {
                db.Articles.Remove(article);
                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            var article = db.Articles.Find(id);

            if (article == null)
            {
                return HttpNotFound();
            }

            return View(article);
        }


    }
}