using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.Item
{
    public class ItemEditDTO : ItemHasItemGroupBase
    {
        [Required]
        public double price { get; set; }
        public List<string> images { get; set; }
        public List<string> deleteDocuments { get; set; }
    }
}