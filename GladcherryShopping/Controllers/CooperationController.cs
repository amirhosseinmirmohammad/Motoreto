using DataLayer.Models;
using GladcherryShopping.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GladcherryShopping.Controllers
{
    public class CooperationController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Cooperation
        public ActionResult Index()
        {
            Application app = db.Applications.FirstOrDefault();
            if (app != null && app.AboutUs != null)
            {
                return View(app);
            }
            else
                return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitMessage(Cooperation model)
        {
            if (ModelState.IsValid)
            {
                db.Cooperations.Add(model);
                db.SaveChanges();
                TempData["Success"] = "درخواست شما با موفقیت در سیستم ثبت گردید که پس از بررسی نتیجه به شما اطلاع رسانی میشود .";
                return RedirectToAction("Index", "Cooperation");
            }
            TempData["Error"] = "متاسفانه خطایی رخ داده است لطفا اطلاعات خود را بررسی و مجدد تلاش نمایید";
            return RedirectToAction("Index", "Cooperation");
        }
    }
}