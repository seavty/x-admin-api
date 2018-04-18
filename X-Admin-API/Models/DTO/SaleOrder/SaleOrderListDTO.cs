using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.SaleOrder
{
    public class SaleOrderListDTO
    {
        public MetaData metaData { get; set; }
        public List<SaleOrderViewDTO> results { get; set; }
    }
}