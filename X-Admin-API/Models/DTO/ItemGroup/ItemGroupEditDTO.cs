using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.ItemGroup
{
    public class ItemGroupEditDTO : ItemGroupBase
    {
        public List<string> images { get; set; }
        public List<string> deleteDocuments { get; set; }
    }
}