using DataLayer.Models;
using DataLayer.ViewModels;
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

namespace GladcherryShopping.Controllers
{
    public class ProductController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult List(string sort = "newest", string q = "", int page = 1, int pageSize = 24)
        {
            var query = BuildQuery(sort, q);

            var total = query.Count();
            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToList();

            ViewBag.Sort = sort;
            ViewBag.Q = q;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.HasMore = total > page * pageSize;

            return View(items);
        }

        public PartialViewResult ListPartial(string sort = "newest", string q = "", int page = 1, int pageSize = 24)
        {
            var query = BuildQuery(sort, q);

            var total = query.Count();
            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToList();

            ViewBag.Page = page;
            ViewBag.HasMore = total > page * pageSize;

            return PartialView("_ProductListPartial", items);
        }

        private IQueryable<Product> BuildQuery(string sort, string q)
        {
            var data = db.Products.AsQueryable();

            data = data.Where(p => p.Stock >= 0);

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                data = data.Where(p =>
                    p.PersianName.Contains(q) ||
                    p.EnglishName.Contains(q) ||
                    p.Description.Contains(q));
            }

            switch ((sort ?? "newest").ToLower())
            {
                case "cheap": data = data.OrderBy(p => p.UnitPrice); break;
                case "expensive": data = data.OrderByDescending(p => p.UnitPrice); break;
                case "bestseller": data = data.OrderByDescending(p => p.Orders.Count()); break; 
                case "discount": data = data.OrderByDescending(p => p.DiscountPercent); break;
                case "newest":
                default: data = data.OrderByDescending(p => p.CreateDate); break;
            }

