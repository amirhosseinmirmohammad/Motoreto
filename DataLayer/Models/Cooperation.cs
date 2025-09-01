using GladcherryShopping.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace DataLayer.Models
{
    public class Cooperation
    {
        public Cooperation()
        {
        }

        internal class configuration : EntityTypeConfiguration<Cooperation>
        {
            public configuration()
            {
            }
        }
        public int Id { get; set; }

        [Display(Name = "نام و نام خانوادگی")]
        [DisplayName("نام و نام خانوادگی")]
        public string FullName { get; set; }

        [Display(Name = "کد ملی")]
        [DisplayName("کد ملی")]
        public string NationalCode { get; set; }

        [Display(Name = "تاریخ تولد")]
        [DisplayName("تاریخ تولد")]
        public string Bithdate { get; set; }

        [Display(Name = "میزان تحصیلات")]
        [DisplayName("میزان تحصیلات")]
        public string Education { get; set; }

        [Display(Name = "رشته تحصیلی")]
        [DisplayName("رشته تحصیلی")]
        public string Field { get; set; }

        [Display(Name = "سوابق شغلی گذشته")]
        [DisplayName("سوابق شغلی گذشته")]
        public string Body { get; set; }

        [Display(Name = "نام شرکت")]
        [DisplayName("نام شرکت")]
        public string Company { get; set; }

        [Display(Name = "مدت فعالیت")]
        [DisplayName("مدت فعالیت")]
        public string Duration { get; set; }

        [Display(Name = "تلفن شرکت")]
        [DisplayName("تلفن شرکت")]
        public string PhoneCo { get; set; }

        [Display(Name = "فکس شرکت")]
        [DisplayName("فکس شرکت")]
        public string Fax { get; set; }

        [Display(Name = "پست الکترونیکی شرکت")]
        [DisplayName("پست الکترونیکی شرکت")]
        public string EmailCo { get; set; }

        [Display(Name = "آدرس وب سایت شرکت")]
        [DisplayName("آدرس وب سایت شرکت")]
        public string WebsiteCo { get; set; }

        [Display(Name = "آدرس صفحه اینستاگرام")]
        [DisplayName("آدرس صفحه اینستاگرام")]
        public string Instagram { get; set; }

        [Display(Name = "آدرس کانال تلگرام")]
        [DisplayName("آدرس کانال تلگرام")]
        public string Telegram { get; set; }

        [DisplayName("وضعیت محل فعالیت")]
        [Display(Name = "وضعیت محل فعالیت")]
        public byte? Situation { get; set; }

        [NotMapped]
        public string getSituation
        {
            get
            {
                switch (Situation)
                {
                    case 1:
                        return "تجاری";
                    case 2:
                        return "اداری";
                    case 3:
                        return "مسکونی";
                    default:
                        return "تعیین نشده";
                }
            }
        }

        [DisplayName("کد پستی شرکت")]
        [Display(Name = "کد پستی شرکت")]
        public string PostalCode { get; set; }

        [DisplayName("تضمین شما در صورت اخد نمایندگی ؟")]
        [Display(Name = "تضمین شما در صورت اخد نمایندگی ؟")]
        public byte? Guaranteed { get; set; }

        [NotMapped]
        public string getGuaranteed
        {
            get
            {
                switch (Situation)
                {
                    case 1:
                        return "نقدی";
                    case 2:
                        return "چک";
                    case 3:
                        return "سفته";
                    default:
                        return "تعیین نشده";
                }
            }
        }

        [DisplayName("آدرس کامل پستی")]
        [Display(Name = "آدرس کامل پستی")]
        public string PostalAddress { get; set; }

        [DisplayName("پیش بینی شما از میزان فروشتان در ماه ؟")]
        [Display(Name = "پیش بینی شما از میزان فروشتان در ماه ؟")]
        public string Prediction { get; set; }

        [DisplayName("چنانچه از سایر شرکت‌ها نمایندگی فروش دارید، لطفاً نام و مدت زمان فعالیت را در قسمت ذیل توضیح دهید")]
        [Display(Name = "چنانچه از سایر شرکت‌ها نمایندگی فروش دارید، لطفاً نام و مدت زمان فعالیت را در قسمت ذیل توضیح دهید")]
        public string Agency { get; set; }

        [DisplayName("آتلفن")]
        [Display(Name = "تلفن")]
        public string Phone { get; set; }

        [DisplayName("موبایل")]
        [Display(Name = "موبایل")]
        public string Mobile { get; set; }

        [DisplayName("پست الکترونیکی")]
        [Display(Name = "پست الکترونیکی")]
        public string Email { get; set; }

        [DisplayName("آدرس وب سایت")]
        [Display(Name = "آدرس وب سایت")]
        public string Website { get; set; }

        [DisplayName("نشانی")]
        [Display(Name = "نشانی")]
        public string Address { get; set; }
    }
}
