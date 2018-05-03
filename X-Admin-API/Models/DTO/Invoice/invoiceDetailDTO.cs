using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.Invoice
{
    public class invoiceDetailDTO
    {
        public MetaData metaData { get; set; }
        public List<invoiceDetailListDTO> results { get; set; }
    }
}