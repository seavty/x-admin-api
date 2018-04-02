using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.SaleOrderItem
{
    public class SaleOrderItemBase
    {
        [Required]
        public int itemID { get; set; }

        public String description { get; set; }

        [Required]
        public int quantity { get; set; }

        [Required]
        public double price { get; set; }
    }
}