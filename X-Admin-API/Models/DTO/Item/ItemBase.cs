using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace X_Admin_API.Models.DTO.Item
{
    public class ItemBase
    {
        [JsonProperty(Order = 1)]
        public int? id { get; set; }

        [JsonProperty(Order = 2)]
        [Required]
        [MaxLength(100)]
        public string code { get; set; }

        [JsonProperty(Order = 3)]
        [Required]
        [MaxLength(100)]
        public string name { get; set; }

        [JsonProperty(Order = 4)]
        [MaxLength(500)]
        public string description { get; set; }
    }
}