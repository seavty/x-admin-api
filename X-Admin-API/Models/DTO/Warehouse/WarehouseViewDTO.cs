using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.Warehouse
{
    public class WarehouseViewDTO
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string address { get; set; }
    }
}