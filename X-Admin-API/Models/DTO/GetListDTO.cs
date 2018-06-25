using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO
{
    public class GetListDTO<T>
    {
        public MetaData metaData { get; set; }
        public List<T> results { get; set; }
    }
}