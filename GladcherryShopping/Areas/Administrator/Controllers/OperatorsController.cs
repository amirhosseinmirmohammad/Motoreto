using DataLayer.ViewModels.PagerViewModel;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using GladcherryShopping.Models;
using DataLayer.ViewModels;
using GladCherryShopping.Helpers;

namespace GladcherryShopping.Areas.Administrator.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class OperatorsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Administrator/Operators
        public ActionResult Index(int page = 1)
        {
            var role = db.Roles.Where(p => p.Name == "Operator").FirstOrDefault();
            IQueryable<ApplicationUser> query = db.Users.Include(current => current.State).Include(current => current.City).Where(p => p.Roles.Any(a => a.RoleId == role.Id));
            PagerViewModels<ApplicationUser> UserViewModels = new PagerViewModels<ApplicationUser>();
            UserViewModels.CurrentPage = page;
            UserViewModels.data = query.OrderByDescending(current => current.RegistrationDate).ThenByDescending(current => current.StateId).ThenByDescending(current => current.CityId).ThenByDescending(current => current.LastName).ThenByDescending(current => current.FirstName).Skip((page - 1) * 10).Take(10).ToList();
            UserViewModels.TotalItemCount = query.ToList().Count();
            return View(UserViewModels);
        }
        [HttpGet]
        public ActionResult AddOperator()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddOperator(OperatorViewModel Operator, HttpPostedFileBase image)
        {
            IdentityResult identityResult;
            IUserStore<ApplicationUser> userstore = new UserStore<ApplicationUser>(db);
            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(userstore);
            ApplicationUser user = new ApplicationUser { UserName = Operator.Email, Email = Operator.Email, Mobile = Operator.Mobile, AddressLine = Operator.AddressLine, IsActive = Operator.IsActive, FirstName = Operator.FirstName, LastName = Operator.LastName, BirthDate = DateTime.Now, RegistrationDate = DateTime.Now, CityId = 1, StateId = 1 };
            #region FileUploading
            if (image != null && image.ContentLength > 0)
            {
                string Path = FunctionsHelper.File(FunctionsHelper.FileMode.Upload, FunctionsHelper.FileType.Image, "~/content/images/Operators/", true, image, Server);
                if (Path != string.Empty)
                {
                    user.ProfileImage = Path;
                }
            }
            #endregion FileUploading
            try
            {
                identityResult = await UserManager.CreateAsync(user, Operator.Password);
            }
            catch (Exception)
            {
                TempData["Error"] = "خطایی رخ داده است لطفا مجدد تلاش فرمایید .";
                return View();
            }

            if (identityResult.Succeeded)
            {
                UserManager.AddToRole(user.Id, "Operator");
                TempData["Success"] = "اپراتور شما با موفقیت در سیستم ثبت گردید .";
            }
            else
            {
                TempData["Error"] = "اپراتور شما قبلا در سیستم ثبت شده است .";
                return View(Operator);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> EditOperator(string id)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> userstore = new UserStore<ApplicationUser>(context);
            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(userstore);
            var user = await UserManager.FindByIdAsync(id);
            ResetOperatorViewModel model = new ResetOperatorViewModel();
            //model.Email = user.Email;
            model.Mobile = user.Mobile;
            model.FirstName = user.FirstName;
            //model.Email = user.Email;
            model.AddressLine = user.AddressLine;
            //model.Password = user.PasswordHash;
            model.LastName = user.LastName;
            model.Mobile = user.Mobile;
            model.IsActive = user.IsActive;
            model.UserId = id;
            model.ProfileImage = user.ProfileImage;
            return View(model: model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditOperator(ResetOperatorViewModel model,HttpPostedFileBase image)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            IUserStore<ApplicationUser> userstore = new UserStore<ApplicationUser>(context);
            UserManager<ApplicationUser> UserManager = new UserManager<ApplicationUser>(userstore);
            var user = await UserManager.FindByIdAsync(model.UserId);
            //PasswordHasher hasher = new PasswordHasher();
            //var hash = hasher.HashPassword(model.Password);
            //user.PasswordHash = hash;
            ApplicationUser editedUser = user;
            //user.Email = model.Email;
            user.Mobile = model.Mobile;
            user.FirstName = model.FirstName;
            //user.Email = model.Email;
            user.AddressLine = model.AddressLine;
            //user.PasswordHash = model.Password;
            user.LastName = model.LastName;
            user.PhoneNumber = model.Mobile;
            user.IsActive = model.IsActive;

            #region FileUpdating

            if (image != null && image.ContentLength > 0)
            {
                string Path = string.Empty;

                if (user.ProfileImage == null)
                {
                    Path = FunctionsHelper.File(FunctionsHelper.FileMode.Upload, FunctionsHelper.FileType.Image, filePath: "~/content/images/Operators/", alterName: true, file: image, Server: Server);
                }
                else
                {
                    Path = FunctionsHelper.File(FunctionsHelper.FileMode.Update, FunctionsHelper.FileType.Image, filePath: user.ProfileImage, alterName: true, file: image, Server: Server);
                }

                if (Path != string.Empty)
                {
                    user.ProfileImage = Path;
                }
            }

            #endregion FileUpdating

            try
            {
                UserManager.Update(editedUser);
                context.SaveChanges();
            }
            catch (Exception)
            {
                TempData["Error"] = "خطایی رخ داده است لطفا مجدد تلاش فرمایید .";
                return View(model);
            }
            TempData["Success"] = "اطلاعات اپراتور شما با موفقیت ویرایش شد .";
            return RedirectToAction("Index");
        }

        public ActionResult SearchOperator()
        {
            List<ApplicationUser> users = new List<ApplicationUser>();
            ViewBag.ReturnUrl = "/Administrator/Operators/SearchOperator";
            return View(users);
        }

        [HttpGet]
        public ActionResult OperatorResult(string Name, string returnUrl)
        {
            ApplicationDbContext context = new ApplicationDbContext();
            var OperatorRole = context.Roles.Where(p => p.Name == "Operator").FirstOrDefault();
            var query = context.Users.Where(p => p.FirstName.Contains(Name) || p.LastName.Contains(Name)).Include(current => current.Roles);
            if (query != null)
            {
                PagerViewModels<ApplicationUser> pageviewmodel = new PagerViewModels<ApplicationUser>();
                pageviewmodel.CurrentPage = 1;
                pageviewmodel.data = query.Include(current => current.State).Include(current => current.City).ToList();
                pageviewmodel.TotalItemCount = 1;
                return View("Index", pageviewmodel);
            }
            TempData["Error"] = " متاسفانه اپراتوری با نام " + Name + " یافت نشد . ";
            return Redirect(returnUrl);
        }

        [HttpGet]
        public ActionResult DeleteOperator(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            return View(user);
        }

        [HttpGet]
        public ActionResult DeleteOperatorConfirmed(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            var ImagePath = Server.MapPath(user.ProfileImage);
            if (System.IO.File.Exists(ImagePath))
            {
                System.IO.File.Delete(ImagePath);
            }
            try
            {
                db.Users.Remove(user);
                db.SaveChanges();
            }
            catch (Exception)
            {
                TempData["Error"] = "به دلیل وجود زیر شاخه ها امکان حذف این اپراتور وجود ندارد .";
                return View();
            }
            return RedirectToAction("Index");
        }
    }
}