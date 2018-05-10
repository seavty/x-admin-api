using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using X_Admin_API.Models.DTO.SaleOrderItem;

namespace X_Admin_API.Models.DTO.SaleOrder
{
    public class SaleOrderNewDTO : SaleOrderBase
    {
        [Required]
        public int customerID { get; set; }

        [Required]
        public int warehouseID { get; set; }

        [Required]
        public String discountType { get; set; }

        [Required]
        public double amount { get; set; }

        [Required]
        public List<SaleOrderItemBase> items { get; set; }
    }
}