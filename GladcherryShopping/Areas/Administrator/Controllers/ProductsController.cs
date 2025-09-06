using DataLayer.Models;
using DataLayer.ViewModels.PagerViewModel;
using GladcherryShopping.Models;
using GladCherryShopping.Helpers;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using static GladCherryShopping.Helpers.FunctionsHelper;

namespace GladcherryShopping.Areas.Administrator.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class ProductsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Administrator/Products
        public ActionResult Index(string search = "", int page = 1)
        {
            var products = db.Products.Include(c => c.category).AsQueryable();

            // 🔍 اعمال سرچ
            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p =>
                    p.PersianName.Contains(search) ||
                    p.EnglishName.Contains(search) ||
                    p.Code.ToString().Contains(search) || // سرچ روی کد قطعه
                    p.category.PersianName.Contains(search) // سرچ روی دسته
                );
            }

            // مرتب‌سازی بر اساس نام فارسی
            products = products.OrderBy(p => p.PersianName);

            // صفحه‌بندی
            PagerViewModels<Product> ProductViewModels = new PagerViewModels<Product>
            {
                CurrentPage = page,
                data = products.Skip((page - 1) * 10).Take(10).ToList(),
                TotalItemCount = products.Count()
            };

            ViewBag.Search = search; // برای نگه داشتن سرچ در View
            return View(ProductViewModels);
        }

        // GET: Administrator/Products/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Administrator/Products/Create
        public ActionResult Create()
        {
            var products = db.Products;
            ViewBag.Related = new MultiSelectList(products, "Id", "FullName");
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "PersianName");
            return View();
        }

        // POST: Administrator/Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,PersianName,EnglishName,UnitPrice,Stock,Code,SefUrl,DiscountPercent,Description,CategoryId,SliderImage,AppSmallImage,AppLargeImage,SiteFirstImage,SiteSecondImage,SiteThirdImage,CreateDate,IsSpecial,IsWonderful,Day,Hour,Minute,WoDate,Min")] Product product,
            HttpPostedFileBase SliderImage, HttpPostedFileBase AppSmallImage, HttpPostedFileBase AppLargeImage, HttpPostedFileBase SiteFirstImage, HttpPostedFileBase SiteSecondImage, HttpPostedFileBase SiteThirdImage, IEnumerable<int> Related, IEnumerable<HttpPostedFileBase> Images)
        {
            Product wonderPro = db.Products.Where(current => current.IsWonderful == true).FirstOrDefault();
            if (product.IsWonderful == true)
            {
                if(wonderPro != null)
                {
                    TempData["Error"] = "شما قبلا یک محصول را به عنوان پیشنهاد شگفت انگیز انتخاب کرده اید . شماره : " + wonderPro.Id;
                    return RedirectToAction("Index");
                }
                if(product.Day == null || product.Minute == null || product.Hour == null)
                {
                    TempData["Error"] = "جهت انتخاب محصول خود به عنوان پیشنهاد شگفت انگیز لطفا روز ساعت و دقیقه را وارد کرده یا 0 قرار دهید .";
                    return RedirectToAction("Index");
                }
            }
            var products = db.Products;
            ViewBag.Related = new MultiSelectList(products, "Id", "FullName");
            if (ModelState.IsValid)
            {
                {
                    if (product.IsWonderful == true && product.Day != null && product.Hour != null && product.Minute != null && product.WoDate != null)
                        product.WonderDate = ConvertToGregorian(product.WoDate).AddDays(Convert.ToDouble(product.Day)).AddHours(Convert.ToDouble(product.Hour)).AddMinutes(Convert.ToDouble(product.Minute));
                    #region FileUploading
                    if (SliderImage != null && SliderImage.ContentLength > 0)
                    {
                        string smallImagePath = FunctionsHelper.File(FileMode.Upload, FileType.Image, "~/images/Products/", true, SliderImage, Server);
                        product.SliderImage = smallImagePath;
                    }
                    if (AppSmallImage != null && AppSmallImage.ContentLength > 0)
                    {
                        string largeImagePath = FunctionsHelper.File(FileMode.Upload, FileType.Image, "~/images/Products/", true, AppSmallImage, Server);
                        product.AppSmallImage = largeImagePath;
                    }
                    if (AppLargeImage != null && AppLargeImage.ContentLength > 0)
                    {
                        string backgroundImagePath = FunctionsHelper.File(FileMode.Upload, FileType.Image, "~/images/Products/", true, AppLargeImage, Server);
                        product.AppLargeImage = backgroundImagePath;
                    }
                    if (SiteFirstImage != null && SiteFirstImage.ContentLength > 0)
                    {
                        string smallImagePath = FunctionsHelper.File(FileMode.Upload, FileType.Image, "~/images/Products/", true, SiteFirstImage, Server);
                        product.SiteFirstImage = smallImagePath;
                    }
                    if (SiteSecondImage != null && SiteSecondImage.ContentLength > 0)
                    {
                        string largeImagePath = FunctionsHelper.File(FileMode.Upload, FileType.Image, "~/images/Products/", true, SiteSecondImage, Server);
                        product.SiteSecondImage = largeImagePath;
                    }
                    if (SiteThirdImage != null && SiteThirdImage.ContentLength > 0)
                    {
                        string backgroundImagePath = FunctionsHelper.File(FileMode.Upload, FileType.Image, "~/images/Products/", true, SiteThirdImage, Server);
                        product.SiteThirdImage = backgroundImagePath;
                    }
                    #endregion FileUploading
                    if(!string.IsNullOrEmpty(product.WoDate))
                    {
                        product.WonderDate = ConvertToGregorian(product.WoDate);
                    }
                    product.CreateDate = DateTime.Now;

                    #region Meta Key
                    // ۱. اگر کاربر چیزی وارد نکرد، از PersianName استفاده کن
                    string slug = string.IsNullOrWhiteSpace(product.SefUrl)
                        ? product.PersianName.Trim()
                        : product.SefUrl.Trim();

                    // ۲. به حروف کوچک (برای یکدست شدن)
                    slug = slug.ToLowerInvariant();

                    // ۳. جایگزین کردن فاصله و چندین فاصله با -
                    slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");

                    // ۴. حذف کاراکترهای غیرمجاز (فقط حروف، اعداد و - بمانند)
                    slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\u0600-\u06FF\-]", "");

                    // ۵. حذف - های تکراری
                    slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

                    // ۶. حذف - از اول و آخر
                    slug = slug.Trim('-');

                    // ۷. بررسی تکراری نبودن SefUrl
                    bool exists = db.Products.Any(p => p.SefUrl == slug);
                    if (exists)
                    {
                        TempData["Error"] = "این آدرس (SefUrl) قبلاً استفاده شده است. لطفاً یک آدرس دیگر وارد کنید.";
                        ViewBag.CategoryId = new SelectList(db.Categories, "Id", "PersianName", product.CategoryId);
                        return View(product);
                    }

                    product.SefUrl = slug;
                    #endregion Meta Key

                    db.Products.Add(product);
                    try
                    {
                        TempData["Success"] = "محصول مورد نظر با موفقیت ثبت شد .";
                        db.SaveChanges();
                        #region Image
                        foreach (var image in Images)
                        {
                            if (image != null && image.ContentLength > 0)
                            {
                                string Path = FunctionsHelper.File(FileMode.Upload, FileType.Image, "~/images/Products/", true, image, Server);
                                if (!string.IsNullOrEmpty(Path))
                                {
                                    Image Newimage = new Image()
                                    {
                                        Alt = product.PersianName,
                                        ProductId = product.Id,
                                        Link = Path,
                                        Source = "~/images/Products/",
                                        Title = product.PersianName
                                    };
                                    db.Images.Add(Newimage);
                                    db.SaveChanges();
                                }
                            }
                        }
                        #endregion Image

                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = "خطایی رخ داده است .";
                    }
                    return RedirectToAction("Index");
                }
            }
            else
            {
                ViewBag.CategoryId = new SelectList(db.Categories, "Id", "PersianName", product.CategoryId);
                return View(product);
            }
        }

        // GET: Administrator/Products/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Where(current => current.Id == id).Include(current => current.RelatedProducts).Include(current => current.Images).FirstOrDefault();
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "PersianName", product.CategoryId);
            var products = db.Products;
            ViewBag.Related = new MultiSelectList(products, "Id", "FullName", product.RelatedProducts.Select(current => current.Id));
            return View(product);
        }

        // POST: Administrator/Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,PersianName,EnglishName,UnitPrice,Stock,Code,SefUrl,DiscountPercent,Description,CategoryId,SliderImage,AppSmallImage,AppLargeImage,SiteFirstImage,SiteSecondImage,SiteThirdImage,CreateDate,IsSpecial,IsWonderful,Day,Hour,Minute,WoDate,Min")] Product product,
            HttpPostedFileBase SliderImage, HttpPostedFileBase AppSmallImage, HttpPostedFileBase AppLargeImage, HttpPostedFileBase SiteFirstImage, HttpPostedFileBase SiteSecondImage, HttpPostedFileBase SiteThirdImage, IEnumerable<int> Related, IEnumerable<HttpPostedFileBase> Images)
        {
            Product wonderPro = db.Products.Where(current => current.IsWonderful == true && current.Id != product.Id).FirstOrDefault();
            if (product.IsWonderful == true)
            {
                if (wonderPro != null)
                {
                    TempData["Error"] = "شما قبلا یک محصول را به عنوان پیشنهاد شگفت انگیز انتخاب کرده اید . شماره : " + wonderPro.Id;
                    return RedirectToAction("Index");
                }
                if (product.Day == null || product.Minute == null || product.Hour == null)
                {
                    TempData["Error"] = "جهت انتخاب محصول خود به عنوان پیشنهاد شگفت انگیز لطفا روز ساعت و دقیقه را وارد کرده یا 0 قرار دهید .";
                    return RedirectToAction("Index");
                }
            }
            var products = db.Products;
            ViewBag.Related = new MultiSelectList(products, "Id", "FullName", product.RelatedProducts.Select(current => current.Id));
            if (ModelState.IsValid)
            {

                Product editedProduct = db.Products.Where(current => current.Id == product.Id).Include(current => current.RelatedProducts).FirstOrDefault();
                editedProduct.CategoryId = product.CategoryId;
                editedProduct.Description = product.Description;
                editedProduct.DiscountPercent = product.DiscountPercent;
                editedProduct.EnglishName = product.EnglishName;
                editedProduct.IsSpecial = product.IsSpecial;
                editedProduct.PersianName = product.PersianName;
                editedProduct.Stock = product.Stock;
                editedProduct.UnitPrice = product.UnitPrice;
                editedProduct.IsWonderful = product.IsWonderful;
                editedProduct.Day = product.Day;
                editedProduct.Hour = product.Hour;
                editedProduct.Minute = product.Minute;
                editedProduct.Min = product.Min;

                if (!string.IsNullOrEmpty(product.WoDate))
                {
                    editedProduct.WoDate = product.WoDate;
                    editedProduct.WonderDate = ConvertToGregorian(editedProduct.WoDate);
                }

                #region FileUploading 
                if (SliderImage != null && SliderImage.ContentLength > 0)
                {
                    if (editedProduct.SliderImage != null)
                    {
                        string smallImagePath = FunctionsHelper.File(FileMode.Update, FileType.Image, product.SliderImage, true, SliderImage, Server);
                        editedProduct.SliderImage = smallImagePath;
                    }
                    else
                    {
                        string SliderImagePath = FunctionsHelper.File(FileMode.Upload, FileType.Image, "~/images/Products/", true, SliderImage, Server);
                        editedProduct.SliderImage = SliderImagePath;
                    }
                }
                if (AppSmallImage != null && AppSmallImage.ContentLength > 0)
                {
                    if (editedProduct.AppSmallImage != null)
                    {
                        string AppSmallImagePath = FunctionsHelper.File(FileMode.Update, FileType.Image, product.AppSmallImage, true, AppSmallImage, Server);
                        editedProduct.AppSmallImage = AppSmallImagePath;
                    }
                    else
                    {
                        string largeImagePath = FunctionsHelper.File(FileMode.Upload, FileType.Image, "~/images/Products/", true, AppSmallImage, Server);
                        editedProduct.AppSmallImage = largeImagePath;
                    }
                }
                if (AppLargeImage != null && AppLargeImage.ContentLength > 0)
                {
                    if (editedProduct.AppLargeImage != null)
                    {
                        string AppLargeImagePath = FunctionsHelper.File(FileMode.Update, FileType.Image, product.AppLargeImage, true, AppLargeImage, Server);
                        editedProduct.AppLargeImage = AppLargeImagePath;
                    }
                    else
                    {
                        string backgroundImagePath = FunctionsHelper.File(FileMode.Upload, FileType.Image, "~/images/Products/", true, AppLargeImage, Server);
                        editedProduct.AppLargeImage = backgroundImagePath;
                    }
                }
                if (SiteFirstImage != null && SiteFirstImage.ContentLength > 0)
                {
                    if (editedProduct.SiteFirstImage != null)
                    {
                        string SiteFirstImagePath = FunctionsHelper.File(FileMode.Update, FileType.Image, product.SiteFirstImage, true, SiteFirstImage, Server);
                        editedProduct.SiteFirstImage = SiteFirstImagePath;
                    }
                    else
                    {
                        string SiteFirstImagePath = FunctionsHelper.File(FileMode.Upload, FileType.Image, "~/images/Products/", true, SiteFirstImage, Server);
                        editedProduct.SiteFirstImage = SiteFirstImagePath;
                    }
                }
                if (SiteSecondImage != null && SiteSecondImage.ContentLength > 0)
                {
                    if (editedProduct.SiteSecondImage != null)
                    {
                        string SiteSecondImagePath = FunctionsHelper.File(FileMode.Update, FileType.Image, product.SiteSecondImage, true, SiteSecondImage, Server);
                        editedProduct.SiteSecondImage = SiteSecondImagePath;
                    }
                    else
                    {
                        string SiteSecondImagePath = FunctionsHelper.File(FileMode.Upload, FileType.Image, "~/images/Products/", true, SiteSecondImage, Server);
                        editedProduct.SiteSecondImage = SiteSecondImagePath;
                    }
                }
                if (SiteThirdImage != null && SiteThirdImage.ContentLength > 0)
                {
                    if (editedProduct.SiteThirdImage != null)
                    {
                        string SiteThirdImagePath = FunctionsHelper.File(FileMode.Update, FileType.Image, product.SiteThirdImage, true, SiteThirdImage, Server);
                        editedProduct.SiteThirdImage = SiteThirdImagePath;
                    }
                    else
                    {
                        string SiteThirdImagePath = FunctionsHelper.File(FileMode.Upload, FileType.Image, "~/images/Products/", true, SiteThirdImage, Server);
                        editedProduct.SiteThirdImage = SiteThirdImagePath;
                    }
                }
                #endregion FileUploading

                #region Meta Key
                // ۱. اگر کاربر چیزی وارد نکرد، از PersianName استفاده کن
                string slug = string.IsNullOrWhiteSpace(product.SefUrl)
                    ? product.PersianName.Trim()
                    : product.SefUrl.Trim();

                // ۲. به حروف کوچک (برای یکدست شدن)
                slug = slug.ToLowerInvariant();

                // ۳. جایگزین کردن فاصله و چندین فاصله با -
                slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");

                // ۴. حذف کاراکترهای غیرمجاز (فقط حروف، اعداد و - بمانند)
                slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\u0600-\u06FF\-]", "");

                // ۵. حذف - های تکراری
                slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

                // ۶. حذف - از اول و آخر
                slug = slug.Trim('-');

                // ۷. بررسی تکراری نبودن SefUrl
                bool exists = db.Products.Any(p => p.SefUrl == slug);
                if (exists)
                {
                    TempData["Error"] = "این آدرس (SefUrl) قبلاً استفاده شده است. لطفاً یک آدرس دیگر وارد کنید.";
                    ViewBag.CategoryId = new SelectList(db.Categories, "Id", "PersianName", product.CategoryId);
                    return View(product);
                }

                editedProduct.SefUrl = slug;
                #endregion Meta Key

                #region Image
                foreach (var image in Images)
                {
                    if (image != null && image.ContentLength > 0)
                    {
                        string Path = FunctionsHelper.File(FileMode.Upload, FileType.Image, "~/images/Products/", true, image, Server);
                        if (!string.IsNullOrEmpty(Path))
                        {
                            Image Newimage = new Image()
                            {
                                Alt = product.PersianName,
                                ProductId = product.Id,
                                Link = Path,
                                Source = "~/images/Products/",
                                Title = product.PersianName
                            };
                            db.Images.Add(Newimage);
                            db.SaveChanges();
                        }
                    }
                }
                #endregion Image

                if (Related != null)
                {
                    editedProduct.RelatedProducts.Clear();
                    foreach (var item in Related)
                    {
                        Product prod = db.Products.Find(item);
                        if (prod != null)
                        {
                            editedProduct.RelatedProducts.Add(prod);
                        }
                    }
                }

                else
                {
                    editedProduct.RelatedProducts.Clear();
                }

                db.Entry(editedProduct).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "محصول مورد نظر با موفقیت ویرایش شد .";
                return RedirectToAction("Index");
            }
            ViewBag.CategoryId = new SelectList(db.Categories, "Id", "PersianName", product.CategoryId);
            return View(product);
        }

        // GET: Administrator/Products/Delete/5
        public ActionResult Delete(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Administrator/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(long id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            try
            {
                db.SaveChanges();
                TempData["Success"] = "محصول مورد نظر با موفقیت از سیستم حذف شد .";
            }
            catch (Exception)
            {
                TempData["Error"] = "به دلیل وجود وابستگی ها امکان حذف این محصول از سیستم وجود ندارد .";
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
