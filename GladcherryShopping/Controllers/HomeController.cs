using DataLayer.Models;
using DataLayer.ViewModels;
using DataLayer.ViewModels.PagerViewModel;
using GladcherryShopping.Models;
using GladcherryShopping.ParsianSale;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Diagnostics;
using System.Net.Security;

namespace GladcherryShopping.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetCity(int stateId)
        {
            var cities = db.Cities.Where(current => current.StateId == stateId).OrderByDescending(current => current.Name);
            var jsonResult = cities.Select(result => new { id = result.Id, name = result.Name });
            return Json(jsonResult, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitMessage([Bind(Include = "Id,FullName,Body,Email")] SiteMessage model)
        {
            if (ModelState.IsValid)
            {
                model.RegisterDate = DateTime.Now;
                db.SiteMessages.Add(model);
                db.SaveChanges();
                TempData["Success"] = "پیام شما با موفقیت در سیستم ثبت گردید";
                return RedirectToAction("Index", "ContactUs");
            }
            TempData["Error"] = "متاسفانه خطایی رخ داده است لطفا اطلاعات خود را بررسی و مجدد تلاش نمایید";
            return RedirectToAction("Index", "ContactUs");
        }

        [HttpGet]
        public JsonResult NewsLetter([Bind(Include = "Email")] string text, NewsLetter newsletter)
        {
            if (text == string.Empty || text == null)
            {
                return Json(new { text = "لطفا ایمیل خود را وارد نمایید .", status = 0 }, JsonRequestBehavior.AllowGet);
            }
            if (!new EmailAddressAttribute().IsValid(text))
            {
                return Json(new { text = "لطفا ایمیل معتبری را وارد نمایید .", status = 0 }, JsonRequestBehavior.AllowGet);
            }

            var email = db.NewsLetters.Where(n => n.Email == text).FirstOrDefault();
            if (email != null)
            {
                return Json(new { text = "ایمیل شما قبلا در سیستم ثبت شده است .", status = 0 }, JsonRequestBehavior.AllowGet);
            }

            newsletter.Email = text;
            db.NewsLetters.Add(newsletter);
            try
            {
                db.SaveChanges();
                return Json(new { text = "ایمیل شما با موفقیت در سیستم ثبت شد .", status = 1 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                return Json(new { text = "خطایی رخ داده است لطفا مجدد تلاش فرمایید .", status = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetDiscount(string Code)
        {

            #region Validation
            if (string.IsNullOrEmpty(Code))
            {
                return Json(new { Status = false, Text = "لطفا کد خود را وارد نمایید ." }, JsonRequestBehavior.AllowGet);
            }
            IQueryable<Discount> query = db.Discounts.Where(currrent => currrent.Code == Code && currrent.IsActived == true);
            if (query.Count() == 0)
            {
                return Json(new { Status = false, Text = "کد وارد شده معتبر نمیباشد ." }, JsonRequestBehavior.AllowGet);
            }
            if (query.Count() == 1 && query.FirstOrDefault().IsActived == false)
            {
                return Json(new { Status = false, Text = "کد وارد شده غیر فعال شده است ." }, JsonRequestBehavior.AllowGet);
            }
            Discount dis = query.FirstOrDefault();
            if (dis == null)
            {
                return Json(new { Status = false, Text = "کد تخفیف مورد نظر شما پیدا نشد ." }, JsonRequestBehavior.AllowGet);
            }
            if (dis.Count == dis.MaxCount)
            {
                if (dis.IsActived == true)
                {
                    dis.IsActived = false;
                    db.Entry(dis).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return Json(new { Status = false, Text = "ظرفیت استفاده از این کد به پایان رسیده است ." }, JsonRequestBehavior.AllowGet);
            }
            #endregion Validation

            #region Login
            //اگر عضو بود و ورود کرده بود
            if (User.Identity.IsAuthenticated == true)
            {
                string userId = User.Identity.GetUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { Status = false, Text = "کد کاربری شما وجود ندارد ." }, JsonRequestBehavior.AllowGet);
                }
                ApplicationUser user = db.Users.Where(current => current.Id == userId).Include(current => current.Discounts).FirstOrDefault();
                if (user == null)
                {
                    return Json(new { Status = false, Text = "حساب کاربری شما پیدا نشد ." }, JsonRequestBehavior.AllowGet);
                }
                if (user.Discounts.Contains(dis) || Request.Cookies.AllKeys.Contains("Discount_" + dis.Id.ToString()))
                {
                    return Json(new { Status = false, Text = "شما قبلا از این کد تخفیف استفاده کرده اید ." }, JsonRequestBehavior.AllowGet);
                }

                #region Cookie
                //Add Cookie
                HttpCookie cookie = new HttpCookie("cookie");
                if (dis.IsPercentage)
                {
                    cookie = new HttpCookie("Discount_" + dis.Id.ToString(), dis.Percent.ToString());
                }
                else
                {
                    cookie = new HttpCookie("Discount_" + dis.Id.ToString(), dis.Amount.ToString());
                }
                cookie.Expires = DateTime.Now.AddMonths(1);
                cookie.HttpOnly = true;
                Response.Cookies.Add(cookie);
                #endregion Cookie

                user.Discounts.Add(dis);
                db.Entry(user).State = EntityState.Modified;
            }
            #endregion Login

            else
            {
                #region Guest
                if (Request.Cookies.AllKeys.Contains("Discount_" + dis.Id.ToString()))
                {
                    return Json(new { Status = false, Text = "شما قبلا از این کد تخفیف استفاده کرده اید ." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    #region Cookie
                    //Add Cookie
                    HttpCookie cookie = new HttpCookie("cookie");
                    if (dis.IsPercentage)
                    {
                        cookie = new HttpCookie("Discount_" + dis.Id.ToString(), dis.Percent.ToString());
                    }
                    else
                    {
                        cookie = new HttpCookie("Discount_" + dis.Id.ToString(), dis.Amount.ToString());
                    }
                    cookie.Expires = DateTime.Now.AddMonths(1);
                    cookie.HttpOnly = true;
                    Response.Cookies.Add(cookie);
                    #endregion Cookie
                }
                #endregion Guest
            }

            dis.Count++;
            db.Entry(dis).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
                if (query.FirstOrDefault().IsPercentage)
                {
                    var discountPrice = double.Parse(CartPrice()) - double.Parse(CartPrice()) * (dis.Percent) / 100;
                    return Json(new { Status = true, PriceHtml = discountPrice, Percent = dis.Percent, Text = "تخفیف شما با مقدار " + dis.Percent + " درصد بر روی سفارش شما اعمال خواهد شد ." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var discountPrice = double.Parse(CartPrice()) - double.Parse(dis.Amount);
                    return Json(new { Status = true, PriceHtml = discountPrice, Percent = dis.Amount, Text = "تخفیف شما با مقدار " + dis.Amount + " ریال بر روی سفارش شما اعمال خواهد شد ." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { Status = false, Text = "خطایی رخ داده است لطفا مجدد تلاش فرمایید ." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Products(int? id, string Title, int page = 1)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Where(current => current.Id == id).Include(current => current.SubCategories).FirstOrDefault();
            if (category == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                PagerViewModels<Product> ProductViewModels = new PagerViewModels<Product>();
                ViewBag.CategoryName = category.PersianName.ToString();
                if (category.SubCategories.Count() > 0)
                {
                    ViewBag.CategoryId = category.Id;
                }
                else
                {
                    var products = new List<Product>();
                    if (!string.IsNullOrEmpty(Title))
                    {
                        products = db.Products.Include(current => current.Orders).Where(current => current.CategoryId == id && current.PersianName.Contains(Title) || current.EnglishName.Contains(Title) || current.Description.Contains(Title)).ToList();
                    }
                    else
                    {
                        products = db.Products.Include(current => current.Orders).Where(current => current.CategoryId == id).ToList();
                    }
                    ProductViewModels.CurrentPage = page;
                    ProductViewModels.data = products.OrderByDescending(current => current.CreateDate).ThenByDescending(current => current.PersianName).Skip((page - 1) * 12).Take(12).ToList();
                    ProductViewModels.TotalItemCount = products.Count();
                }
                return View(ProductViewModels);
            }
        }

        public ActionResult Filter(string Title, int page = 1)
        {
            ViewBag.CategoryName = "محصولات موتورتو";
            PagerViewModels<Product> ProductViewModels = new PagerViewModels<Product>();
            var products = db.Products.Include(current => current.Orders).Where(current => current.PersianName.Contains(Title) || current.EnglishName.Contains(Title) || current.Description.Contains(Title)).ToList();
            ProductViewModels.CurrentPage = page;
            ProductViewModels.data = products.OrderByDescending(current => current.CreateDate).ThenByDescending(current => current.PersianName).Skip((page - 1) * 12).Take(12).ToList();
            ProductViewModels.TotalItemCount = products.Count();
            return View(ProductViewModels);
        }

        [HttpPost]
        public JsonResult AddToShoppingCart(int Id)
        {
            try
            {
                string cookieKey = "MotoretoCt_" + Id;
                int count = 0;

                if (Request.Cookies[cookieKey] != null)
                {
                    int.TryParse(Request.Cookies[cookieKey].Value, out count);
                    count++;
                }
                else
                {
                    var pro = db.Products.Find(Id);
                    if (pro == null)
                    {
                        return Json(new CardViewModel
                        {
                            Success = false,
                            CountHtml = "",
                            PriceHtml = ""
                        });
                    }

                    count = (int)((pro.Min > 0) ? pro.Min : 1);
                }

                var cookie = new HttpCookie(cookieKey, count.ToString())
                {
                    Expires = DateTime.Now.AddHours(3),
                    HttpOnly = false
                };
                Response.Cookies.Set(cookie);

                return Json(new CardViewModel
                {
                    Success = true,
                    CountHtml = CartCount(),
                    PriceHtml = CartPrice()
                });
            }
            catch
            {
                return Json(new CardViewModel
                {
                    Success = false,
                    CountHtml = "",
                    PriceHtml = ""
                });
            }
        }

        [HttpPost]
        public JsonResult MinusFromShoppingCart(int Id)
        {
            try
            {
                string cookieKey = "MotoretoCt_" + Id;

                if (Request.Cookies[cookieKey] != null)
                {
                    int count;
                    int.TryParse(Request.Cookies[cookieKey].Value, out count);

                    var pro = db.Products.Find(Id);
                    int minCount = (int)((pro != null && pro.Min > 0) ? pro.Min : 1);

                    if (count > minCount)
                    {
                        count--;

                        var cookie = new HttpCookie(cookieKey, count.ToString())
                        {
                            Expires = DateTime.Now.AddHours(3),
                            HttpOnly = false
                        };
                        Response.Cookies.Set(cookie);

                        return Json(new CardViewModel
                        {
                            Success = true,
                            CountHtml = CartCount(),
                            PriceHtml = CartPrice()
                        });
                    }
                    else
                    {
                        return Json(new CardViewModel
                        {
                            Success = false,
                            CountHtml = CartCount(),
                            PriceHtml = CartPrice()
                        });
                    }
                }

                return Json(new CardViewModel
                {
                    Success = false,
                    CountHtml = "",
                    PriceHtml = ""
                });
            }
            catch
            {
                return Json(new CardViewModel
                {
                    Success = false,
                    CountHtml = "",
                    PriceHtml = ""
                });
            }
        }

        [HttpPost]
        public JsonResult RemoveCart(int Id)
        {
            try
            {
                if (Request.Cookies.AllKeys.Contains("MotoretoCt_" + Id.ToString()))
                {
                    Response.Cookies["MotoretoCt_" + Id.ToString()].Expires = DateTime.Now.AddDays(-1);
                    Request.Cookies.Remove("MotoretoCt_" + Id.ToString());
                    return Json(new CardViewModel()
                    {
                        Success = true,
                        CountHtml = CartCount(),
                        PriceHtml = CartPrice()
                    });
                }
                else
                {
                    return Json(new CardViewModel()
                    {
                        Success = false,
                        CountHtml = CartCount(),
                        PriceHtml = CartPrice()
                    });
                }
            }
            catch (Exception)
            {
                return Json(new CardViewModel()
                {
                    Success = false,
                    CountHtml = "",
                    PriceHtml = ""
                });
            }
        }

        public string CartCount()
        {
            List<HttpCookie> lst = new List<HttpCookie>();
            for (int i = Request.Cookies.Count - 1; i >= 0; i--)
            {
                if (lst.Where(p => p.Name == Request.Cookies[i].Name).Any() == false)
                    lst.Add(Request.Cookies[i]);
            }
            int CartCount = lst.Where(p => p.Name.StartsWith("MotoretoCt_")).Count();
            return CartCount.ToString();
        }

        public string CartPrice()
        {
            List<HttpCookie> lst = new List<HttpCookie>();
            for (int i = Request.Cookies.Count - 1; i >= 0; i--)
            {
                if (lst.Where(p => p.Name == Request.Cookies[i].Name).Any() == false)
                    lst.Add(Request.Cookies[i]);
            }
            int TotalPrice = 0;
            foreach (var item in lst.Where(p => p.Name.StartsWith("MotoretoCt_")))
            {
                string idstring = item.Name.Substring(11);
                int id = Convert.ToInt32(idstring);
                int CartCount = Convert.ToInt32(item.Value);
                Product product = db.Products.Find(id);
                if (product != null)
                {
                    if (product.DiscountPercent > 0)
                    {
                        var discountPrice = product.UnitPrice - (product.UnitPrice) * (product.DiscountPercent) / 100;
                        TotalPrice += CartCount * discountPrice;
                    }
                    else
                    {
                        TotalPrice += CartCount * product.UnitPrice;
                    }
                }
                else
                {
                    continue;
                }
            }
            string x = string.Format("{0:n0}", TotalPrice.ToString());
            return TotalPrice.ToString("N0").Replace(System.Threading.Thread.CurrentThread.CurrentCulture.NumberFormat.NumberGroupSeparator, "");
        }

        [HttpGet]
        public ActionResult Cart()
        {
            List<BasketViewModel> listBasket = new List<BasketViewModel>();
            List<HttpCookie> lst = new List<HttpCookie>();
            for (int i = Request.Cookies.Count - 1; i >= 0; i--)
            {
                if (lst.Where(p => p.Name == Request.Cookies[i].Name).Any() == false)
                    lst.Add(Request.Cookies[i]);
            }
            foreach (var item in lst.Where(p => p.Name.StartsWith("MotoretoCt_")))
            {
                Product pro = db.Products.Find(Convert.ToInt32(item.Name.Substring(11)));
                if (pro != null)
                {
                    listBasket.Add(new BasketViewModel
                    {
                        Product = db.Products.Find(Convert.ToInt32(item.Name.Substring(11))),
                        Count = Convert.ToInt32(item.Value)
                    });
                }
            }

            List<SeenViewModel> lstSeen = new List<SeenViewModel>();
            //foreach (var item in lst.Where(p => p.Name.StartsWith("SeenProduct_")))
            //{
            //    Product pro = db.Products.Find(Convert.ToInt32(item.Name.Substring(11)));
            //    if (pro != null)
            //    {
            //        lstSeen.Add(new SeenViewModel
            //        {
            //            Product = db.Products.Find(Convert.ToInt32(item.Name.Substring(11))),
            //            Count = Convert.ToInt32(item.Value)
            //        });
            //    }
            //}
            ViewBag.SeenProducts = lstSeen.Take(2);
            return View(listBasket);
        }

        [HttpPost]
        public JsonResult GetBasket()
        {
            List<Product> productlist = new List<Product>();

            List<HttpCookie> lst = new List<HttpCookie>();
            for (int i = Request.Cookies.Count - 1; i >= 0; i--)
            {
                if (lst.Where(p => p.Name == Request.Cookies[i].Name).Any() == false)
                    lst.Add(Request.Cookies[i]);
            }
            foreach (var item in lst.Where(p => p.Name.StartsWith("MotoretoCt_")))
            {
                string idstring = item.Name.Substring(11);
                int id = Convert.ToInt32(idstring);
                Product product = db.Products.Find(id);
                if (product != null)
                {
                    productlist.Add(product);
                }
            }
            return new JsonResult { Data = productlist.Select(current => new { Product = current.PersianName, Count = productlist.Count }), JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        [HttpGet]
        public ActionResult InsertOrder(byte? PaymentType, byte? Type)
        {
            ViewBag.StateId = new SelectList(db.States.OrderByDescending(current => current.Name), "Id", "Name");
            if (!User.Identity.IsAuthenticated)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (PaymentType == null || PaymentType == 0 || Type == null || Type == 0)
            {
                TempData["error"] = "لطفا روش ارسال و پرداخت را تعیین نمایید .";
                return RedirectToAction("Cart", "Home");
            }
            if (Convert.ToInt32(CartPrice()) == 0)
            {
                return RedirectToAction("Cart");
            }
            List<BasketViewModel> listBasket = new List<BasketViewModel>();
            List<HttpCookie> lst = new List<HttpCookie>();
            for (int i = Request.Cookies.Count - 1; i >= 0; i--)
            {
                if (lst.Where(p => p.Name == Request.Cookies[i].Name).Any() == false)
                    lst.Add(Request.Cookies[i]);
            }
            foreach (var item in lst.Where(p => p.Name.StartsWith("MotoretoCt_")))
            {
                listBasket.Add(new BasketViewModel
                {
                    Product = db.Products.Find(Convert.ToInt32(item.Name.Substring(11))),
                    Count = Convert.ToInt32(item.Value)
                });
            }

            List<DiscountViewModel> listDiscount = new List<DiscountViewModel>();
            List<HttpCookie> lstDiscount = new List<HttpCookie>();
            for (int i = Request.Cookies.Count - 1; i >= 0; i--)
            {
                if (lst.Where(p => p.Name == Request.Cookies[i].Name).Any() == false)
                    lst.Add(Request.Cookies[i]);
            }

            List<SeenViewModel> lstSeen = new List<SeenViewModel>();
            //foreach (var item in lst.Where(p => p.Name.StartsWith("SeenProduct_")))
            //{
            //    lstSeen.Add(new SeenViewModel
            //    {
            //        Product = db.Products.Find(Convert.ToInt32(item.Name.Substring(11))),
            //        Count = Convert.ToInt32(item.Value)
            //    });
            //}
            ViewBag.SeenProducts = lstSeen.Take(2);
            SubmitOrderViewModel viewmodel = new SubmitOrderViewModel();
            viewmodel.basket = listBasket;
            List<Product> BasketPros = new List<Product>();
            foreach (var item in listBasket)
            {
                BasketPros.Add(item.Product);
            }
            viewmodel.Products = BasketPros;
            viewmodel.discount = listDiscount;
            viewmodel.FinalPrice = double.Parse(CartPrice());
            ViewBag.Price = CartPrice();
            return View(viewmodel);
        }

        private void ClearCartCookies()
        {
            var cookies = Request.Cookies.AllKeys.Where(k => k.StartsWith("MotoretoCt_")).ToList();
            foreach (var cookieKey in cookies)
            {
                var cookie = new HttpCookie(cookieKey)
                {
                    Expires = DateTime.Now.AddDays(-1), 
                    Value = null
                };
                Response.Cookies.Set(cookie);
            }
        }

        [HttpPost, ActionName("SubmitOrder")]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitOrder(FinalViewModel order)
        {
            ViewBag.StateId = new SelectList(db.States.OrderByDescending(current => current.Name), "Id", "Name");
            if (!User.Identity.IsAuthenticated)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (order.PaymentType == 0 || order.Type == 0)
            {
                TempData["error"] = "لطفا روش ارسال و پرداخت را تعیین نمایید .";
                return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
            }

            if (Convert.ToInt32(CartPrice()) == 0)
            {
                TempData["Error"] = "سبد خرید شما خالی است و امکان ثبت سفارش وجود ندارد .";
                return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
            }

            string userId = User.Identity.GetUserId();
            ApplicationUser user = db.Users.Where(current => current.Id == userId).Include(current => current.Discounts).Include(current => current.Notifications).FirstOrDefault();
            if (user == null)
            {
                TempData["Error"] = "شناسه کاربری شما یافت نشد .";
                return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
            }

            #region Validate_Discount
            if (!string.IsNullOrEmpty(order.Discount))
            {
                Discount dis = db.Discounts.Where(currrent => currrent.Code == order.Discount).FirstOrDefault();
                if (dis == null)
                {
                    TempData["Error"] = "کد تخفیف وارد شده معتبر نمیباشد .";
                    return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
                }
                if (dis.IsActived == false)
                {
                    TempData["Error"] = "کد تخفیف وارد شده غیر فعال شده است .";
                    return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
                }
                if (dis.IsPercentage == true)
                {
                    if (dis.Percent == 0 || dis.Percent == null)
                    {
                        TempData["Error"] = "کد تخفیف وارد شده مخدوش شده است .";
                        return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(dis.Amount))
                    {
                        TempData["Error"] = "کد تخفیف وارد شده مخدوش شده است .";
                        return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
                    }
                }
                //اگر کد تخفیف سقف داشت .
                if (!string.IsNullOrEmpty(dis.MaxOrder) && Convert.ToInt32(dis.MaxOrder) > 0)
                {
                    if (Convert.ToInt32(Convert.ToInt32(CartPrice())) > Convert.ToInt32(dis.MaxOrder))
                    {
                        TempData["Error"] = "مبلغ سفارش شما از کد تخفیف مورد نظر بیشتر است .";
                        return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
                    }
                }
                if (dis.Count > 0 && dis.Count == dis.MaxCount)
                {
                    if (dis.IsActived == true)
                    {
                        dis.IsActived = false;
                        db.Entry(dis).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Error"] = "ظرفیت استفاده از این کد تخفیف به پایان رسیده است .";
                        return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
                    }
                }
                if (dis.ExpireDate != null && dis.ExpireDate <= DateTime.Now)
                {
                    if (dis.IsActived == true)
                    {
                        dis.IsActived = false;
                        db.Entry(dis).State = EntityState.Modified;
                        db.SaveChanges();
                        TempData["Error"] = "زمان استفاده از کد تخفیف مورد نظر شما به اتمام رسیده است .";
                        return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
                    }
                }
                if (dis.ShowcaseDate != null && dis.ShowcaseDate <= DateTime.Now)
                {
                    if (dis.IsPublic == true)
                    {
                        dis.IsPublic = false;
                        db.Entry(dis).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            #endregion Validate_Discount

            List<DiscountViewModel> listDiscount = new List<DiscountViewModel>();
            List<HttpCookie> lst = new List<HttpCookie>();
            for (int i = Request.Cookies.Count - 1; i >= 0; i--)
            {
                if (lst.Where(p => p.Name == Request.Cookies[i].Name).Any() == false)
                    lst.Add(Request.Cookies[i]);
            }
            int TotalPrice = 0;
            if (listDiscount.Count() > 0)
            {
                TotalPrice = Convert.ToInt32(CartPrice()) - Convert.ToInt32(CartPrice()) * Convert.ToInt32(listDiscount.FirstOrDefault().Percent) / 100;
            }
            else
            {
                TotalPrice = Convert.ToInt32(CartPrice());
            }

            Order newOrder = new Order();
            newOrder.Send = 1;
            //آنلاین
            //اتصال به درگاه
            if (TotalPrice < 1000)
            {
                TempData["Error"] = "حداقل تراکنش 1000 ریال است .";
                return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
            }
            Random rnd = new Random();
            newOrder.StateId = user.StateId;
            newOrder.CityId = user.CityId;
            newOrder.FactorNumber = rnd.Next(11111, 99999).ToString();
            newOrder.FullName = user.FirstName + " " + user.LastName;
            newOrder.InvoiceNumber = rnd.Next(11111, 99999).ToString();
            newOrder.PaymentType = order.PaymentType;
            newOrder.Phone = user.Mobile;
            newOrder.UserOrderDescription = order.UserOrderDescription;
            if (!string.IsNullOrEmpty(user.AddressLine))
            {
                newOrder.UserAddress = user.AddressLine;
            }
            else
            {
                newOrder.UserAddress = order.UserAddress;
            }
            newOrder.TotalPrice = TotalPrice.ToString();
            newOrder.Receiver = 1;
            newOrder.Type = order.Type;
            newOrder.UserId = userId;
            newOrder.OrderDate = DateTime.Now;
            try
            {
                db.Orders.Add(newOrder);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "خطایی رخ داده است لطفا مجدد تلاش فرمایید . 0";
                return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
            }
            List<BasketViewModel> listBasket = new List<BasketViewModel>();
            foreach (var item in lst.Where(p => p.Name.StartsWith("MotoretoCt_")))
            {
                listBasket.Add(new BasketViewModel
                {
                    Product = db.Products.Find(Convert.ToInt32(item.Name.Substring(11))),
                    Count = Convert.ToInt32(item.Value)
                });
            }
            foreach (var item in listBasket)
            {
                if (item.Product != null)
                {
                    ProductInOrder productInOrder = new ProductInOrder();
                    productInOrder.ProductId = item.Product.Id;
                    productInOrder.OrderId = newOrder.Id;
                    productInOrder.Count = item.Count;
                    try
                    {
                        Product product = db.Products.Find(item.Product.Id);
                        if (product.Stock > item.Count)
                        {
                            //product.Stock -= item.Count;
                            //db.Entry(product).State = EntityState.Modified;
                        }
                        else
                        {
                            db.Orders.Remove(newOrder);
                            db.SaveChanges();
                            TempData["Error"] = "تعداد دلخواه شما از محصول " + product.PersianName + " بیش از موجودی سامانه است .";
                            return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
                        }
                        db.ProductInOrders.Add(productInOrder);
                        db.Entry(newOrder).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        db.Orders.Remove(newOrder);
                        db.SaveChanges();
                        TempData["Error"] = "خطایی رخ داده است لطفا مجدد تلاش فرمایید . 3" + ex;
                        return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
                    }
                }
            }

            #region HasDiscount
            if (!string.IsNullOrEmpty(order.Discount))
            {
                Discount dis = db.Discounts.Where(currrent => currrent.Code == order.Discount).FirstOrDefault();
                dis.Count++;
                db.Entry(dis).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    if (dis.IsPercentage == true)
                    {
                        if (dis.IsPercentage)
                        {
                            var discountPrice = int.Parse(newOrder.TotalPrice) - int.Parse(newOrder.TotalPrice) * (dis.Percent) / 100;
                            newOrder.TotalPrice = Convert.ToInt32(discountPrice).ToString();
                        }
                        else
                        {
                            var discountPrice = int.Parse(newOrder.TotalPrice) - int.Parse(dis.Amount);
                            newOrder.TotalPrice = Convert.ToInt32(discountPrice).ToString();
                        }
                    }
                    else
                    {
                        int cost = Convert.ToInt32(newOrder.TotalPrice) - Convert.ToInt32(dis.Amount);
                        if (cost > 0)
                            newOrder.TotalPrice = cost.ToString();
                        else
                            newOrder.TotalPrice = "0";
                    }
                    db.Entry(newOrder).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "خطایی رخ داده است لطفا مجدد تلاش فرمایید . 11";
                    return RedirectToAction("InsertOrder", "Home", new { PaymentType = order.PaymentType, Type = order.Type });
                }
            }
            //هزینه ارسال
            Application app = db.Applications.First();
            if (app != null)
            {
                if (app.Transferring > 0)
                {
                    newOrder.TotalPrice = (Convert.ToInt32(newOrder.TotalPrice) + Convert.ToInt32(app.Transferring)).ToString();
                    db.Entry(newOrder).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            #endregion HasDiscount

            #region Conditional_Discount
            Discount conditionalDiscount = db.Discounts.Where(current => current.DiscountCount != null && current.DiscountCount > 0 && current.IsActived == true).FirstOrDefault();
            if (conditionalDiscount != null)
            {
                //مکرر هست
                if (conditionalDiscount.IsRepeated == true)
                {
                    var catOrder = db.Orders.Where(current => current.UserId == newOrder.UserId && current.Done == true);
                    int remain = catOrder.Count() % (int)conditionalDiscount.DiscountCount;
                    if (remain == 0)
                    {
                        Notification notification = new Notification();
                        notification.Text = "کد تخفیف جدید برای شما : " + conditionalDiscount.Code;
                        notification.ForwardDate = DateTime.Now;
                        db.Notifications.Add(notification);
                        db.SaveChanges();
                        //user.Notifications.Add(notification);
                        //db.SaveChanges();
                    }
                }
                //مکرر نیست
                else
                {
                    var catOrder = db.Orders.Where(current => current.UserId == newOrder.UserId && current.Done == true);
                    if (catOrder.Count() == 1 && conditionalDiscount.DiscountCount == 1)
                    {
                        if (conditionalDiscount.IsPercentage == true)
                        {
                            var discountPrice = int.Parse(newOrder.TotalPrice) - int.Parse(newOrder.TotalPrice) * (conditionalDiscount.Percent) / 100;
                            newOrder.TotalPrice = Convert.ToInt32(discountPrice).ToString();
                        }
                        else
                        {
                            int cost = Convert.ToInt32(newOrder.TotalPrice) - Convert.ToInt32(conditionalDiscount.Amount);
                            if (cost > 0)
                                newOrder.TotalPrice = cost.ToString();
                            else
                                newOrder.TotalPrice = "0";
                        }
                        db.Entry(newOrder).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        if (catOrder.Count() == conditionalDiscount.DiscountCount)
                        {
                            Notification notification = new Notification();
                            notification.Text = "کد تخفیف جدید برای شما : " + conditionalDiscount.Code;
                            notification.ForwardDate = DateTime.Now;
                            db.Notifications.Add(notification);
                            db.SaveChanges();
                            //user.Notifications.Add(notification);
                            //db.SaveChanges();
                        }
                    }
                }
            }
            #endregion Conditional_Discount

            TempData["Success"] = "سفارش شما با موفقیت ثبت شد .";
            TempData["OrderId"] = newOrder.Id;

            ClearCartCookies();

            return RedirectToAction("Result", "Home", new { PaymentType = order.PaymentType, Type = order.Type });

            #region Tejarat
            long token = 0;
            int price = 0;
            short paymentStatus = short.MinValue;
            ClientSaleRequestData sendData = null;
            var callbackurl = Url.Action("VerifyOrder", "Home", new { order = newOrder.Id }, protocol: Request.Url.Scheme);
            Random random = new Random();
            var randomNumber = random.Next(10000, 99999);
            if (order.PaymentType == 1)
            {
                price = Convert.ToInt32(app.Transferring) - (Convert.ToInt32(user.Credit));
            }
            else if (order.PaymentType == 2)
            {
                price = (Convert.ToInt32(newOrder.TotalPrice) - (Convert.ToInt32(user.Credit)));
            }
            sendData = new ClientSaleRequestData()
            {
                Amount = Convert.ToInt64(price),
                AdditionalData = "پرداخت هزینه سفارش محصولات موتورتو",
                CallBackUrl = callbackurl,
                LoginAccount = "FuCLnCvM30xSMVO8AdRA",
                OrderId = randomNumber,
                Originator = newOrder.UserId
            };

            SaleServiceSoapClient payment = new SaleServiceSoapClient();
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((o, xc, xch, sslP) => true);
            ClientSaleResponseData responseData = null;

            responseData = payment.SalePaymentRequest(sendData);

            paymentStatus = responseData.Status;

            //check Status property of the response object to see if the operation was successful.
            if (responseData.Status == 0)
            {
                //if everything is OK (LoginAccount and your IP address is valid in Parsian PGW), save the token in a data store
                // to use it for redirectgion of your web site's user to the Parsian IPG (Internet Payment Gateway) page to complete payment.
                token = responseData.Token;
                return Redirect("https://pec.shaparak.ir/NewIPG/?Token=" + token);
            }
            if (responseData.Status == -112)
            {
                TempData["Error"] = "سفارش شما تکراری است لطفا درخواست فاکتور جدید از شرکت نمایید .";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "امکان اتصال به درگاه بانک وجود ندارد ." + responseData.Status;
                return RedirectToAction("Index");
            }
            #endregion Tejarat

        }

        public ActionResult VerifyOrder(PaymentCallbackModel model)
        {
            TempData["OrderId"] = model.order;
            if (model != null)
            {
                //save callback info to your database and relate those info to the user's requested service or product.
                //if needed, deliver product or service to the user after ensuring payment.
                //in the case of Bill Payment, no delivery is commonly required.
                //but if a Sale payment is performed, may be a product or service delivery will be needed.

                //if transaction by user is successful, call Confirm service from Parsian PGW to complete the transaction
                //and indicate that you will deliver service or product to your end user.
                //it is better to save callback results into a database.

                var callBackViewModel = new PaymentCallbackModel()
                {
                    Amount = model.Amount,
                    HashCardNumber = model.HashCardNumber,
                    OrderId = model.OrderId,
                    RRN = model.RRN,
                    status = model.status,
                    TerminalNo = model.TerminalNo,
                    Token = model.Token,
                    TspToken = model.TspToken,
                    UserId = model.UserId,
                    order = model.order
                };

                if (callBackViewModel.status == 0 && callBackViewModel.Token != null)
                {
                    //ایجاد یک نمونه از سرویس تایید پرداخت
                    TejaratConfirmService.ClientConfirmRequestData confirmSvc = new TejaratConfirmService.ClientConfirmRequestData();

                    //ایجاد یک نمونه از نوع پارامتر سرویس تایید پرداخت
                    var confirmRequestData = new TejaratConfirmService.ClientConfirmRequestData();

                    //توکن باید ارائه شود
                    confirmRequestData.Token = callBackViewModel.Token ?? -1;
                    confirmRequestData.LoginAccount = "FuCLnCvM30xSMVO8AdRA";
                    //فراخوانی سرویس و دریافت نتیجه فراخوانی
                    TejaratConfirmService.ConfirmServiceSoapClient confirm = new TejaratConfirmService.ConfirmServiceSoapClient();

                    var confirmResponse = confirm.ConfirmPayment(confirmRequestData);
                    //کنترل کد وضعیت نتیجه فراخوانی
                    //درصورتی که موفق باشد، باید خدمات یا کالا به کاربر پرداخت کننده ارائه شود
                    if (confirmResponse.Status == 0)
                    {
                        //deliver service or product to end user accourding to your business rules.
                        //it is better to save confirm response into a database.

                        #region EnableOrder
                        //پس از اتصال به درگاه بانکی و در صورت موفقیت تراکنش
                        Order order = db.Orders.Where(current => current.Id == callBackViewModel.order).Include(current => current.Products).FirstOrDefault();
                        ApplicationUser user = db.Users.Find(order.UserId);
                        Application app = db.Applications.First();
                        int price = 0;
                        //موفقیت پرداخت
                        Random rnd = new Random();
                        Transaction trans = new Transaction()
                        {
                            Number = rnd.Next(11111, 99999).ToString(),
                            Date = DateTime.Now,
                            InvoiceNumber = callBackViewModel.RRN.ToString(),
                            BankName = "بانک تجارت",
                            RecievedDocumentNumber = callBackViewModel.RRN.ToString(),
                            RecievedDocumentDate = DateTime.Now,
                            CardNumber = "****",
                            TerminalNumber = callBackViewModel.TerminalNo.ToString(),
                            Acceptor = "Motoreto",
                            OperationResult = 1,
                            AcceptorPostalCode = "*********",
                            AcceptorPhoneNumber = "09128049946",
                            OrderId = order.Id
                        };
                        db.Transactions.Add(trans);
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            TempData["Error"] = " خطایی رخ داده است لطفا با پشتیبانی تماس بگیرید ، کد پیگیری : " + callBackViewModel.TerminalNo.ToString();
                            return View("Result");
                        }
                        #endregion EnableOrder
                        Payment payment = new Payment();
                        payment.UserId = order.UserId;
                        if (order.PaymentType == 1)
                        {
                            price = Convert.ToInt32(app.Transferring) - (Convert.ToInt32(user.Credit));
                        }
                        else if (order.PaymentType == 2)
                        {
                            price = (Convert.ToInt32(order.TotalPrice) - (Convert.ToInt32(user.Credit)));
                        }
                        payment.Amount = price;
                        payment.Status = 3;
                        payment.ActionType = 1;
                        payment.OrderId = order.Id;
                        payment.Description = "هزینه سفارش شماره : " + order.Id;
                        payment.TrackingCode = callBackViewModel.TerminalNo.ToString() ?? rnd.Next(1, 6).ToString();
                        payment.CreateDate = DateTime.Now;
                        db.Payments.Add(payment);
                        try
                        {
                            db.SaveChanges();
                            db.Entry(order).State = EntityState.Modified;
                            db.SaveChanges();
                            user.Credit = 0;
                            db.Entry(user).State = EntityState.Modified;
                            db.SaveChanges();
                            //مالی کاربر
                            if (user.Credit > 0)
                            {
                                Payment payment2 = new Payment();
                                payment2.UserId = order.UserId;
                                payment2.Amount = Convert.ToInt64(order.TotalPrice) - user.Credit;
                                payment2.Status = 3;
                                payment2.ActionType = 1;
                                payment2.OrderId = order.Id;
                                Random rnd2 = new Random();
                                payment2.Description = "کسر از کیف پول برای سفارش : " + order.Id;
                                payment2.TrackingCode = callBackViewModel.TerminalNo.ToString() ?? rnd2.Next(1, 6).ToString();
                                payment2.CreateDate = DateTime.Now;
                                db.Payments.Add(payment2);
                                db.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            TempData["Error"] = "خطایی رخ داده است .";
                            return View("Result");
                        }
                        #region Cookie
                        string[] allDomainCookes = Request.Cookies.AllKeys;
                        foreach (string domainCookie in allDomainCookes)
                        {
                            if (domainCookie.Contains("MotoretoCt_"))
                                Response.Cookies[domainCookie].Expires = DateTime.Now.AddDays(-1);
                        }
                        #endregion Cookie
                        foreach (var item in order.Products.ToList())
                        {
                            Product product = db.Products.Find(item.ProductId);
                            if (product.Stock >= item.Count)
                            {
                                product.Stock -= item.Count;
                                db.Entry(product).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        ViewBag.Message = "پرداخت شما با موفقیت به شماره مرجع " + callBackViewModel.RRN.ToString() + " انجام شد .";
                        return RedirectToAction("Result", "Home");
                    }
                    else
                    {
                        ViewBag.Message = "خطایی رخ داده است لطفا مجدد تلاش فرمایید ." + confirmResponse.Status;
                        return View("Result");
                    }
                }
                else if (callBackViewModel.status == -138)
                {
                    ViewBag.Message = "کاربر از انجام تراکنش منصرف شده است .";
                    return View("Result");
                }
                else
                {
                    ViewBag.Message = "خطایی رخ داده است لطفا مجدد تلاش فرمایید . " + callBackViewModel.status + " | " + callBackViewModel.Token;
                    return View("Result");
                }
            }
            else
            {
                ViewBag.Message = "خطایی رخ داده است لطفا مجدد تلاش فرمایید . 5";
                return View("Result");
            }
        }

        public ActionResult Result()
        {
            return View();
        }
    }
}

