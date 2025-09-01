using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLayer.ViewModels
{
    public class PaymentCallbackModel
    {
        public long? Token { get; set; }

        [Display(Name = "شناسه سفارش")]
        public long? OrderId { get; set; }
        public int? TerminalNo { get; set; }

        [Display(Name = "شماره مرجع تراکنش")]
        public long? RRN { get; set; }

        [Display(Name = "کد وضعیت تراکنش")]
        public short? status { get; set; }

        [Display(Name = "شماره کارت هش")]
        public string HashCardNumber { get; set; }

        [Display(Name = "مبلغ")]
        public string Amount { get; set; }

        [Display(Name = "TSP Token")]
        public string TspToken { get; set; }
        public string UserId { get; set; }
        public int? order { get; set; }
    }
    public class CreditCallbackModel
    {
        public long? Token { get; set; }

        [Display(Name = "شناسه سفارش")]
        public long? OrderId { get; set; }
        public int? TerminalNo { get; set; }

        [Display(Name = "شماره مرجع تراکنش")]
        public long? RRN { get; set; }

        [Display(Name = "کد وضعیت تراکنش")]
        public short? status { get; set; }

        [Display(Name = "شماره کارت هش")]
        public string HashCardNumber { get; set; }

        [Display(Name = "مبلغ")]
        public string Amount { get; set; }

        [Display(Name = "TSP Token")]
        public string TspToken { get; set; }
        public string UserId { get; set; }
    }
    public class ContractorCreditCallbackModel
    {
        public long? Token { get; set; }

        [Display(Name = "شناسه سفارش")]
        public long? OrderId { get; set; }
        public int? TerminalNo { get; set; }

        [Display(Name = "شماره مرجع تراکنش")]
        public long? RRN { get; set; }

        [Display(Name = "کد وضعیت تراکنش")]
        public short? status { get; set; }

        [Display(Name = "شماره کارت هش")]
        public string HashCardNumber { get; set; }

        [Display(Name = "مبلغ")]
        public string Amount { get; set; }

        [Display(Name = "TSP Token")]
        public string TspToken { get; set; }
        public int? ContractorId { get; set; }
        public long? Ammount { get; set; }
    }
}
