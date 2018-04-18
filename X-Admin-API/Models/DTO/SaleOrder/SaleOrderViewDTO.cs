using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using X_Admin_API.Models.DTO.Customer;

namespace X_Admin_API.Models.DTO.SaleOrder
{
    public class SaleOrderViewDTO : SaleOrderBase
    {
        [JsonProperty(Order = 2)]
        public String saleOrderNo { get; set; }

        [JsonProperty(Order = 3)]
        public double total { get; set; }

        [JsonProperty(Order = 4)]
        public CustomerViewDTO customer { get; set; }
    }
}