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
    public class PreProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Administrator/PreProducts
        public ActionResult Index(int page = 1)
        {
            var PreProducts = db.PreProducts;
            PagerViewModels<PreProduct> PreProductViewModels = new PagerViewModels<PreProduct>();
            PreProductViewModels.CurrentPage = page;
            PreProductViewModels.data = PreProducts.OrderByDescending(current => current.Id).Skip((page - 1) * 10).Take(10).ToList();
            PreProductViewModels.TotalItemCount = PreProducts.Count();
            return View(PreProductViewModels);
        }

        // GET: Administrator/PreProducts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PreProduct PreProduct = db.PreProducts.Find(id);
            if (PreProduct == null)
            {
                return HttpNotFound();
            }
            return View(PreProduct);
        }

        // GET: Administrator/PreProducts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Administrator/PreProducts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Body")] PreProduct PreProduct)
        {
            if (ModelState.IsValid)
            {
                db.PreProducts.Add(PreProduct);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(PreProduct);
        }

        // GET: Administrator/PreProducts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PreProduct PreProduct = db.PreProducts.Find(id);
            if (PreProduct == null)
            {
                return HttpNotFound();
            }
            return View(PreProduct);
        }

        // POST: Administrator/PreProducts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Body")] PreProduct PreProduct)
        {
            if (ModelState.IsValid)
            {
                db.Entry(PreProduct).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(PreProduct);
        }

        // GET: Administrator/PreProducts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PreProduct PreProduct = db.PreProducts.Find(id);
            if (PreProduct == null)
            {
                return HttpNotFound();
            }
            return View(PreProduct);
        }

        // POST: Administrator/PreProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            PreProduct PreProduct = db.PreProducts.Find(id);
            db.PreProducts.Remove(PreProduct);
            try
            {
                db.SaveChanges();
            }
            catch (Exception)
            {
                TempData["Error"] = "به دلیل وجود وابستگی ها در سیستم امکان حذف این توضیحات وجود ندارد .";
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
