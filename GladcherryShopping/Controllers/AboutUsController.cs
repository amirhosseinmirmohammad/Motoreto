using DataLayer.Models;
using GladcherryShopping.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace GladcherryShopping.Controllers
{
    public class AboutUsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: AboutUs
        public ActionResult Index()
        {
            Application app = db.Applications.FirstOrDefault();
            return View(app);
        }
    }
}