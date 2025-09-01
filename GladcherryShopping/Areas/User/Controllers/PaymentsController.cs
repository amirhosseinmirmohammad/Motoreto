using DataLayer.Models;
using DataLayer.ViewModels.PagerViewModel;
using GladcherryShopping.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace GladcherryShopping.Areas.User.Controllers
{
    [Authorize(Roles = "User")]
    public class PaymentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Administrator/Notifications
        public ActionResult Index(int page = 1)
        {
            string UserId = User.Identity.GetUserId();
            if (string.IsNullOrWhiteSpace(UserId))
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            var UserInPayments = db.Users.Include(current => current.Notifications).Where(current => current.Id == UserId && current.Payments.Count > 0).Include(current => current.Payments).FirstOrDefault();
            List<Payment> PaymentList = new List<Payment>();
            if (UserInPayments != null)
            {
                foreach (var item in UserInPayments.Payments.OrderByDescending(current => current.CreateDate))
                {
                    PaymentList.Add(item);
                }
            }
            PagerViewModels<Payment> NotificationsViewModels = new PagerViewModels<Payment>();
            NotificationsViewModels.CurrentPage = page;
            NotificationsViewModels.data = PaymentList.OrderByDescending(current => current.CreateDate).Skip((page - 1) * 10).Take(10).ToList();
            NotificationsViewModels.TotalItemCount = PaymentList.Count();
            return View(NotificationsViewModels);
        }
    }
}