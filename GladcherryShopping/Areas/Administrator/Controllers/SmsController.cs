using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DataLayer.Models;
using DataLayer.ViewModels.PagerViewModel;
using System.IO;
using GladcherryShopping.Models;

namespace GladcherryShopping.Areas.Administrator.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class SmsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Administrator/Sms
        public ActionResult Index(int page = 1)
        {
            var sms = db.Sms;
            PagerViewModels<Sms> SmsViewModels = new PagerViewModels<Sms>();
            SmsViewModels.CurrentPage = page;
            SmsViewModels.data = sms.OrderByDescending(current => current.CreateDate).Skip((page - 1) * 10).Take(10).ToList();
            SmsViewModels.TotalItemCount = sms.Count();
            return View(SmsViewModels);
        }

        // GET: Administrator/Sms/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sms sms = db.Sms.Find(id);
            if (sms == null)
            {
                return HttpNotFound();
            }
            return View(sms);
        }

        // GET: Administrator/Sms/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Administrator/Sms/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Body")] Sms sms)
        {
            if (ModelState.IsValid)
            {
                sms.CreateDate = DateTime.Now;
                db.Sms.Add(sms);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(sms);
        }

        // GET: Administrator/Sms/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sms sms = db.Sms.Find(id);
            if (sms == null)
            {
                return HttpNotFound();
            }
            return View(sms);
        }

        // POST: Administrator/Sms/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Body")] Sms sms)
        {
            if (ModelState.IsValid)
            {
                db.Entry(sms).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(sms);
        }

        [HttpGet]
        public ActionResult UserSend(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Application application = db.Applications.FirstOrDefault();
            if (application != null)
            {
                try
                {
                    var role = db.Roles.Where(p => p.Name == "User").FirstOrDefault();
                    IQueryable<ApplicationUser> query = db.Users.Include(current => current.State).Include(current => current.City).Where(p => p.Roles.Any(a => a.RoleId == role.Id));
                    if (query.Count() > 0)
                    {
                        Sms model = db.Sms.Find(id);
                        IHtmlString htmlString = new HtmlString(model.Body);
                        //foreach (var item in db.Contractors.Where(current => current.IsActive == true).ToList())
                        //{
                        //    var request = (HttpWebRequest)WebRequest
                        //    .Create("http://webservice.falizsms.ir/newsmswebservice.asmx/Send?username=" + application.UserName + "&password=" + application.Password + "&message=" + htmlString + "&fromNumber=" + application.FromNumber + "&toNumbers=" + item.PhoneNumber);
                        //    var response = (HttpWebResponse)request.GetResponse();
                        //    var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                        //}
                        TempData["Success"] = " پیامک شما با موفقیت به  " + query.Count() + " کاربر فعال در سامانه ارسال شد . ";
                    }
                    else
                    {
                        TempData["Error"] = " هنوز کاربری در سیستم ثبت نشده است . ";
                    }
                }
                catch (Exception)
                {
                    TempData["Error"] = "خطایی رخ داده است لطفا مجدد تلاش فرمایید .";
                }
            }
            else
            {
                TempData["Error"] = " لطفا تنطیمات کلی برنامه خود را تعیین نمایید . ";
            }
            return RedirectToAction("Index");
        }
        // GET: Administrator/Sms/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Sms sms = db.Sms.Find(id);
            if (sms == null)
            {
                return HttpNotFound();
            }
            return View(sms);
        }

        // POST: Administrator/Sms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Sms sms = db.Sms.Find(id);
            db.Sms.Remove(sms);
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
