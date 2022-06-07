using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebMvc.Models
{
    public class Shop
    {

        [Key]
        public int ShopId { get; set; } 

        public string? ShopIdentity { set; get; }

        public string? Name { get; set; }

        public bool Open { get; set; } = true;

        //public virtual ICollection<Order> Orders { get; set; }
    }
}
