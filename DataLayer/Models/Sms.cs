using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DataLayer.Models
{
    public class Sms
    {
        [ScaffoldColumn(false)]
        [Bindable(false)]
        [Key]
        [Required]
        [DatabaseGenerated
        (DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [AllowHtml]
        [DisplayName("متن پیامک")]
        [Required(ErrorMessage = "لطفا متن پیامک را وارد کنید")]
        [DataType(dataType:DataType.MultilineText)]
        public string Body { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
