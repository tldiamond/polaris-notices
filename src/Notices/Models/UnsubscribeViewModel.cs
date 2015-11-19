using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NoticeSuite.Models
{
    public class UnsubscribeViewModel
    {
        public int? OrgID { get; set; }
        [Required]
        [MinLength(5)]
        public string Barcode { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(4)]
        public string Pin { get; set; }
    }
}