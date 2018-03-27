using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using X_Admin_API.Models.DTO.ItemGroup;

namespace X_Admin_API.Models.DTO.Item
{
    public class ItemHasItemGroupBase : ItemBase
    {
        [JsonProperty(Order = 10)]
        [Required]
        public ItemGroupBase itemGroup { get; set; }
    }
}