using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using X_Admin_API.Models.DTO;

namespace X_Admin_API.Models.DTO.ItemGroup
{
    public class ItemGroupBase
    {
        [JsonProperty(Order = 1)]
        public int? id { get; set; }

        [JsonProperty(Order = 2)]
        [Required]
        [MaxLength(100)]
        public string name { get; set; }

    }
}