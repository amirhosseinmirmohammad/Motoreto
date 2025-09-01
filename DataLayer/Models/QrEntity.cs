using DataLayer.Models;
using GladcherryShopping.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace DataLayer.Models
{
    public class QrEntity
    {
        public QrEntity()
        {
        }

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string QrCode { get; set; }
        public string SerialNumber { get; set; }
        public int? ProductId { get; set; }    
    }
}