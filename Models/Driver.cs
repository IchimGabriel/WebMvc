using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebMvc.Models
{
    public class Driver
    {
        
        [Key]
        public int DriverId { get; set; }

        public string DriverIdentity { get; set; }

        public string Name { get; set; }

        public bool OnLine { get; set; } = false;

        public bool OnDelivery { get; set; } = false;

        public virtual ICollection<Order>? Orders { get; set; }
    }
}
