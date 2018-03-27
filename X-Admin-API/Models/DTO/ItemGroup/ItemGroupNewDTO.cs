using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.ItemGroup
{
    public class ItemGroupNewDTO : ItemGroupBase
    {
        public List<string> images { get; set; }
    }
}