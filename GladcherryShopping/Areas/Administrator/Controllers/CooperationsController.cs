using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DataLayer.Models;
using GladcherryShopping.Models;
using DataLayer.ViewModels.PagerViewModel;

namespace GladcherryShopping.Areas.Administrator.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class CooperationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Administrator/Cooperations
        public ActionResult Index(int page = 1)
        {
            var list = db.Cooperations;
            PagerViewModels<Cooperation> CategoryViewModels = new PagerViewModels<Cooperation>();
            CategoryViewModels.CurrentPage = page;
            CategoryViewModels.data = list.OrderByDescending(current => current.Id).Skip((page - 1) * 10).Take(10).ToList();
            CategoryViewModels.TotalItemCount = list.Count();
            return View(CategoryViewModels);
        }

        // GET: Administrator/Cooperations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cooperation cooperation = db.Cooperations.Find(id);
            if (cooperation == null)
            {
                return HttpNotFound();
            }
            return View(cooperation);
        }

        // GET: Administrator/Cooperations/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Administrator/Cooperations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FullName,NationalCode,Bithdate,Education,Field,Body,Company,Duration,PhoneCo,Fax,EmailCo,WebsiteCo,Instagram,Telegram,Situation,PostalCode,Guaranteed,PostalAddress,Prediction,Agency,Phone,Mobile,Email,Website,Address")] Cooperation cooperation)
        {
            if (ModelState.IsValid)
            {
                db.Cooperations.Add(cooperation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(cooperation);
        }

        // GET: Administrator/Cooperations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cooperation cooperation = db.Cooperations.Find(id);
            if (cooperation == null)
            {
                return HttpNotFound();
            }
            return View(cooperation);
        }

        // POST: Administrator/Cooperations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FullName,NationalCode,Bithdate,Education,Field,Body,Company,Duration,PhoneCo,Fax,EmailCo,WebsiteCo,Instagram,Telegram,Situation,PostalCode,Guaranteed,PostalAddress,Prediction,Agency,Phone,Mobile,Email,Website,Address")] Cooperation cooperation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cooperation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(cooperation);
        }

        // GET: Administrator/Cooperations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cooperation cooperation = db.Cooperations.Find(id);
            if (cooperation == null)
            {
                return HttpNotFound();
            }
            return View(cooperation);
        }

        // POST: Administrator/Cooperations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Cooperation cooperation = db.Cooperations.Find(id);
            db.Cooperations.Remove(cooperation);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
