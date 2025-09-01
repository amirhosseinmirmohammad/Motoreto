using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DataLayer.Models;
using DataLayer.ViewModels.PagerViewModel;
using static GladCherryShopping.Helpers.FunctionsHelper;
using GladcherryShopping.Models;
using GladCherryShopping.Helpers;

namespace GladcherryShopping.Areas.Administrator.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class QrEntitiesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Administrator/QrEntities
        public ActionResult Index(int page = 1)
        {
            var QrEntities = db.QrEntities;
            PagerViewModels<QrEntity> QrEntityViewModels = new PagerViewModels<QrEntity>();
            QrEntityViewModels.CurrentPage = page;
            QrEntityViewModels.data = QrEntities.OrderByDescending(current => current.ProductId).Skip((page - 1) * 10).Take(10).ToList();
            QrEntityViewModels.TotalItemCount = QrEntities.Count();
            return View(QrEntityViewModels);
        }

        // GET: Administrator/QrEntities/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QrEntity QrEntity = db.QrEntities.Find(id);
            if (QrEntity == null)
            {
                return HttpNotFound();
            }
            return View(QrEntity);
        }

        // GET: Administrator/QrEntities/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Administrator/QrEntities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,QrCode,SerialNumber,ProductId")] QrEntity QrEntity)
        {
            if (ModelState.IsValid)
            {
                Product pro = db.Products.Find(QrEntity.Id);
                if (pro == null)
                {
                    TempData["Error"] = "محصول مورد نظر یافت نشد .";
                    return RedirectToAction("Index");
                }
                db.QrEntities.Add(QrEntity);
                db.SaveChanges();
                TempData["Success"] = "کیو آر کد مورد نظر شما با موفقیت در سامانه ثبت شد .";
                return RedirectToAction("Index");
            }
            return View(QrEntity);
        }

        // GET: Administrator/QrEntities/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QrEntity QrEntity = db.QrEntities.Find(id);
            if (QrEntity == null)
            {
                return HttpNotFound();
            }
            return View(QrEntity);
        }

        // POST: Administrator/QrEntities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,QrCode,SerialNumber,ProductId")] QrEntity QrEntity)
        {
            if (ModelState.IsValid)
            {
                Product pro = db.Products.Find(QrEntity.Id);
                if (pro == null)
                {
                    TempData["Error"] = "محصول مورد نظر یافت نشد .";
                    return RedirectToAction("Index");
                }
                db.Entry(QrEntity).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "کیو آر کد مورد نظر شما با موفقیت در سامانه ویرایش شد .";
                return RedirectToAction("Index");
            }
            return View(QrEntity);
        }

        // GET: Administrator/QrEntities/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            QrEntity QrEntity = db.QrEntities.Find(id);
            if (QrEntity == null)
            {
                return HttpNotFound();
            }
            return View(QrEntity);
        }

        // POST: Administrator/QrEntities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            QrEntity QrEntity = db.QrEntities.Find(id);
            db.QrEntities.Remove(QrEntity);
            try
            {
                db.SaveChanges();
            }
            catch (Exception)
            {
                TempData["Error"] = "به دلیل وجود وابستگی ها در سیستم امکان حذف این کیو آر کد وجود ندارد .";
                return RedirectToAction("Index");
            }
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
