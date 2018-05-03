using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.Invoice
{
    public class invoiceDetailListDTO : invoiceBase
    {
        public List<invoiceItemBase> results { get; set; }
    }
}