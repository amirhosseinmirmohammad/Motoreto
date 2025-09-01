using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace DataLayer.Models
{
    public class PreProduct
    {
        public PreProduct()
        {
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ScaffoldColumn(false)]
        [Bindable(false)]
        public int Id { get; set; }

        [AllowHtml]
        [DisplayName("متن توضیحات")]
        [Display(Name = "متن توضیحات")]
        [Required(ErrorMessage = "لطفا متن توضیحات را تعیین نمایید .")]
        [DataType(DataType.MultilineText)]
        public string Body { get; set; }
    }
}
