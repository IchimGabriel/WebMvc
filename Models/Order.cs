using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebMvc.Models
{
    public class Order
    {

        [Key]
        public int OrderId { get; set; } 

        public DateTime TimeStamp { get; set; } = DateTime.Now;


        [Display(Name = "Total")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public decimal Total { get; set; }


        [Display(Name = "Driver Commision")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:c}")]
        public decimal Commission { get; set; }


        [Display(Name = "Delivery Address")]
        public string? Address { get; set; }

        public bool IsDelivered { get; set; } = false;   

        public string? DriverIdentity { get; set; }

        public string? ShopIdentity { get; set; }


        //public virtual Driver Driver { get; set; }
        //public virtual Shop Shop { get; set; }
    }
}
