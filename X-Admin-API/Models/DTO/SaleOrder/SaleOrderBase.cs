using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using X_Admin_API.Models.DTO.SaleOrderItem;

namespace X_Admin_API.Models.DTO.SaleOrder
{
    public class SaleOrderBase
    {
        [JsonProperty(Order = 1)]
        [Required]
        public int? id { get; set; }

        /*
        [Required]
        public int customerID { get; set; }

        [Required]
        public int warehouseID { get; set; }

        [Required]
        public List<SaleOrderItemBase> items { get; set; }
        */
        
    }
}