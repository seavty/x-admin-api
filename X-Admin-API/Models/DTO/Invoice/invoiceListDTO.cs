using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.Invoice
{
    public class invoiceListDTO
    {
        public MetaData metaData { get; set; }
        public List<invoiceBase> results { get; set; }
    }
}