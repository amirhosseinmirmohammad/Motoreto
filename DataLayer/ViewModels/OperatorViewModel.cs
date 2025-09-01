using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.ViewModels
{
    public class OperatorViewModel
    {
        [DisplayName("نام")]
        [Display(Name = "نام")]
        [Required(ErrorMessage = "لطفا نام اپراتور را تعیین نمایید .")]
        public string FirstName { get; set; }

        [DisplayName("نام خانوادگی")]
        [Display(Name = "نام خانوادگی")]
        [Required(ErrorMessage = "لطفا نام خانوادگی اپراتور را تعیین نمایید .")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "لطفا ایمیل اپراتور را تعیین نمایید .")]
        [EmailAddress]
        [Display(Name = "ایمیل")]
        public string Email { get; set; }

        [DisplayName("موبایل")]
        [Display(Name = "موبایل")]
        [Required(ErrorMessage = "لطفا شماره موبایل اپراتور را تعیین نمایید .")]
        [RegularExpression(@"\b\d{4}[\s-.]?\d{3}[\s-.]?\d{4}\b", ErrorMessage = "فرمت تلفن همراه وارد شده صحیح نیست .")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "لطفا رمز عبور اپراتور را تعیین نمایید .")]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string Password { get; set; }

        [DisplayName("تصویر پروفایل")]
        [Display(Name = "تصویر پروفایل")]
        public string ProfileImage { get; set; }

        [DisplayName("آدرس اپراتور")]
        [Display(Name = "آدرس اپراتور")]
        public string AddressLine { get; set; }

        [DisplayName("اپراتور فعال است ؟")]
        [Display(Name = "اپراتور فعال است ؟")]
        public bool IsActive { get; set; }
    }

    public class ResetOperatorViewModel
    {
        public string UserId { get; set; }

        [DisplayName("نام")]
        [Display(Name = "نام")]
        [Required(ErrorMessage = "لطفا نام اپراتور را تعیین نمایید .")]
        public string FirstName { get; set; }

        [DisplayName("نام خانوادگی")]
        [Display(Name = "نام خانوادگی")]
        [Required(ErrorMessage = "لطفا نام خانوادگی اپراتور را تعیین نمایید .")]
        public string LastName { get; set; }

        //[Required(ErrorMessage = "لطفا ایمیل اپراتور را تعیین نمایید .")]
        //[EmailAddress]
        //[Display(Name = "ایمیل")]
        //public string Email { get; set; }

        [DisplayName("موبایل")]
        [Display(Name = "موبایل")]
        [Required(ErrorMessage = "لطفا شماره موبایل اپراتور را تعیین نمایید .")]
        [RegularExpression(@"\b\d{4}[\s-.]?\d{3}[\s-.]?\d{4}\b", ErrorMessage = "فرمت تلفن همراه وارد شده صحیح نیست .")]
        public string Mobile { get; set; }

        //[Required(ErrorMessage = "لطفا رمز عبور اپراتور را تعیین نمایید .")]
        //[DataType(DataType.Password)]
        //[Display(Name = "رمز عبور")]
        //public string Password { get; set; }

        [DisplayName("تصویر پروفایل")]
        [Display(Name = "تصویر پروفایل")]
        public string ProfileImage { get; set; }

        [DisplayName("آدرس اپراتور")]
        [Display(Name = "آدرس اپراتور")]
        public string AddressLine { get; set; }

        [DisplayName("اپراتور فعال است ؟")]
        [Display(Name = "اپراتور فعال است ؟")]
        public bool IsActive { get; set; }
    }

}