            return data;
        }

        // GET: Product
        public ActionResult Details(string sefUrl)
        {
            if (string.IsNullOrEmpty(sefUrl))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = db.Products
                .Where(current => current.SefUrl == sefUrl)
                .Include(current => current.RelatedProducts)
                .Include(current => current.category)
                .Include(current => current.category.Parent)
                .Include(current => current.Images)
                .FirstOrDefault();

            if (product == null)
            {
                return HttpNotFound();
            }

            var cookie = new HttpCookie("SeenProduct_" + product.Id, "1")
            {
                Expires = DateTime.Now.AddMonths(1),
                HttpOnly = true
            };
            Response.Cookies.Add(cookie);

            ProductDetailViewModel viewmodel = new ProductDetailViewModel
            {
                product = product
            };
            return View(viewmodel);
        }

        [HttpGet]
        public ActionResult Search(string Search, int page = 1)
        {
            var products = db.Products.Where(current => current.PersianName.Contains(Search) || current.EnglishName.Contains(Search) || current.Description.Contains(Search));
            PagerViewModels<Product> ProductViewModels = new PagerViewModels<Product>();
            ProductViewModels.CurrentPage = page;
            ProductViewModels.data = products.OrderByDescending(current => current.CreateDate).ThenByDescending(current => current.PersianName).Skip((page - 1) * 10).Take(10).ToList();
            ProductViewModels.TotalItemCount = products.Count();
            return View("All", ProductViewModels);
        }

        [HttpGet]
        public ActionResult Filter(string Title, byte? Discount, int? Category, int? Min, int? Max, int page = 1)
        {
            List<Product> products = new List<Product>();
            IQueryable<Product> query = db.Products.Where(current => current.SiteFirstImage != null);
            if (!string.IsNullOrEmpty(Title))
            {
                query = query.Where(current => current.PersianName.Contains(Title) || current.Description.Contains(Title) || current.EnglishName.Contains(Title));
            }
            if (Category != null)
            {
                query = query.Where(current => current.CategoryId == Category);
            }
            if (Discount != null)
            {
                query = query.Where(current => current.DiscountPercent >= Discount);
            }
            if (Min != null)
            {
                query = query.Where(current => current.UnitPrice >= Min);
            }
            if (Max != null)
            {
                query = query.Where(current => current.UnitPrice <= Max);
            }
            PagerViewModels<Product> ProductViewModels = new PagerViewModels<Product>();
            ProductViewModels.CurrentPage = page;
            ProductViewModels.data = query.OrderByDescending(current => current.CreateDate).ThenByDescending(current => current.PersianName).Skip((page - 1) * 12).Take(12).ToList();
            ProductViewModels.TotalItemCount = query.Count();
            return View("All", ProductViewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InsertComment([Bind(Include = "Id,Text,DateTime,IsApprove,UserId,ProductId,Email,FullName")] Comment comment, long ProductId, string Email, string FullName)
        {
            if (User.Identity.IsAuthenticated)
            {
                string UserId = User.Identity.GetUserId();
                comment.UserId = UserId;
            }
            comment.Fullname = FullName;
            comment.Email = Email;
            comment.ProductId = ProductId;
            comment.DateTime = DateTime.Now;
            db.Comments.Add(comment);
            try
            {
                db.SaveChanges();
                TempData["Success"] = "نظر شما با موفقیت ثبت شد و در انتظار تایید ادمین میباشد .";
            }
            catch (Exception)
            {
                TempData["error"] = "خطایی رخ داده است لطفا مجدد تلاش فرمایید .";
            }
            return RedirectToAction("Details", new { id = comment.ProductId });
        }

        [Route("product/bycar/{name}")]
        public ActionResult ByCar(string name, int page = 1, int pageSize = 8)
        {
            // دسته‌بندی‌ها
            var baseCategories = db.Categories
                                   .Where(c => c.PersianName == name)
                                   .Select(c => c.Id)
                                   .ToList();

            var allCategoryIds = db.Categories
                                   .Where(c => baseCategories.Contains(c.Id) || baseCategories.Contains(c.ParentId ?? 0))
                                   .Select(c => c.Id)
                                   .ToList();

            var query = db.Products.Where(p => allCategoryIds.Contains(p.CategoryId));

            var products = query
                           .OrderByDescending(p => p.CreateDate)
                           .Skip((page - 1) * pageSize)
                           .Take(pageSize)
                           .ToList();

            var vm = new PagerViewModels<Product>
            {
                CurrentPage = page,
                TotalItemCount = query.Count(),
                data = products
            };

            ViewBag.CarName = name; 

            return View("All", vm);
        }

        [HttpGet]
        public ActionResult LoadMoreProducts(int page, int pageSize, string q = "", string sort = "new", string filter = "")
        {
            var query = db.Products.Include(p => p.Orders).AsQueryable();

            // سرچ
            if (!string.IsNullOrEmpty(q))
            {
                query = query.Where(p => p.PersianName.Contains(q) || p.EnglishName.Contains(q));
            }

            // فیلتر چیپ‌ها
            if (filter == "inStock")
                query = query.Where(p => p.Stock > 0);
            else if (filter == "special")
                query = query.Where(p => p.IsSpecial); // فرض کن فیلد IsSpecial داری
            else if (filter == "hasImage")
                query = query.Where(p => !string.IsNullOrEmpty(p.SiteFirstImage));

            // مرتب‌سازی
            switch (sort)
            {
                case "bestsellers":
                    query = query.OrderByDescending(p => p.Orders.Count()); // باید فیلد فروش داشته باشی
                    break;
                case "expensive":
                    query = query.OrderByDescending(p =>
                        (p.DiscountPercent > 0)
                            ? (p.UnitPrice - (p.UnitPrice * p.DiscountPercent / 100))
                            : p.UnitPrice);
                    break;

                case "cheap":
                    query = query.OrderBy(p =>
                        (p.DiscountPercent > 0)
                            ? (p.UnitPrice - (p.UnitPrice * p.DiscountPercent / 100))
                            : p.UnitPrice);
                    break;
                case "discount":
                    query = query.OrderByDescending(p => p.DiscountPercent);
                    break;
                default: // new
                    query = query.OrderByDescending(p => p.Id); // یا تاریخ ثبت محصول
                    break;
            }

            var products = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return PartialView("_ProductListPartial", products);
        }

        public ActionResult All(string Search, int? id, int page = 1)
        {
            PagerViewModels<Product> ProductViewModels = new PagerViewModels<Product>();
            var query = db.Products.AsQueryable();

            if (id != null)
            {
                var categoryIds = db.Categories
                                    .Where(c => c.Id == id || c.ParentId == id)
                                    .Select(c => c.Id)
                                    .ToList();

                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            if (!string.IsNullOrEmpty(Search))
            {
                query = query.Where(current =>
                    current.PersianName.Contains(Search) ||
                    current.EnglishName.Contains(Search) ||
                    current.Description.Contains(Search));
            }

            var products = query.ToList();

            ProductViewModels.CurrentPage = page;
            ProductViewModels.data = products;
            ProductViewModels.TotalItemCount = products.Count();

            var category = db.Categories
                             .FirstOrDefault(c => c.Id == id || c.ParentId == id);

            ViewBag.CarName = category?.PersianName;

            return View(ProductViewModels);
        }

        public ActionResult Special(string Search, int page = 1)
        {
            PagerViewModels<Product> ProductViewModels = new PagerViewModels<Product>();
            var products = new List<Product>();
            if (!string.IsNullOrEmpty(Search))
            {
                products = db.Products.Where(current => current.PersianName.Contains(Search) || current.EnglishName.Contains(Search) || current.Description.Contains(Search)).ToList();
            }
            else
            {
                products = db.Products.Where(current => current.IsSpecial == true).ToList();
                ProductViewModels.data = products.Where(current => current.IsSpecial == true).OrderByDescending(current => current.CreateDate).ThenByDescending(current => current.PersianName).Skip((page - 1) * 10).Take(10).ToList();
                ProductViewModels.TotalItemCount = products.Where(current => current.IsSpecial == true).Count();
            }
            ProductViewModels.CurrentPage = page;
            return View(ProductViewModels);
        }
    }
}